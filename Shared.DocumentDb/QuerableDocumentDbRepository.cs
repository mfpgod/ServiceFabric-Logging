namespace Shared.DocumentDb
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    public abstract class QuerableDocumentDbRepository<T> : DocumentDbRepository<T>, IQueryableRepository<T>
    {
        protected QuerableDocumentDbRepository(IDocumentDbClientFactory documentDbInitializer, string partitionKey = null)
            : base(documentDbInitializer, partitionKey)
        {
        }

        #region IQueryableRepository implementation

        public virtual IOrderedQueryable<T> GetQueryable(string partitionKey = null, int? maxResults = null, string requestContinuation = null)
        {
            ValidatePartitionKey(partitionKey);

            FeedOptions feedOptions = new FeedOptions();
            if (partitionKey != null || PartitionKey != null)
            {
                feedOptions.PartitionKey = new PartitionKey(PartitionKey ?? partitionKey);
            }

            if (requestContinuation != null)
            {
                feedOptions.MaxItemCount = maxResults;
                feedOptions.RequestContinuation = requestContinuation;
            }

            return Client.CreateDocumentQuery<T>(GetCollectionUri(), feedOptions);
        }

        public virtual async Task<DbListResponse<T>> ExecuteQueryableAsync(IQueryable<T> queryable)
        {
            return await queryable.QueryDocumentDbAsync<T>();
        }

        public virtual async Task<PagedDbResponse<T>> ExecutePagedQueryableAsync(IQueryable<T> queryable)
        {
            IDocumentQuery<T> documentQuery = queryable.AsDocumentQuery();
            FeedResponse<T> response = await documentQuery.ExecuteNextAsync<T>();
            return new PagedDbResponse<T>
            {
                Entities = response,
                OperationCost = response.RequestCharge,
                NextPageToken = response.ResponseContinuation
            };
        }

        #endregion
    }
}
