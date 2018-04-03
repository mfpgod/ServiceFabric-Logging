namespace Shared.DocumentDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;

    public abstract class DocumentDbRepository<T> : IRepository<T>
    {
        protected DocumentDbRepository(IDocumentDbClientFactory documentDbInitializer, string partitionKey = null)
        {
            PartitionKey = partitionKey;
            Client = documentDbInitializer.GetClient();
        }

        public abstract string DatabaseId { get; }

        public abstract string CollectionId { get; }

        protected string PartitionKey { get; }

        protected IDocumentClient Client { get; }

        #region IRepository implementation

        public virtual async Task<DbResponse<T>> CreateAsync(T entity)
        {
            ResourceResponse<Document> response = await Client.CreateDocumentAsync(GetCollectionUri(), entity);
            T newEntity = (dynamic)response.Resource;
            return new DbResponse<T>
            {
                ETag = response.Resource.ETag,
                OperationCost = response.RequestCharge,
                Entity = newEntity
            };
        }

        public virtual async Task<DbResponse<T>> UpsertAsync(T entity)
        {
            ResourceResponse<Document> response = await Client.UpsertDocumentAsync(GetCollectionUri(), entity);
            T updatedEntity = (dynamic)response.Resource;
            return new DbResponse<T>
            {
                ETag = response.Resource.ETag,
                OperationCost = response.RequestCharge,
                Entity = updatedEntity
            };
        }

        public virtual async Task<DbResponse<T>> DeleteAsync(string id, string partitionKey = null)
        {
            ValidatePartitionKey(partitionKey);

            var requestOptions = new RequestOptions();
            if (partitionKey != null || PartitionKey != null)
            {
                requestOptions.PartitionKey = new PartitionKey(PartitionKey ?? partitionKey);
            }

            Uri docUri = UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id);
            ResourceResponse<Document> response = await Client.DeleteDocumentAsync(docUri, requestOptions);

            return new DbResponse<T>
            {
                OperationCost = response.RequestCharge,
            };
        }

        public virtual async Task<DbListResponse<T>> QueryAsync(Expression<Func<T, bool>> predicate = null, string partitionKey = null)
        {
            ValidatePartitionKey(partitionKey);

            var feedOptions = new FeedOptions();
            if (partitionKey != null || PartitionKey != null)
            {
                feedOptions.PartitionKey = new PartitionKey(PartitionKey ?? partitionKey);
            }

            IQueryable<T> querable = Client.CreateDocumentQuery<T>(GetCollectionUri(), feedOptions);
            if (predicate != null)
            {
                querable = querable.Where(predicate);
            }

            return await querable.QueryDocumentDbAsync<T>();
        }

        public virtual async Task<DbResponse<T>> QuerySingleAsync(Expression<Func<T, bool>> predicate, string partitionKey = null)
        {
            DbListResponse<T> response = await QueryAsync(predicate, partitionKey);
            return new DbResponse<T>
            {
                Entity = response.Entities.FirstOrDefault(),
                OperationCost = response.OperationCost,
                RequestCount = response.RequestCount
            };
        }

        public virtual async Task<DbResponse<T>> GetAsync(string id, string partitionKey = null)
        {
            ValidatePartitionKey(partitionKey);

            var feedOptions = new RequestOptions();
            if (partitionKey != null || PartitionKey != null)
            {
                feedOptions.PartitionKey = new PartitionKey(PartitionKey ?? partitionKey);
            }

            try
            {
                Uri docUri = UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id);
                ResourceResponse<Document> response = await Client.ReadDocumentAsync(docUri, feedOptions);

                T entity = (dynamic)response.Resource;
                return new DbResponse<T>
                {
                    ETag = response.Resource.ETag,
                    Entity = entity,
                    OperationCost = response.RequestCharge,
                };
            }
            catch (DocumentClientException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException($"'{typeof(T).Name}' with id='{id}' was not found", ex);
                }

                throw;
            }
        }

        #endregion

        protected Uri GetCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);
        }

        protected virtual void ValidatePartitionKey(string partitionKey)
        {
            if (PartitionKey != null && partitionKey != null && PartitionKey != partitionKey)
            {
                throw new ArgumentException($"Parameter is invalid for collections with a static partition key {PartitionKey}", nameof(partitionKey));
            }
        }
    }
}