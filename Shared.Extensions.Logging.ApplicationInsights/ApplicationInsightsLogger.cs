namespace Shared.Extensions.Logging.ApplicationInsights
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Extensions.Logging;
    using Shared.Logging;

    /// <summary>
    /// <see cref="ILogger"/> Implementation that forwards log messages as Application Insight trace events.
    /// </summary>
    internal class ApplicationInsightsLogger : ILogger
    {
        private readonly string categoryName;
        private readonly TelemetryClient telemetryClient;
        private readonly Func<string, LogLevel, bool> filter;

        /// <summary>
        /// Creates a new instance of <see cref="ApplicationInsightsLogger"/>
        /// </summary>
        public ApplicationInsightsLogger(string name, TelemetryClient telemetryClient, Func<string, LogLevel, bool> filter)
        {
            categoryName = name;
            this.telemetryClient = telemetryClient;
            this.filter = filter;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return filter != null && telemetryClient != null && filter(categoryName, logLevel) && telemetryClient.IsEnabled();
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                var stateDictionary = state as IReadOnlyList<KeyValuePair<string, object>>;

                if (exception != null)
                {
                    var exceptionTelemetry = new ExceptionTelemetry(exception)
                    {
                        Message = formatter(state, exception),
                        SeverityLevel = GetSeverityLevel(logLevel)
                    };
                    PopulateTelemetry(exceptionTelemetry, stateDictionary);
                    telemetryClient.TrackException(exceptionTelemetry);
                    return;
                }

                var requestState = stateDictionary.FirstOrDefault(x => x.Key == Constants.Request).Value as RequestLog;
                if (requestState != null)
                {
                    var telemetry = new RequestTelemetry
                    {
                        Id = requestState.Id,
                        Name = requestState.Name,
                        // Type = requestState.Type,
                        Url = requestState.Uri,
                        Source = requestState.Source,

                        Timestamp = requestState.StartTime,
                        Duration = requestState.Duration,

                        ResponseCode = requestState.ResponseCode,
                        Success = requestState.Success
                    };
                    telemetry.Context.Operation.Id = requestState.OperationId;
                    telemetry.Context.Operation.Name = requestState.OperationName;
                    telemetry.Context.Operation.ParentId = requestState.OperationParentId;

                    telemetryClient.TrackRequest(telemetry);
                    return;
                }

                var dependencyState = stateDictionary.FirstOrDefault(x => x.Key == Constants.Response).Value as DependencyLog;
                if (dependencyState != null)
                {
                    var telemetry = new DependencyTelemetry
                    {
                        Id = dependencyState.Id,
                        Name = dependencyState.Name,
                        Type = dependencyState.Type,
                        Data = dependencyState.Uri?.ToString(),
                        Target = dependencyState.Target,

                        Timestamp = dependencyState.StartTime,
                        Duration = dependencyState.Duration,

                        ResultCode = dependencyState.ResponseCode,
                        Success = dependencyState.Success
                    };
                    telemetry.Context.Operation.Id = dependencyState.OperationId;
                    telemetry.Context.Operation.Name = dependencyState.OperationName;
                    telemetry.Context.Operation.ParentId = dependencyState.OperationParentId;

                    telemetryClient.TrackDependency(telemetry);
                    return;
                }

                var traceTelemetry = new TraceTelemetry(formatter(state, exception), GetSeverityLevel(logLevel));
                PopulateTelemetry(traceTelemetry, stateDictionary);
                telemetryClient.TrackTrace(traceTelemetry);
            }
        }

        private void PopulateTelemetry(ITelemetry telemetry, IReadOnlyList<KeyValuePair<string, object>> stateDictionary)
        {
            IDictionary<string, string> dict = telemetry.Context.Properties;
            dict["CategoryName"] = categoryName;
            if (stateDictionary != null)
            {
                foreach (KeyValuePair<string, object> item in stateDictionary)
                {
                    dict[item.Key] = Convert.ToString(item.Value);
                }
            }
        }

        private SeverityLevel GetSeverityLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return SeverityLevel.Critical;
                case LogLevel.Error:
                    return SeverityLevel.Error;
                case LogLevel.Warning:
                    return SeverityLevel.Warning;
                case LogLevel.Information:
                    return SeverityLevel.Information;
                case LogLevel.Debug:
                case LogLevel.Trace:
                default:
                    return SeverityLevel.Verbose;
            }
        }
    }
}
