namespace Shared.WebApi
{
    using System;
    using System.Web.Http.ModelBinding;

    public class BaseApiResponse<T> : BaseApiResponse
    {
        public BaseApiResponse(ModelStateDictionary result)
            : base(result)
        {
        }

        public BaseApiResponse(Exception exception)
            : base(exception)
        {
        }

        [Newtonsoft.Json.JsonConstructor]
        public BaseApiResponse(T response)
        {
            Response = response;
            Success = true;
        }

        public T Response { get; protected set; }
    }
}