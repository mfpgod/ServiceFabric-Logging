namespace Shared.ServiceFabric.WebApi
{
    using System.Fabric;
    using System.Threading.Tasks;
    using Microsoft.Owin;

    public class ServiceFabricContextOwinMiddleware : OwinMiddleware
    {
        private readonly ServiceContext serviceContext;

        public ServiceFabricContextOwinMiddleware(
            OwinMiddleware next,
            ServiceContext serviceContext)
            : base(next)
        {
            this.serviceContext = serviceContext;
        }

        public override async Task Invoke(IOwinContext context)
        {
            ServiceFabricServiceContext.Set(this.serviceContext);

            await Next.Invoke(context);
        }
    }
}
