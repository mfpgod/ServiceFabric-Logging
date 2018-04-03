namespace Shared.ServiceFabric.ApplicationInsights
{
    using System;
    using System.Fabric;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;

    public class ApplicationContextTelemetryInitializer : ITelemetryInitializer
    {
        public const string NodeName = "ServiceFabric.NodeName";
        public const string ApplicationTypeName = "ServiceFabric.ApplicationTypeName";
        public const string ApplicationName = "ServiceFabric.ApplicationName";

        private static Lazy<ServiceFabricApplicationContext> context = new Lazy<ServiceFabricApplicationContext>(() =>
        {
            // todo: migrate to async methods
            CodePackageActivationContext sfActivationContext = FabricRuntime.GetActivationContext();
            NodeContext sfNodeContext = FabricRuntime.GetNodeContext();

            return new ServiceFabricApplicationContext
            {
                NodeName = sfNodeContext.NodeName,
                Name = sfActivationContext.ApplicationName,
                Type = sfActivationContext.ApplicationTypeName,
                Version = sfActivationContext.CodePackageVersion
            };
        });

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry == null)
            {
                throw new ArgumentNullException(nameof(telemetry));
            }

            if (context.Value != null)
            {
                if(!telemetry.Context.Properties.ContainsKey(NodeName))
                {
                    telemetry.Context.Properties.Add(NodeName, context.Value.NodeName);
                }

                if (!telemetry.Context.Properties.ContainsKey(ApplicationTypeName))
                {
                    telemetry.Context.Properties.Add(ApplicationTypeName, context.Value.Type);
                }

                if (!telemetry.Context.Properties.ContainsKey(ApplicationName))
                {
                    telemetry.Context.Properties.Add(ApplicationName, context.Value.Name);
                }

                telemetry.Context.Component.Version = context.Value.Version;
            }
        }

        internal class ServiceFabricApplicationContext
        {
            public string NodeName { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Version { get; set; }
        }
    }
}
