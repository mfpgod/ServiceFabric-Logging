namespace Services.WebApi
{
    using System.Fabric;
    using System.Web.Http;
    using Autofac;
    using Owin;
    using Shared.ServiceFabric.WebApi;

    public class Startup : IOwinAppBuilder
    {
        public void Configuration(IAppBuilder appBuilder, ServiceContext serviceContext)
        {
            var config = new HttpConfiguration();

            IContainer container = AutofacContainerConfig.ConfigureContainer(config, serviceContext);

            WebApiConfig.Configure(config, appBuilder, container);
        }
    }
}