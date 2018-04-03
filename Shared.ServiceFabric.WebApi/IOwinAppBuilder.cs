namespace Shared.ServiceFabric.WebApi
{
    using System.Fabric;
    using global::Owin;

    public interface IOwinAppBuilder
    {
        void Configuration(IAppBuilder appBuilder, ServiceContext serviceContext);
    }
}
