namespace Shared.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Shared.Logging;
    using Shared.WebApi.Context;

    public class RequestTrackingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger logger = LoggingContext.CreateLogger<RequestTrackingDelegatingHandler>();

        private readonly IEnumerable<IOperationIdResolver> operationIdResolverList;

        private readonly string requestIdHeaderName;

        public RequestTrackingDelegatingHandler(
            string requestIdHeaderName,
            IEnumerable<IOperationIdResolver> operationIdResolverList = null)
        {
            this.requestIdHeaderName = requestIdHeaderName;
            this.operationIdResolverList = operationIdResolverList;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(!request.TryGetRouteTemplate(out string rootTemplate))
            {
                rootTemplate = request.RequestUri.GetLeftPart(UriPartial.Path);
            }

            HttpMethod method = request.Method;
            Uri uri = request.RequestUri;

            var activity = new Activity($"{method} {rootTemplate}");
            activity.AddBaggage(ActivityConstants.RootOperationName, activity.OperationName);

            // try resolve parent operationid
            if(operationIdResolverList != null)
            {
                foreach(IOperationIdResolver resolver in operationIdResolverList)
                {
                    if(resolver.TryResolve(request, out string parentOperationId))
                    {
                        activity.SetParentId(parentOperationId);
                        break;
                    }
                }
            }

            activity.Start();
            HttpResponseMessage response = null;
            var success = true;
            try
            {
                response = await base.SendAsync(request, cancellationToken);

                // return requestId in response header, as a reference for this specific operation
                response.Headers.Add(requestIdHeaderName, new[] { activity.RootId });

                return response;
            }
            catch(Exception ex)
            {
                success = false;
                logger.LogError(ex, "Request {uri} exception", uri);
                throw;
            }
            finally
            {
                activity.Stop();
                var requestLog = new RequestLog
                {
                    Name = activity.OperationName,
                    OperationName = activity.OperationName,
                    Type = "HTTP",
                    Uri = uri,
                    ResponseCode = response != null ? ((int)response.StatusCode).ToString() : null,
                    Success = response != null && (int)response.StatusCode < 400 && success
                };
                logger.LogRequest(requestLog, activity);
            }
        }
    }
}
