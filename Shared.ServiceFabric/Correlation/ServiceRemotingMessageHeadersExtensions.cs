namespace Shared.ServiceFabric.Correlation
{
    using System.Text;
    using Microsoft.ServiceFabric.Services.Remoting.V1;

    /// <summary>
    /// Class with extension helper methods for working with headers in a service remoting message
    /// </summary>
    internal static class ServiceRemotingMessageHeadersExtensions
    {
        public static bool ContainsHeader(this ServiceRemotingMessageHeaders messageHeaders, string headerName)
        {
            return messageHeaders.TryGetHeaderValue(headerName, out byte[] headerValueBytes);
        }

        public static bool TryGetHeaderValue(this ServiceRemotingMessageHeaders messageHeaders, string headerName, out string headerValue)
        {
            headerValue = null;
            if (!messageHeaders.TryGetHeaderValue(headerName, out byte[] headerValueBytes))
            {
                return false;
            }

            headerValue = Encoding.UTF8.GetString(headerValueBytes);
            return true;
        }

        public static void AddHeader(this ServiceRemotingMessageHeaders messageHeaders, string headerName, string value)
        {
            messageHeaders.AddHeader(headerName, Encoding.UTF8.GetBytes(value));
        }
    }
}
