namespace Shared.WebApi
{
    using System.Web.Http.ExceptionHandling;
    using Microsoft.Extensions.Logging;
    using Shared.Logging;

    public class ExceptionLogger : System.Web.Http.ExceptionHandling.ExceptionLogger
    {
        public override void Log(ExceptionLoggerContext context)
        {
            LoggingContext.CreateLogger<ExceptionLogger>()
                .LogError(context.Exception, "Request {uri} failed", context.Request.RequestUri);

            base.Log(context);
        }
    }
}