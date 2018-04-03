namespace Shared.ServiceFabric.ApplicationInsights
{
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;

    public class ServiceContextTelemetryInitializer : ITelemetryInitializer
    {
        public const string ServiceTypeName = "ServiceFabric.ServiceTypeName";
        public const string ServiceName = "ServiceFabric.ServiceName";
        public const string ServiceUri = "ServiceFabric.ServiceUri";

        public const string PartitionId = "ServiceFabric.PartitionId";
        public const string InstanceId = "ServiceFabric.InstanceId";
        public const string ReplicaId = "ServiceFabric.ReplicaId";

        public void Initialize(ITelemetry telemetry)
        {
            ServiceFabricServiceContext sfServiceContext = ServiceFabricServiceContext.Current;

            if (sfServiceContext != null)
            {
                if (sfServiceContext.Type != null && !telemetry.Context.Properties.ContainsKey(ServiceTypeName))
                {
                    telemetry.Context.Properties.Add(ServiceTypeName, sfServiceContext.Type);
                }

                if (sfServiceContext.Name != null && !telemetry.Context.Properties.ContainsKey(ServiceName))
                {
                    telemetry.Context.Properties.Add(ServiceName, sfServiceContext.Name);
                }

                if (sfServiceContext.Uri != null && !telemetry.Context.Properties.ContainsKey(ServiceUri))
                {
                    telemetry.Context.Properties.Add(ServiceUri, sfServiceContext.Uri.ToString());
                }

                if (sfServiceContext.InstanceId != null && !telemetry.Context.Properties.ContainsKey(InstanceId))
                {
                    telemetry.Context.Properties.Add(InstanceId, sfServiceContext.InstanceId);
                }

                if (sfServiceContext.ReplicaId != null && !telemetry.Context.Properties.ContainsKey(ReplicaId))
                {
                    telemetry.Context.Properties.Add(ReplicaId, sfServiceContext.ReplicaId);
                }

                if (sfServiceContext.PartitionId != null && !telemetry.Context.Properties.ContainsKey(PartitionId))
                {
                    telemetry.Context.Properties.Add(PartitionId, sfServiceContext.PartitionId);
                }
            }
        }
    }
}
