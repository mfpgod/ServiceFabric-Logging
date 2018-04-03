namespace Shared.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.ModelBinding;
    using Newtonsoft.Json;

    public class BaseApiResponse
    {
        public BaseApiResponse()
        {
            Success = true;
        }

        public BaseApiResponse(ModelStateDictionary result)
        {
            Errors = result.Values.SelectMany(v => v.Errors.Select(e => $"{e.ErrorMessage} {e.Exception?.Message}"));
            Success = !Errors.Any();
        }

        public BaseApiResponse(Exception exception)
        {
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                Errors = aggregateException.Flatten().InnerExceptions
                    .Select(ex => $"{ex.GetType().Name}: {ex.Message}");
            }
            else
            {
                Errors = new List<string> { $"{exception.GetType().Name}: {exception.Message}" };
            }

            Success = false;
        }

        public BaseApiResponse(string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(errorMessage) == false)
            {
                Errors = new List<string> { errorMessage };
            }

            Success = false;
        }

        [JsonProperty("success")]
        public bool Success { get; protected set; }

        [JsonProperty("code")]
        public string Code { get; protected set; }

        [JsonProperty("errors")]
        public IEnumerable<string> Errors { get; protected set; }
    }
}