namespace Services.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Shared.Logging;
    using Shared.ServiceFabric;
    using Shared.ServiceFabric.WebApi;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WebApi : StatelessService
    {
        public WebApi(StatelessServiceContext context)
            : base(context)
        {
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            // http://localhost/webapp/api/values
            return new[]
            {
                new ServiceInstanceListener(serviceContext => new OwinCommunicationListener(serviceContext, new WebApiOwinServiceInitializer(new Startup()), "webapp"))
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
                LoggingContext.CreateLogger<WebApi>()
                    .LogError(ex, "Service exception in RunAsync");
            }
        }
    }
}
