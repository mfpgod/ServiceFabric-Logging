namespace Shared.Logging
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class HttpClientLoggingHandler : DelegatingHandler
    {
        private readonly ILogger logger = LoggingContext.CreateLogger<HttpClientLoggingHandler>();

        private readonly bool logResponse;

        public HttpClientLoggingHandler(HttpMessageHandler innerHandler, bool logResponse = true)
            : base(innerHandler)
        {
            this.logResponse = logResponse;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage httpResponse = null;
            string requestContent = null;
            string responseContent = null;
            try
            {
                if (request.Content != null)
                {
                    requestContent = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                httpResponse = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (logResponse && httpResponse.Content != null)
                {
                    responseContent = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                if (logResponse || (httpResponse != null && httpResponse.IsSuccessStatusCode == false))
                {
                    logger.LogDebug(
                        "Http request to {uri}:\nheaders: {httpRequestHeaders}\nrequest: {httpRequest}\nheaders: {httpResponseHeaders}\nresponse: {httpResponse}",
                        request.RequestUri,
                        request.Headers.ToString(),
                        requestContent,
                        httpResponse?.Headers.ToString(),
                        responseContent);
                }
                else
                {
                    logger.LogDebug(
                        "Http request to {uri}:\nheaders: {httpRequestHeaders}\nrequest: {httpRequest}",
                        request.RequestUri,
                        request.Headers.ToString(),
                        requestContent);
                }
            }

            return httpResponse;
        }
    }
}