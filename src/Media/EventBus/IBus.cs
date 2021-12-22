namespace Media.EventBus
{
    public interface IBus
    {
        Task SendQueueAsync<T>(string queue, T message);
        Task SendExchangeAsync<T>(string exchange, T message, string? routingKey = null, string exchangeType = "direct");
        Task ReceiveAsync<T>(string queue, Action<T?> onMessage, string? exchange = null, string? exchangeType = null, string? routingKey = null);
    }
}
