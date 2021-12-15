using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;

namespace Account.Logging
{
    public static class RabbitLoggerExtensions
    {
        public static ILoggingBuilder AddRabbitLogger(
            this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, RabbitLoggerProvider>());

            LoggerProviderOptions.RegisterProviderOptions
                <RabbitLoggerOptions, RabbitLoggerProvider>(builder.Services);

            return builder;
        }

        public static ILoggingBuilder AddRabbitLogger(
            this ILoggingBuilder builder,
            Action<RabbitLoggerOptions> configure)
        {
            builder.AddRabbitLogger();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}
