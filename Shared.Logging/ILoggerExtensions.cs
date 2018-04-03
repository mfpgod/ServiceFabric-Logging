namespace Shared.Logging
{
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;

    public static class ILoggerExtensions
    {
        public static void LogRequest(this ILogger logger, RequestLog request, Activity activity = null)
        {
            if (activity != null)
            {
                // id - this is to support the hierarchical ids
                request.Id = activity.Id;

                // timing
                request.StartTime = activity.StartTimeUtc;
                request.Duration = activity.Duration;

                // set context
                request.OperationId = activity.RootId;
                request.OperationParentId = activity.ParentId;
            }

            logger.LogInformation("Request Call: {request}", request);
        }

        public static void LogDependency(this ILogger logger, DependencyLog dependency, Activity activity = null)
        {
            // id - this is to support the hierarchical ids
            dependency.Id = activity.Id;

            // timing
            dependency.StartTime = activity.StartTimeUtc;
            dependency.Duration = activity.Duration;

            // context
            dependency.OperationId = activity.RootId;
            dependency.OperationParentId = activity.ParentId;

            logger.LogInformation("Dependency Call: {dependency}", dependency);
        }
    }
}
