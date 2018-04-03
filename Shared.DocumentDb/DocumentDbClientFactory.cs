namespace Shared.DocumentDb
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    public class DocumentDbClientFactory : IDocumentDbClientFactory, IDisposable
    {
        private readonly object lockObj = new object();
        private readonly IDocumentDbConnectionConfig config;
        private DocumentClient client;

        public DocumentDbClientFactory(IDocumentDbConnectionConfig config)
        {
            this.config = config;
        }

        public IDocumentClient GetClient()
        {
            if (client == null)
            {
                InitializeClient();
            }

            return client;
        }

        public void Dispose()
        {
            client?.Dispose();
        }

        private void InitializeClient()
        {
            lock (lockObj)
            {
                if (client == null)
                {
                    TimeSpan requestTimeout = config.RequestTimeout;
                    var connectionPolicy = new ConnectionPolicy
                    {
                        ConnectionMode = ConnectionMode.Gateway,
                        ConnectionProtocol = Protocol.Https,
                        RequestTimeout = requestTimeout,
                    };
                    var newClient = new DocumentClient(new Uri(config.EndpointUri), config.PrimaryKey, connectionPolicy);

                    // need to use OpenAsync before any use
                    Task.Run(async () => { await newClient.OpenAsync(); }).Wait();

                    this.client = newClient;
                }
            }
        }
    }
}
