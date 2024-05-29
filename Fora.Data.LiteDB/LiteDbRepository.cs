using System.Linq.Expressions;
using Fora.Data.Models;
using LiteDB;

namespace Fora.Data.LiteDB;

public class LiteDbRepository<T>(string connectionString) : IRepository<T>
    where T : class, IDataModel
{
    public Task<T> GetByIdAsync(int id)
    {
        return Task.Run(() =>
        {
            using (var db = new LiteDatabase(connectionString))
            {
                var collection = db.GetCollection<T>();
                return collection.FindById(new BsonValue(id));
            }
        })!;
    }

    public Task<List<T>> GetAllAsync()
    {
        return Task.Run(() =>
        {
            using (var db = new LiteDatabase(connectionString))
            {
                var collection = db.GetCollection<T>();
                return collection.FindAll().ToList();
            }
        });
    }

    public Task AddAsync(T entity)
    {
        return Task.Run(() =>
        {
            using (var db = new LiteDatabase(connectionString))
            {
                var collection = db.GetCollection<T>();
                collection.Insert(entity);
            }
        });
    }

    public Task UpdateAsync(T entity)
    {
        return Task.Run(() =>
        {
            using (var db = new LiteDatabase(connectionString))
            {
                var collection = db.GetCollection<T>();
                collection.Update(entity);
            }
        });
    }

    public Task DeleteAsync(T entity)
    {
        return Task.Run(() =>
        {
            using (var db = new LiteDatabase(connectionString))
            {
                var collection = db.GetCollection<T>();
                collection.Delete(new BsonValue(entity.Id));
            }
        });
    }

    public Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return Task.Run(() =>
        {
            using (var db = new LiteDatabase(connectionString))
            {
                var collection = db.GetCollection<T>();
                return collection.Find(predicate).ToList();
            }
        });
    }
}