namespace Adapters.DocumentDbDependency
{
    using System.Configuration;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Shared.Config;
    using Shared.DocumentDb;

    public class UserCollectionInitializer
    {
        public const string CollectionThroughputConfigName = "DocumentDb.UserCollectionThroughput";
        public const string DatabaseIdConfigName = "DocumentDb.DatabaseId";
        public const string CollectionNameConfigName = "DocumentDb.CollectionName";

        public static string DatabaseId { get; private set; }

        public static string CollectionId { get; private set; }

        private static IndexingPolicy CollectionIndexingPolicy => new IndexingPolicy
        {
            IndexingMode = IndexingMode.Consistent,
            Automatic = true
        };

        private static RequestOptions CollectionRequestOptions => new RequestOptions
        {
            OfferThroughput = ConfigurationManager.AppSettings.GetValue<int>(CollectionThroughputConfigName)
        };

        public static async Task Initialize(IDocumentDbClientFactory documentDbClientFactory)
        {
            DatabaseId = ConfigurationManager.AppSettings.GetRefValue<string>(DatabaseIdConfigName);
            CollectionId = ConfigurationManager.AppSettings.GetRefValue<string>(CollectionNameConfigName);

            IDocumentClient client = documentDbClientFactory.GetClient();
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
            }

            try
            {
                  await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    var documentCollection = new DocumentCollection
                    {
                        Id = CollectionId,
                        IndexingPolicy = CollectionIndexingPolicy,
                    };

                    await client.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(DatabaseId), documentCollection, CollectionRequestOptions);
                }
            }
        }
    }
}