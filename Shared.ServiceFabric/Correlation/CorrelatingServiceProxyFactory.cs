namespace Shared.ServiceFabric.Correlation
{
    using System;
    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Communication.Client;
    using Microsoft.ServiceFabric.Services.Remoting;
    using Microsoft.ServiceFabric.Services.Remoting.Client;
    using Microsoft.ServiceFabric.Services.Remoting.V1;
    using Microsoft.ServiceFabric.Services.Remoting.V1.Client;

    public class CorrelatingServiceProxyFactory : IServiceProxyFactory
    {
        private MethodNameProvider methodNameProvider;
        private ServiceProxyFactory serviceProxyFactory;

        public CorrelatingServiceProxyFactory(Func<IServiceRemotingCallbackClient, IServiceRemotingClientFactory> createServiceRemotingClientFactory = null, OperationRetrySettings retrySettings = null)
        {
            methodNameProvider = new MethodNameProvider(true /* threadSafe */);

            // Layer the factory structure so the hierarchy will look like this:
            // CorrelatingServiceProxyFactory
            //  --> ServiceProxyFactory
            //      --> CorrelatingServiceRemotingFactory
            //          --> <Factory created by createServcieRemotingClientFactory>
            serviceProxyFactory = new ServiceProxyFactory(
                callbackClient =>
                {
                    IServiceRemotingClientFactory innerClientFactory = createServiceRemotingClientFactory(callbackClient);
                    return new CorrelatingServiceRemotingClientFactory(innerClientFactory, methodNameProvider);
                },
                retrySettings);
        }

        public TServiceInterface CreateServiceProxy<TServiceInterface>(Uri serviceUri, ServicePartitionKey partitionKey = null, TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default, string listenerName = null)
            where TServiceInterface : IService
        {
            TServiceInterface proxy = serviceProxyFactory.CreateServiceProxy<TServiceInterface>(serviceUri, partitionKey, targetReplicaSelector, listenerName);
            methodNameProvider.AddMethodsForProxyOrService(proxy.GetType().GetInterfaces(), typeof(IService));
            return proxy;
        }
    }
}
