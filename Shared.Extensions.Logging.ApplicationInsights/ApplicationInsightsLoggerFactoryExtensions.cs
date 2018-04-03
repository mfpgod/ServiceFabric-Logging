namespace Shared.Extensions.Logging.ApplicationInsights
{
    using System;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extension methods for <see cref="ILoggerFactory"/> that allow adding Application Insights logger.
    /// </summary>
    public static class ApplicationInsightsLoggerFactoryExtensions
    {
        /// <summary>
        /// Adds an ApplicationInsights logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="serviceProvider">The instance of <see cref="IServiceProvider"/> to use for service resolution.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        public static ILoggerFactory AddApplicationInsights(
            this ILoggerFactory factory,
            Func<TelemetryClient> telemetryClientFactory,
            LogLevel minLevel)
        {
            factory.AddApplicationInsights(telemetryClientFactory, (category, logLevel) => logLevel >= minLevel);
            return factory;
        }

        /// <summary>
        /// Adds an ApplicationInsights logger that is enabled as defined by the filter function.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="filter"></param>
        /// <param name="serviceProvider">The instance of <see cref="IServiceProvider"/> to use for service resolution.</param>
        public static ILoggerFactory AddApplicationInsights(
            this ILoggerFactory factory,
            Func<TelemetryClient> telemetryClientFactory,
            Func<string, LogLevel, bool> filter)
        {
            factory.AddProvider(new ApplicationInsightsLoggerProvider(telemetryClientFactory, filter));
            return factory;
        }
    }
}
