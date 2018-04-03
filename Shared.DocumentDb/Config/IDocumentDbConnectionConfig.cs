namespace Shared.DocumentDb
{
    using System;

    public interface IDocumentDbConnectionConfig
    {
        string EndpointUri { get; }

        string PrimaryKey { get; }

        TimeSpan RequestTimeout { get; }
    }
}
