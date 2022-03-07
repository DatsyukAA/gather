using RabbitMQ.Client;

namespace Account.EventBus
{
    public class Rabbit
    {
        private static ConnectionFactory? _factory;
        private static IConnection? _connection;
        public static IBus CreateBus(string hostName)
        {
            _factory = new ConnectionFactory
            {
                HostName = hostName,
                DispatchConsumersAsync = true
            };
            _connection = _factory.CreateConnection();
            return new RabbitBus(_connection);
        }
        public static IBus StubBus()
        {
            return new RabbitBus(null);
        }

        public static IBus CreateBus(
        string hostName,
        ushort hostPort,
        string virtualHost,
        string username,
        string password)
        {
            _factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = hostPort,
                VirtualHost = virtualHost,
                UserName = username,
                Password = password,
                DispatchConsumersAsync = true,
            };

            _connection = _factory.CreateConnection();
            return new RabbitBus(_connection);
        }
    }
}
