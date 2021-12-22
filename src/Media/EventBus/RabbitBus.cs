using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Media.EventBus
{
    public class RabbitBus : IBus
    {
        private IConnection _connection;

        internal RabbitBus(IConnection conn)
        {
            _connection = conn;
        }
        public async Task SendQueueAsync<T>(string queue, T message)
        {
            await Task.Run(() =>
            {
                var _channel = _connection.CreateModel();
                _channel.QueueDeclare(queue, true, false, false);
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = false;
                var output = JsonConvert.SerializeObject(message);
                _channel.BasicPublish(string.Empty, queue, null, Encoding.UTF8.GetBytes(output));
            });
        }

        public async Task SendExchangeAsync<T>(string exchange, T message, string? routingKey = null, string exchangeType = "direct")
        {
            await Task.Run(() =>
            {
                var _channel = _connection.CreateModel();
                _channel.ExchangeDeclare(exchange, exchangeType);
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = false;
                var output = JsonConvert.SerializeObject(message);
                _channel.BasicPublish(exchange, routingKey ?? string.Empty, null, Encoding.UTF8.GetBytes(output));
            });
        }
        public async Task ReceiveAsync<T>(string queue, Action<T?> onMessage, string? exchange = null, string? exchangeType = null, string? routingKey = null)
        {
            var _channel = _connection.CreateModel();
            if (exchange != null)
            {
                _channel.ExchangeDeclare(exchange, exchangeType ?? "direct");
            }
            _channel.QueueDeclare(queue, true, false, false);

            if (exchange != null)
            {
                _channel.QueueBind(queue, exchange, routingKey ?? string.Empty);
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (s, e) =>
            {
                var jsonSpecified = Encoding.UTF8.GetString(e.Body.Span);
                var item = JsonConvert.DeserializeObject<T>(jsonSpecified);
                onMessage(item);
                await Task.Yield();
            };
            _channel.BasicConsume(queue, true, consumer);
            await Task.Yield();
        }
    }
}
