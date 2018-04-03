namespace Shared.ServiceFabric.Correlation
{
    using System;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Communication.Client;
    using Microsoft.ServiceFabric.Services.Remoting.V1.Client;

    /// <summary>
    /// Base class for remoting client factory for constructing clients that can call other Service Fabric services. Clients created by this factory pass correlation ids and relevant information
    /// to the callee so diagnostic traces can be tagged with the relevant ids. This factory wraps and use an instance of <see cref="IServiceRemotingClientFactory"/> for most of the
    /// underying functionality. <see cref="CorrelatingServiceRemotingClientFactory"/> calls <see cref="IServiceRemotingClientFactory"/> to create an inner client, which
    /// handles the main call transport and will be wrapped by a <see cref="CorrelatingServiceRemotingClient"/> object.
    /// </summary>
    internal class CorrelatingServiceRemotingClientFactory : IServiceRemotingClientFactory
    {
        private readonly IServiceRemotingClientFactory innerClientFactory;
        private readonly IMethodNameProvider methodNameProvider;

        public CorrelatingServiceRemotingClientFactory(IServiceRemotingClientFactory innerClientFactory, IMethodNameProvider methodNameProvider)
        {
            this.methodNameProvider = methodNameProvider ?? throw new ArgumentNullException(nameof(methodNameProvider));

            this.innerClientFactory = innerClientFactory ?? throw new ArgumentNullException(nameof(innerClientFactory));
            this.innerClientFactory.ClientConnected += ClientConnected;
            this.innerClientFactory.ClientDisconnected += ClientDisconnected;
        }

        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientConnected;

        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientDisconnected;

        public async Task<IServiceRemotingClient> GetClientAsync(
            Uri serviceUri,
            ServicePartitionKey partitionKey,
            TargetReplicaSelector targetReplicaSelector,
            string listenerName,
            OperationRetrySettings retrySettings,
            CancellationToken cancellationToken)
        {
            IServiceRemotingClient innerClient = await innerClientFactory.GetClientAsync(serviceUri, partitionKey, targetReplicaSelector, listenerName, retrySettings, cancellationToken).ConfigureAwait(false);
            return new CorrelatingServiceRemotingClient(innerClient, serviceUri, methodNameProvider);
        }

        public async Task<IServiceRemotingClient> GetClientAsync(ResolvedServicePartition previousRsp, TargetReplicaSelector targetReplicaSelector, string listenerName, OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            IServiceRemotingClient innerClient = await this.innerClientFactory.GetClientAsync(previousRsp, targetReplicaSelector, listenerName, retrySettings, cancellationToken).ConfigureAwait(false);
            return new CorrelatingServiceRemotingClient(innerClient, previousRsp.ServiceName, methodNameProvider);
        }

        public Task<OperationRetryControl> ReportOperationExceptionAsync(IServiceRemotingClient client, ExceptionInformation exceptionInformation, OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            IServiceRemotingClient effectiveClient = (client as IWrappingClient)?.InnerClient ?? client;
            return innerClientFactory.ReportOperationExceptionAsync(effectiveClient, exceptionInformation, retrySettings, cancellationToken);
        }
    }
}
