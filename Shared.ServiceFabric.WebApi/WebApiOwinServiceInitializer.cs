namespace Shared.ServiceFabric.WebApi
{
    using System.Configuration;
    public class WebApiOwinServiceInitializer
    {
        public WebApiOwinServiceInitializer(IOwinAppBuilder owinStartup)
        {
            OwinStartup = owinStartup;
        }

        public string ServiceEndpointName => ConfigurationManager.AppSettings["ServiceEndpointName"];

        public IOwinAppBuilder OwinStartup { get; set; }
    }
}
