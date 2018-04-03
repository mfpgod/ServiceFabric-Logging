namespace Shared.WebApi
{
    using System;
    using System.Net;

    public class HttpStatusCodeHelper
    {
        public static HttpStatusCode GetHttpStatusCode(Exception exception)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            if (exception is ArgumentNullException)
            {
                statusCode = HttpStatusCode.BadRequest;
            }
            else
            {
                // Handle other exceptions, do other things
            }

            return statusCode;
        }
    }
}