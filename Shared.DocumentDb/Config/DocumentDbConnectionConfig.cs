namespace Shared.DocumentDb
{
    using System;
    using System.Configuration;
    using Shared.Config;

    public class DocumentDbConnectionConfig : IDocumentDbConnectionConfig
    {
        public string EndpointUri => ConfigurationManager.AppSettings.GetRefValue<string>("DocumentDb.EndpointUri");

        public string PrimaryKey => ConfigurationManager.AppSettings.GetRefValue<string>("DocumentDb.PrimaryKey");

        public TimeSpan RequestTimeout => ConfigurationManager.AppSettings.GetValue<TimeSpan>("DocumentDb.RequestTimeout");
    }
}
