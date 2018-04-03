namespace Adapters.ServiceBusDependency
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Services.Contracts.Adapters.ServiceBusDependency;
    using Shared.Config;
    using Shared.Logging;
    using Shared.ServiceBus;
    using Shared.ServiceFabric;

    internal sealed class ServiceBusDependency : StatelessService, IServiceBusDependency
    {
        private ServiceBusClient sbSenderClient;

        public ServiceBusDependency(StatelessServiceContext context)
            : base(context)
        {
            var sbConnectionString = ConfigurationManager.AppSettings.GetRefValue<string>("ServiceBusConnectionString");
            var sbQueueName = ConfigurationManager.AppSettings.GetRefValue<string>("ServiceBusQueueName");

            sbSenderClient = new ServiceBusClient(sbConnectionString, sbQueueName);
        }

        public async Task QueueRequest(QueueRequest queueRequest)
        {
            await sbSenderClient.SendAsync(queueRequest);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[1]
            {
                ServiceFabricHelper.CreateServiceInstanceListenerWithCorrelation(this)
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                // init service fabric context
                ServiceFabricServiceContext.Set(Context);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LoggingContext.CreateLogger<ServiceBusDependency>()
                    .LogError(ex, "Service exception in RunAsync");
            }
        }
    }
}
