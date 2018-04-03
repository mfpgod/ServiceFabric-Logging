namespace Shared.WebApi.Context
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class OperationIdQueryStringAttribute : Attribute
    {
        public OperationIdQueryStringAttribute(string queryPath, string queryParameter)
        {
            QueryPath = queryPath;
            QueryParameter = queryParameter;
        }

        public string QueryPath { get; }
        public string QueryParameter { get; }
    }
}