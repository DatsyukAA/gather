using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Account.Logging
{
    public class RabbitLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable _onChangeToken;
        private readonly ConcurrentDictionary<string, RabbitLogger> _loggers = new();
        private RabbitLoggerOptions options;

        public RabbitLoggerProvider(IOptionsMonitor<RabbitLoggerOptions> config)
        {
            options = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => options = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new RabbitLogger(options.Exchange, options.Bus));

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }
}
