namespace Shared.WebApi.Context
{
    using System.Net.Http;

    public interface IOperationIdResolver
    {
        bool TryResolve(HttpRequestMessage request, out string operationId);
    }
}