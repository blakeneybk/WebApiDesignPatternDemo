using System.Linq.Expressions;
using Fora.Data.Models;

namespace Fora.Data;

public interface IRepository<T> where T : class, IDataModel
{
    Task<T> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
}