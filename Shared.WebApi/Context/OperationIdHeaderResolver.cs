namespace Shared.WebApi.Context
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;

    public class OperationIdHeaderResolver: IOperationIdResolver
    {
        public OperationIdHeaderResolver(string requestIdHeaderName)
        {
            RequestIdHeaderName = requestIdHeaderName;
        }

        public string RequestIdHeaderName { get; set; }

        public bool TryResolve(HttpRequestMessage request, out string operationId)
        {
            operationId = null;
            if (request.Headers.TryGetValues(RequestIdHeaderName, out IEnumerable<string> values))
            {
                operationId = values.FirstOrDefault();
            }

            return operationId != null;
        }
    }
}