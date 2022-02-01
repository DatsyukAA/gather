using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;

namespace Media.Clients.Impl
{
    internal class Vk : IClient, IDisposable
    {
        private readonly ApiAuthParams authParams;

        public IConfiguration Configuration { get; }
        private ILogger<Vk> Logger { get; }
        private VkApi Instance { get; init; }
        private readonly Dictionary<string, IList<Action<Models.Message>>> _actions = new();

        public Vk(IConfiguration configuration, VkApi api, ILogger<Vk> logger)
        {
            Logger = logger;
            Configuration = configuration;
            Instance = api;
            authParams = new ApiAuthParams
            {
                AccessToken = Configuration["Vk:Token"]
            };
        }
        public async Task Start(CancellationToken? token = null)
        {
            Instance.Authorize(authParams);
            var longPoolServer = await Instance.Groups.GetLongPollServerAsync(ulong.Parse(Configuration["Vk:GroupId"]));
            Logger?.LogInformation($"[{GetType().Name}] Client started.");
            while (!(token?.IsCancellationRequested ?? false))
            {
                var pool = await Instance.Groups.GetBotsLongPollHistoryAsync(new VkNet.Model.RequestParams.BotsLongPollHistoryParams
                {
                    Server = longPoolServer.Server,
                    Key = longPoolServer.Key,
                    Ts = longPoolServer.Ts,
                    Wait = 1
                });
                if (pool?.Updates == null) continue;
                longPoolServer.Ts = pool.Ts;
                foreach (var update in pool.Updates)
                {
                    if (update.Type == GroupUpdateType.MessageNew)
                    {
                        var message = update?.MessageNew?.Message;
                        var chId = message?.PeerId?.ToString() ?? "undefined";

                        var channelActions = _actions.ContainsKey(chId) ? _actions[chId].Concat(_actions["public"]) : _actions["public"];

                        foreach (var action in channelActions)
                        {
                            action.Invoke(new Models.Message
                            {
                                MediaService = GetType().Name.ToLower(),
                                Id = message?.Id?.ToString() ?? "undefined",
                                Sender = message?.FromId?.ToString() ?? "undefined",
                                Channel = message?.PeerId?.ToString() ?? "undefined",
                                Target = message?.UserId?.ToString() ?? "undefined",
                                Text = message?.Text.ToString() ?? "undefined",
                                Attachments = string.Join(", ", message?.Attachments?.Select(x => x.Instance.Id.ToString()) ?? Array.Empty<string>())
                            });
                        }
                    }
                    else
                    {
                        Logger.LogWarning($"called unsupported update type $0", update.Type);
                    }
                }
            }
        }

        public async Task Publish(string channel, Models.Message message, CancellationToken? token = null)
        {
            try
            {
                var parameters = new VkNet.Model.RequestParams.MessagesSendParams
                {
                    PeerId = long.Parse(channel),
                    Message = message.Text,
                    RandomId = long.Parse(message.Id)
                };
                await Instance.Messages.SendAsync(parameters);
                Logger?.LogInformation($"[{GetType().Name}][Sent][{channel}] {message.Text}");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex.Message);
            }
        }

        public Task Subscribe(Action<Models.Message> action, string channel = "public", CancellationToken? token = null)
        {
            if (!_actions.ContainsKey(channel))
                _actions.Add(channel, new List<Action<Models.Message>> { action });
            else _actions[channel].Add(action);
            Logger?.LogInformation($"[{GetType().Name}] Added action to {channel}.");
            return Task.CompletedTask;
        }

        public Task Unsubscribe(string channel = "public", Action<Models.Message>? action = null, CancellationToken? token = null)
        {
            if (action == null)
            {
                _actions.Remove(channel);
                Logger?.LogInformation($"[{GetType().Name}] Cleared actions from {channel}.");
            }
            else
            {
                _actions[channel] = _actions[channel].Where(x => x != action).ToList();
                Logger?.LogInformation($"[{GetType().Name}] Removed action from {channel}.");
            }

            return Task.CompletedTask;
        }
        public void Dispose()
        {
        }
    }
}
