using DSharpPlus;
using Media.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Media.Clients.Impl
{
    public class Discord : IClient, IDisposable
    {
        private DiscordClient Instance { get; init; }
        private readonly Dictionary<string, IList<Action<Message>>> _actions = new();
        private readonly ILogger<Discord>? _logger;

        public Discord(IConfiguration conf, ILoggerFactory? loggerFactory = null)
        {
            _logger = loggerFactory?.CreateLogger<Discord>();
            Instance = new DiscordClient(new DiscordConfiguration
            {
                Token = conf["Discord:Token"],
                TokenType = TokenType.Bot,
                LoggerFactory = loggerFactory
            });
        }

        public async Task Publish(string channel, Message message, CancellationToken? token = null)
        {
            var parseResult = ulong.TryParse(channel, out var id);
            if (!parseResult) return;
            var ch = await Instance.GetChannelAsync(id);
            var mess = $"<@{message.Sender}> send: {string.Join(" ", message.Target.Split(", ").Select(x => $"<@{x}>"))}{message.Text}";
            await Instance.SendMessageAsync(ch, mess);
            _logger?.LogInformation($"[{GetType().Name}][Sent][{ch}] {mess}");
        }

        public Task Subscribe(Action<Message> action, string channel = "public", CancellationToken? token = null)
        {
            if (!_actions.ContainsKey(channel))
                _actions.Add(channel, new List<Action<Message>> { action });
            else _actions[channel].Add(action);
            _logger?.LogInformation($"[{GetType().Name}] Added action to {channel}.");
            return Task.CompletedTask;
        }

        public Task Unsubscribe(string channel = "public", Action<Message>? action = null, CancellationToken? token = null)
        {
            if (action == null)
            {
                _actions.Remove(channel);
                _logger?.LogInformation($"[{GetType().Name}] Cleared actions from {channel}.");
            }
            else
            {
                _actions[channel] = _actions[channel].Where(x => x != action).ToList();
                _logger?.LogInformation($"[{GetType().Name}] Removed action from {channel}.");
            }

            return Task.CompletedTask;
        }
        public void Dispose()
        {
            Instance.MessageCreated -= Discord_MessageCreated;
            _logger?.LogInformation($"[{GetType().Name}] Client disposed.");
        }

        public async Task Start(CancellationToken? token = null)
        {
            await Instance.ConnectAsync();
            Instance.MessageCreated += Discord_MessageCreated;
            _logger?.LogInformation($"[{GetType().Name}] Client started.");
        }

        private Task Discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            var chId = e.Channel.Id.ToString();
            var channelActions = _actions.ContainsKey(chId) ? _actions[chId].Concat(_actions["public"]) : _actions["public"];

            foreach (var action in channelActions)
            {
                action.Invoke(new Message
                {
                    MediaService = GetType().Name.ToLower(),
                    Id = e.Message.Id.ToString(),
                    Sender = e.Message.Author.Id.ToString(),
                    Channel = e.Channel.Id.ToString(),
                    Target = e.Message.Reference != null ? e.Message.Reference.Message.Author.Id.ToString() :
                        string.Join(", ",
                            e.Message.MentionedUsers?.Select(x => x.Id.ToString()) ??
                            new string[1] { e.Channel.Id.ToString() }.ToArray() ??
                            Array.Empty<string>()),
                    Text = e.Message.Content.ToString(),
                    Attachments = string.Join(", ", e.Message.Attachments.Select(x => x.Url))
                });
            }

            return Task.CompletedTask;
        }
    }
}
