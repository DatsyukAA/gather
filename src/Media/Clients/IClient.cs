using Media.Models;

namespace Media.Clients
{
    public interface IClient
    {
        public Task Publish(string channel, Message message, CancellationToken? token);
        public Task Subscribe(Action<Message> action, string channel = "public", CancellationToken? token = null);
        public Task Unsubscribe(string channel = "public", Action<Message>? action = null, CancellationToken? token = null);
        public Task Start(CancellationToken? token);
    }
}
