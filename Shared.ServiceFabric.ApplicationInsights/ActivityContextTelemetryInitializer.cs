namespace Shared.ServiceFabric.ApplicationInsights
{
    using System.Diagnostics;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Shared.Logging;

    public class ActivityContextTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            Activity activity = Activity.Current;
            if (Activity.Current != null)
            {
                if (string.IsNullOrEmpty(telemetry.Context.Operation.Id))
                {
                    telemetry.Context.Operation.Id = activity.RootId;
                }

                if (string.IsNullOrEmpty(telemetry.Context.Operation.ParentId))
                {
                    telemetry.Context.Operation.ParentId = activity.ParentId;
                }

                if (string.IsNullOrEmpty(telemetry.Context.Operation.Name))
                {
                    var rootOperationName = activity.GetBaggageItem(ActivityConstants.RootOperationName);
                    if (!string.IsNullOrEmpty(rootOperationName))
                    {
                        telemetry.Context.Operation.Name = rootOperationName;
                    }
                }
            }
        }
    }
}
