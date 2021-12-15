using Account.EventBus;

namespace Account.Logging
{
    public class RabbitLoggerOptions
    {
        public string Exchange;
        public IBus Bus;
    }
}
