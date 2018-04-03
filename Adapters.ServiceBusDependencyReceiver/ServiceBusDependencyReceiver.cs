namespace Adapters.ServiceBusDependencyReceiver
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
    using Services.Contracts.Adapters.DocumentDbDependency;
    using Services.Contracts.Adapters.ServiceBusDependency;
    using Shared.Config;
    using Shared.Logging;
    using Shared.ServiceBus;
    using Shared.ServiceFabric;

    internal sealed class ServiceBusDependencyReceiver : StatelessService
    {
        private ServiceBusClient sbReceiverClient;
        private ServiceBusClient sbSenderClient;

        private IDocumentDbDependency adapterDocumentDbDependencyClient;

        public ServiceBusDependencyReceiver(StatelessServiceContext context)
            : base(context)
        {
            var sbConnectionString = ConfigurationManager.AppSettings.GetRefValue<string>("ServiceBusConnectionString");
            var sbQueueName = ConfigurationManager.AppSettings.GetRefValue<string>("ServiceBusQueueName");
            var sbForwardQueueName = ConfigurationManager.AppSettings.GetRefValue<string>("ServiceBusForwardQueueName");

            sbReceiverClient = new ServiceBusClient(sbConnectionString, sbQueueName);
            sbSenderClient = new ServiceBusClient(sbConnectionString, sbForwardQueueName);

            string adapterDocumentDbDependencyAddress = ConfigurationManager.AppSettings.GetRefValue<string>("AdapterDocumentDbDependencyAddress");
            adapterDocumentDbDependencyClient = ServiceFabricHelper.CreateServiceProxyWithCorrelation<IDocumentDbDependency>(adapterDocumentDbDependencyAddress);
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0] { };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                // init service fabric context
                ServiceFabricServiceContext.Set(Context);

                // start a service bus receiver
                sbReceiverClient.Receive<QueueRequest>(ProcessMessagesAsync);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LoggingContext.CreateLogger<ServiceBusDependencyReceiver>()
                    .LogError(ex, "Service exception in RunAsync");
            }
        }

        private async Task<QueueItemProcessResult> ProcessMessagesAsync(QueueRequest request, IServiceBusClient sbSender, CancellationToken token)
        {
            try
            {
                // update user
                UserEntity dbUser = await adapterDocumentDbDependencyClient.GetUser(request.Id);
                dbUser.Processed = true;
                await adapterDocumentDbDependencyClient.UpdateUser(dbUser);

                await sbSenderClient.SendAsync(request);

                return QueueItemProcessResult.Complete;
            }
            catch (Exception ex)
            {
                LoggingContext.CreateLogger<ServiceBusDependencyReceiver>()
                    .LogError(
                        ex,
                        "{queue} item processing unhandled exception for type={type}, id={messageId}",
                        sbReceiverClient.QueueName,
                        request.GetType().Name,
                        request.Id);
                return QueueItemProcessResult.Retry;
            }
        }
    }
}
