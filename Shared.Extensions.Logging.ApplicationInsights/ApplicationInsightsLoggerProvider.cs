namespace Shared.Extensions.Logging.ApplicationInsights
{
    using System;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// <see cref="ILoggerProvider"/> Implementation that creates returns instances of <see cref="ApplicationInsightsLogger"/>
    /// </summary>
    internal class ApplicationInsightsLoggerProvider : ILoggerProvider
    {
        private readonly Func<TelemetryClient> telemetryClientFactory;
        private readonly Func<string, LogLevel, bool> filter;

        public ApplicationInsightsLoggerProvider(Func<TelemetryClient> telemetryClientFactory, Func<string, LogLevel, bool> filter)
        {
            this.telemetryClientFactory = telemetryClientFactory;
            this.filter = filter;
        }

        public ILogger CreateLogger(string categoryName)
        {
            TelemetryClient telemetryClient = telemetryClientFactory();
            return new ApplicationInsightsLogger(categoryName, telemetryClient, filter);
        }

        public void Dispose()
        {
        }
    }
}
