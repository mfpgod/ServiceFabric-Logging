namespace Shared.WebApi
{
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Routing;

    public static class HttpRequestMessageExtensions
    {
        public static bool TryGetRouteTemplate(this HttpRequestMessage request, out string result)
        {
            IHttpRouteData routeData = request.GetRouteData() ?? request.GetConfiguration().Routes.GetRouteData(request);
            string routeTemplate = routeData.Route?.RouteTemplate;
            if (string.IsNullOrEmpty(routeTemplate) && routeData?.Values != null && routeData.Values.TryGetValue("MS_SubRoutes", out object objValue))
            {
                routeTemplate = (objValue as IHttpRouteData[])?.FirstOrDefault()?.Route?.RouteTemplate;
            }

            result = routeTemplate;
            return routeTemplate != null;
        }
    }
}