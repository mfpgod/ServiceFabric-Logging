namespace Shared
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception to use when a 3rd party service return an error
    /// </summary>
    [Serializable]
    public class DependencyException : AppException
    {
        public DependencyException()
        {
        }

        public DependencyException(string service, string uri, Exception innerException = null)
            : base(FormatMessage(service, uri, innerException), innerException)
        {
            Service = service;
            Uri = uri;
        }

        public DependencyException(string service, string uri, string errorMessage, string errorCode = null, Exception innerException = null)
            : base($"{service} request {uri} failed with message={errorMessage}, code={errorCode}", innerException)
        {
            Service = service;
            Uri = uri;
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        protected DependencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string Service { get; }

        public string Uri { get; }

        public string ErrorMessage { get; set; }

        public string ErrorCode { get; set; }

        private static string FormatMessage(string service, string uri, Exception innerException)
        {
            if (innerException == null)
            {
                return $"{service} request {uri} failed";
            }
            else
            {
                var dependencyException = innerException as DependencyException;
                if (dependencyException != null)
                {
                    return $"{service} request {uri} failed{Environment.NewLine}--->{dependencyException.Message.Replace(Environment.NewLine, $"{Environment.NewLine}\t")}";
                }

                return $"{service} request {uri} failed with message={innerException.Message}";
            }
        }
    }
}
