namespace Shared.DocumentDb
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IRepository<T>
    {
        Task<DbResponse<T>> CreateAsync(T entity);

        Task<DbResponse<T>> UpsertAsync(T entity);

        Task<DbResponse<T>> DeleteAsync(string id, string partitionKey = null);

        Task<DbResponse<T>> GetAsync(string id, string partitionKey = null);

        Task<DbListResponse<T>> QueryAsync(Expression<Func<T, bool>> predicate, string partitionKey = null);

        Task<DbResponse<T>> QuerySingleAsync(Expression<Func<T, bool>> predicate, string partitionKey = null);
    }
}