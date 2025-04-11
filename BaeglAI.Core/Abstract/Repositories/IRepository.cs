using System.Linq.Expressions;

public interface IRepository<T>
{
    Task<bool> AddAsync(string storeId, T entity);
    Task<bool> UpdateAsync(string storeId, string entityId, T entity);
    Task<bool> DeleteAsync(string storeId, string entityId);
    Task<T> GetByIdAsync(string storeId, string entityId);
    Task<IEnumerable<T>> GetAllAsync(string storeId);
    Task<IEnumerable<T>> GetWhereAsync(string storeId, Expression<Func<T, bool>> predicate);
}