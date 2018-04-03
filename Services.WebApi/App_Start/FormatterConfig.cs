namespace Services.WebApi
{
    using System.Net.Http.Formatting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public static class FormatterConfig
    {
        public static void ConfigureFormatters(MediaTypeFormatterCollection formatters)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            if (formatters.JsonFormatter != null)
            {
                formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }
        }
    }
}
