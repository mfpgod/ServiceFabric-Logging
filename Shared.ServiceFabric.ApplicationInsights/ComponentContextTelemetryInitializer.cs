namespace Shared.ServiceFabric.ApplicationInsights
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;

    public class ComponentContextTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
            {
                ServiceFabricServiceContext sfServiceContext = ServiceFabricServiceContext.Current;
                if (sfServiceContext?.Name != null)
                {
                       telemetry.Context.Cloud.RoleName = sfServiceContext.Name;
                }
            }
        }
    }
}
