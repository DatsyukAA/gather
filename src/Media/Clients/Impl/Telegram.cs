using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Media.Clients.Impl
{
    public class Telegram : IClient, IDisposable
    {
        public ITelegramBotClient Client { get; }
        public User? Me { get; private set; }
        private ILogger<Telegram>? Logger { get; }

        private readonly Dictionary<string, IList<Action<Models.Message>>> _actions = new();
        public Telegram(IConfiguration configuration, ILogger<Telegram> logger)
        {
            Client = new TelegramBotClient(configuration["Telegram:Token"]);
            Logger = logger;
        }

        public Telegram(string token)
        {
            Client = new TelegramBotClient(token);
            Logger?.LogInformation($"[{GetType().Name}] Client created.");
        }

        public void Dispose()
        {
            Logger?.LogInformation($"[{GetType().Name}] Client disposed.");
        }

        public async Task Publish(string channel, Models.Message message, CancellationToken? token = null)
        {
            var targetResult = int.TryParse(message.Target, out var target);
            await Client.SendTextMessageAsync(channel, message.Text, replyToMessageId: targetResult ? target : null);
            Logger?.LogInformation($"[{GetType().Name}][Sent][{channel}] {message.Text}");
        }

        public async Task Start(CancellationToken? token = null)
        {
            var cts = new CancellationTokenSource();
            Me = await Client.GetMeAsync();
            ReceiverOptions receiverOptions = new() { AllowedUpdates = { } };
            Logger?.LogInformation($"[{GetType().Name}] Client started.");
            Client.StartReceiving(
                                async (bot, update, token) =>
                                {
                                    Task handler = update.Type switch
                                    {
                                        UpdateType.Message => BotOnMessageReceived(bot, update.Message!),
                                        UpdateType.EditedMessage => BotOnMessageReceived(bot, update.EditedMessage!),
                                        _ => Task.Run(() =>
                                                Logger?.LogWarning($"called unsupported update type $0", update.Type), token)
                                    };

                                    try
                                    {
                                        await handler;
                                    }
                                    catch (Exception exception)
                                    {
                                        Logger?.LogError(message: exception.Message);
                                    }
                                },
                               async (bot, exception, token) =>
                               {
                                   Logger?.LogError(message: exception.Message);
                                   await Task.CompletedTask;
                               },
                               receiverOptions,
                               token ?? cts.Token);
        }

        private Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            var chId = message.Chat.Id.ToString() ?? "undefined";

            var channelActions = _actions.ContainsKey(chId) ? _actions[chId].Concat(_actions["public"]) : _actions["public"];

            foreach (var action in channelActions)
            {
                //TODO: add attachments
                action.Invoke(new Models.Message
                {
                    MediaService = GetType().Name.ToLower(),
                    Id = message?.MessageId.ToString() ?? "undefined",
                    Sender = message?.From?.ToString() ?? "undefined",
                    Channel = chId ?? "undefined",
                    Target = message?.ReplyToMessage?.MessageId.ToString() ?? "undefined",
                    Text = message?.Text?.ToString() ?? "undefined",
                    Attachments = ""
                });
            }
            return Task.CompletedTask;
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
    }
}
