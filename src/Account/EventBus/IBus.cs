namespace Account.EventBus
{
    public interface IBus
    {
        Task SendQueueAsync<T>(string queue, T message);
        Task SendExchangeAsync<T>(string exchange, T message, string exchangeType = "direct");
        Task ReceiveAsync<T>(string queue, Action<T?> onMessage);
    }
}
