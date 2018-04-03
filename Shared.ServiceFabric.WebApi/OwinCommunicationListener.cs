namespace Shared.ServiceFabric.WebApi
{
    using System;
    using System.Fabric;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Owin.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using NuGet;

    public class OwinCommunicationListener : ICommunicationListener
    {
        private readonly ServiceContext serviceContext;
        private readonly string appRoot;
        private readonly WebApiOwinServiceInitializer initializer;

        private IDisposable serverHandle;
        private string publishAddress;
        private string listeningAddress;

        public OwinCommunicationListener(ServiceContext serviceContext, WebApiOwinServiceInitializer initializer, string appRoot = null)
        {
            if (initializer.ServiceEndpointName == null)
            {
                throw new ArgumentNullException(nameof(initializer.ServiceEndpointName));
            }

            this.serviceContext = serviceContext;
            this.initializer = initializer;
            this.appRoot = appRoot;
        }

        public virtual string FileServerRootFolderRelativePath
        {
            get { return null; }
        }

        public bool ListenOnSecondary { get; set; }

        protected PhysicalFileSystem PhysicalFileSystem { get; set; }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            StopWebServer();

            return Task.FromResult(true);
        }

        public void Abort()
        {
            StopWebServer();
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            System.Fabric.Description.EndpointResourceDescription serviceEndpoint = serviceContext.CodePackageActivationContext.GetEndpoint(initializer.ServiceEndpointName);
            var port = serviceEndpoint.Port;

            var statefulServiceContext = serviceContext as StatefulServiceContext;
            if (statefulServiceContext != null)
            {
                listeningAddress = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}://+:{1}/{2}{3}/{4}/{5}",
                    serviceEndpoint.Protocol,
                    port,
                    string.IsNullOrWhiteSpace(appRoot) ? string.Empty : appRoot.TrimEnd('/') + '/',
                    statefulServiceContext.PartitionId,
                    statefulServiceContext.ReplicaId,
                    Guid.NewGuid());
            }
            else
            {
                listeningAddress = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}://+:{1}/{2}",
                    serviceEndpoint.Protocol,
                    port,
                    string.IsNullOrWhiteSpace(appRoot) ? string.Empty : appRoot.TrimEnd('/') + '/');
            }

            serverHandle = WebApp.Start(listeningAddress, (appBuilder) =>
            {
                initializer.OwinStartup.Configuration(appBuilder, serviceContext);
            });
            publishAddress = listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            return Task.FromResult(publishAddress);
        }

        private void StopWebServer()
        {
            if (serverHandle != null)
            {
                try
                {
                    serverHandle.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }
}
