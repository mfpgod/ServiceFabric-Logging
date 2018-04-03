namespace Services.WebApi
{
    using System.Configuration;
    using System.Fabric;
    using System.Reflection;
    using System.Web.Http;
    using Autofac;
    using Autofac.Integration.WebApi;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Logging;
    using Services.Contracts.Services.Dependency;
    using Shared.Config;
    using Shared.Extensions.Logging.ApplicationInsights;
    using Shared.Logging;
    using Shared.ServiceFabric;
    using Shared.ServiceFabric.WebApi;

    public static class AutofacContainerConfig
    {
        public static IContainer ConfigureContainer(HttpConfiguration httpConfig, ServiceContext serviceContext)
        {
            var containerBuilder = new ContainerBuilder();

            // TelemetryClient should be used per request
            containerBuilder.RegisterType<TelemetryClient>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<ServiceFabricContextOwinMiddleware>();

            containerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            containerBuilder.RegisterInstance(serviceContext);

            var dependencyAddress = ConfigurationManager.AppSettings.GetRefValue<string>("DependencyAddress");
            containerBuilder.Register((e) => ServiceFabricHelper.CreateServiceProxyWithCorrelation<IDependency>(dependencyAddress))
            .As<IDependency>();

            // register httpConfig itself in the container
            containerBuilder.RegisterInstance(httpConfig);

            // LoggerFactory registration in container
            var loggerFactory = new LoggerFactory();
            containerBuilder.RegisterInstance(loggerFactory)
                .As<ILoggerFactory>();

            IContainer container = containerBuilder.Build();

            // LoggerFactory App Insights configuration
            loggerFactory.AddApplicationInsights(() => container.Resolve<TelemetryClient>(), LogLevel.Trace);
            LoggingContext.Initialize(loggerFactory);

            // set Web API Dependency Resolver
            httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            return container;
        }
    }
}