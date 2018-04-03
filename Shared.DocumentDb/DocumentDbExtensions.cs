namespace Shared.DocumentDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    public static class DocumentDbExtensions
    {
        public static async Task<DbListResponse<T>> QueryDocumentDbAsync<T>(this IQueryable<T> query, Action<IEnumerable<T>> callback = null)
        {
            double ruCost = 0;
            int requestCount = 0;

            IDocumentQuery<T> documentQuery = query.AsDocumentQuery();
            FeedResponse<T> batch = await documentQuery.ExecuteNextAsync<T>();

            ruCost += batch.RequestCharge;
            requestCount++;

            callback?.Invoke(batch);

            if (!documentQuery.HasMoreResults)
            {
                return new DbListResponse<T>
                {
                    Entities = batch,
                    OperationCost = ruCost,
                    RequestCount = requestCount
                };
            }

            var batches = new List<IEnumerable<T>> { batch };
            do
            {
                batch = await documentQuery.ExecuteNextAsync<T>();

                callback?.Invoke(batch);

                batches.Add(batch);

                ruCost += batch.RequestCharge;
                requestCount++;
            }
            while (documentQuery.HasMoreResults);

            return new DbListResponse<T>
            {
                Entities = batches.SelectMany(b => b),
                OperationCost = ruCost,
                RequestCount = requestCount
            };
        }
    }
}
