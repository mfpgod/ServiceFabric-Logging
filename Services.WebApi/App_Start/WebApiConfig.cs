namespace Services.WebApi
{
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.ExceptionHandling;
    using Autofac;
    using Owin;
    using Shared.ServiceFabric.WebApi;
    using Shared.WebApi;
    using Shared.WebApi.Context;

    internal static class WebApiConfig
    {
        public static void Configure(HttpConfiguration config, IAppBuilder appBuilder, IContainer container)
        {
            config.MapHttpAttributeRoutes();

            // This adds ONLY the Autofac lifetime scope to the pipeline.
            appBuilder.UseAutofacLifetimeScopeInjector(container);

            // Now you can add middleware from the container into the pipeline
            appBuilder.UseMiddlewareFromContainer<ServiceFabricContextOwinMiddleware>();

            FormatterConfig.ConfigureFormatters(config.Formatters);

            OperationIdQueryStringResolver.Initialize("Services.WebApi");
            config.MessageHandlers.Add(new RequestTrackingDelegatingHandler(
                "X-OperationId",
                new List<IOperationIdResolver>
                {
                    new OperationIdHeaderResolver("X-OperationId"),
                    new OperationIdQueryStringResolver()
                }));

            // filter that validates a model before executing a controller method
            config.Filters.Add(new ModelValidationActionFilterAttribute());

            // filter that catch exceptions in controller methods and return a standard response
            config.Filters.Add(new ResponseModelExceptionFilterAttribute());

            // filter that catch exceptions in web api infrastructure and return a standard response
            config.Services.Replace(typeof(IExceptionHandler), new Shared.WebApi.ResponseModelExceptionHandler());

            // exception logger that sends exceptions to logging infrastructure used
            config.Services.Add(typeof(IExceptionLogger), new Shared.WebApi.ExceptionLogger());

            appBuilder.UseWebApi(config);
            config.EnsureInitialized();
        }
    }
}