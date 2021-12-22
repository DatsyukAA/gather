using Media.EventBus;

namespace Media.Logging
{
    public class RabbitLoggerOptions
    {
        public string Exchange;
        public IBus Bus;
    }
}
