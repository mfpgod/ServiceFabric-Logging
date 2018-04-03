namespace Shared.WebApi
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Filters;

    public class ResponseModelExceptionFilterAttribute: ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                HttpStatusCode statusCode = HttpStatusCodeHelper.GetHttpStatusCode(context.Exception);
                context.Response = context.Request.CreateResponse(statusCode, new BaseApiResponse(context.Exception));
            }

            base.OnException(context);
        }
    }
}