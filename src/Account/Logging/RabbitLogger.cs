using Account.EventBus;
using Account.Models;
using Newtonsoft.Json;

namespace Account.Logging
{
    public class RabbitLogger : ILogger
    {
        private readonly string _exchange;
        private readonly IBus _bus;
        public RabbitLogger(
        string exchange,
        IBus bus) =>
        (_exchange, _bus) = (exchange, bus);

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel) => _bus != null;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var log = new Log
            {
                Code = eventId.Id,
                LogLevel = logLevel,
                Message = state?.ToString() ?? JsonConvert.SerializeObject(state)
            };
            _bus?.SendExchangeAsync(_exchange, log, logLevel.ToString().ToLower());
        }
    }
}
