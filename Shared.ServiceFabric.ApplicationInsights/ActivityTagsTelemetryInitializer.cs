namespace Shared.ServiceFabric.ApplicationInsights
{
    using System.Diagnostics;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;

    public class ActivityTagsTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            Activity activity = Activity.Current;
            if (activity != null && activity.Tags != null)
            {
                foreach(System.Collections.Generic.KeyValuePair<string, string> tag in activity.Tags)
                {
                    if (!telemetry.Context.Properties.ContainsKey(tag.Key))
                    {
                        telemetry.Context.Properties.Add(tag);
                    }
                }
            }
        }
    }
}
