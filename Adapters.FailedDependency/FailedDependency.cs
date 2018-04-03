namespace Adapters.FailedDependency
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Services.Contracts.Adapters.FailedDependency;
    using Shared.Logging;
    using Shared.ServiceFabric;

    public class FailedDependency : StatelessService, IFailedDependency
    {
        public FailedDependency(StatelessServiceContext context)
            : base(context)
        {
        }

        public Task Fail()
        {
            throw new NotImplementedException("Method 'Fail' is not implemented");
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

                // throw new Exception("RunAsync Service Exception");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LoggingContext.CreateLogger<FailedDependency>()
                    .LogError(ex, "Service exception in RunAsync");
            }
        }
    }
}
