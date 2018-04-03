namespace Shared
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception to use when a 3rd party service return an error
    /// </summary>
    [Serializable]
    public class InternalDependencyException : DependencyException
    {
        public InternalDependencyException()
        {
        }

        public InternalDependencyException(string service, string uri, Exception innerException = null)
            : base(service, uri, innerException)
        {
        }

        public InternalDependencyException(string service, string uri, string message, string code, Exception innerException = null)
            : base(service, uri, message, code, innerException)
        {
        }

        protected InternalDependencyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
