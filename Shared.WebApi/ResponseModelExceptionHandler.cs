namespace Shared.WebApi
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.ExceptionHandling;

    public class ResponseModelExceptionHandler : ExceptionHandler
    {
        public override void Handle(ExceptionHandlerContext context)
        {
            HttpStatusCode statusCode = HttpStatusCodeHelper.GetHttpStatusCode(context.Exception);
            HttpResponseMessage response = context.Request.CreateResponse(statusCode, new BaseApiResponse(context.Exception));
            context.Result = new ErrorHttpActionResult(response);
        }

        public override async Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Handle(context);
            await Task.CompletedTask;
        }

        internal class ErrorHttpActionResult : IHttpActionResult
        {
            private readonly HttpResponseMessage httpResponseMessage;

            public ErrorHttpActionResult(HttpResponseMessage httpResponseMessage)
            {
                this.httpResponseMessage = httpResponseMessage;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(httpResponseMessage);
            }
        }
    }
}