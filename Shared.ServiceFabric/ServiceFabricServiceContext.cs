namespace Shared.ServiceFabric
{
    using System;
    using System.Fabric;
    using System.Globalization;
    using System.Threading;
    public class ServiceFabricServiceContext
    {
        private static readonly AsyncLocal<ServiceFabricServiceContext> current = new AsyncLocal<ServiceFabricServiceContext>();

        internal ServiceFabricServiceContext()
        {
        }

        public static ServiceFabricServiceContext Current
        {
            get => current.Value;

            internal set => current.Value = value;
        }

        public Uri Uri { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string PartitionId { get; set; }
        public string InstanceId { get; set; }
        public string ReplicaId { get; set; }

        public static void Set(ServiceContext context)
        {
            var sfServiceContext = new ServiceFabricServiceContext
            {
                Type = context.ServiceTypeName,
                Name = ServiceFabricHelper.GetServiceName(context.ServiceName),
                Uri = context.ServiceName,
                PartitionId = context.PartitionId.ToString()
            };

            if(context is StatelessServiceContext)
            {
                sfServiceContext.InstanceId = context.ReplicaOrInstanceId.ToString(CultureInfo.InvariantCulture);
            }

            if(context is StatefulServiceContext)
            {
                sfServiceContext.ReplicaId = context.ReplicaOrInstanceId.ToString(CultureInfo.InvariantCulture);
            }

            Current = sfServiceContext;
        }
    }
}
