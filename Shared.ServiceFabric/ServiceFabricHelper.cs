namespace Shared.ServiceFabric
{
    using System;
    using System.Configuration;
    using System.Linq;
    using global::Shared.Config;
    using Microsoft.ServiceFabric.Services.Communication.Client;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting;
    using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
    using Microsoft.ServiceFabric.Services.Remoting.V1.FabricTransport.Client;
    using Microsoft.ServiceFabric.Services.Remoting.V1.FabricTransport.Runtime;
    using Shared.ServiceFabric.Correlation;

    public static class ServiceFabricHelper
    {
        public static readonly string RemotingTypeName = "Remoting";

        public static string GetServiceName(Uri serviceUri)
        {
            return serviceUri.AbsoluteUri.Split('/').Last();
        }

        public static string GetApplicationName(Uri serviceUri)
        {
            return serviceUri.AbsoluteUri.Split('/').Reverse().Skip(1).FirstOrDefault();
        }

        public static Uri MakeOperationUri(Uri serviceUri, string methodName)
        {
            return new Uri($"{serviceUri.AbsoluteUri}/{methodName}");
        }

        public static string MakeOperationName(Uri serviceUri, string methodName)
        {
            return $"{GetServiceName(serviceUri)}/{methodName}";
        }

        public static T CreateServiceProxyWithCorrelation<T>(string serviceAddress)
            where T : IService
        {
            var sfrRetryInterval = TimeSpan.Parse(ConfigurationManager.AppSettings.GetRefValue<string>("ServiceFabricRemoting_OperationRetryInterval"));
            var sfrRetryCount = int.Parse(ConfigurationManager.AppSettings.GetRefValue<string>("ServiceFabricRemoting_OperationRetryCount"));
            var sfrTimeout = TimeSpan.Parse(ConfigurationManager.AppSettings.GetRefValue<string>("ServiceFabricRemoting_OperationTimeout"));

            var fabricTransportRemotingSettings = new FabricTransportRemotingSettings
            {
                OperationTimeout = sfrTimeout
            };

            var operationRetrySettings = new OperationRetrySettings(sfrRetryInterval, sfrRetryInterval, sfrRetryCount);

            // read https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-services-communication-remoting
            // _dependencyClient = ServiceProxy.Create<IDependency>(new Uri(serviceAddress));

            var proxyFactory = new CorrelatingServiceProxyFactory(
                callbackClient => new FabricTransportServiceRemotingClientFactory(
                callbackClient: callbackClient,
                FabricTransportRemotingSettings: fabricTransportRemotingSettings), retrySettings: operationRetrySettings);
            return proxyFactory.CreateServiceProxy<T>(new Uri(serviceAddress));
        }

        public static ServiceInstanceListener CreateServiceInstanceListenerWithCorrelation(IService service)
        {
            return new ServiceInstanceListener(context => new FabricTransportServiceRemotingListener(context, new CorrelatingRemotingMessageHandler(context, service)));
        }
    }
}
