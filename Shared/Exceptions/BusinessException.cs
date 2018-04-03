namespace Shared
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception to use when a business rule is violated
    /// </summary>
    [Serializable]
    public class BusinessException : AppException
    {
        public BusinessException()
        {
        }

        public BusinessException(string message)
            : base(message)
        {
        }

        public BusinessException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected BusinessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}