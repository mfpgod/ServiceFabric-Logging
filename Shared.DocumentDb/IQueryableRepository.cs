namespace Shared.DocumentDb
{
    using System.Linq;
    using System.Threading.Tasks;

    public interface IQueryableRepository<T> : IRepository<T>
    {
        IOrderedQueryable<T> GetQueryable(string partitionKey = null, int? maxResults = null, string requestContinuation = null);

        Task<DbListResponse<T>> ExecuteQueryableAsync(IQueryable<T> queryable);

        Task<PagedDbResponse<T>> ExecutePagedQueryableAsync(IQueryable<T> queryable);
    }
}