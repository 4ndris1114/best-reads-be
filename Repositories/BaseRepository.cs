using BestReads.Database;
using MongoDB.Driver;

namespace BestReads.Repositories;

public abstract class BaseRepository<T> where T : class {
    private readonly IMongoCollection<T> _collection;

    public BaseRepository(MongoDbContext dbContext, string collectionName) {
        _collection = dbContext.Database.GetCollection<T>(collectionName);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync() {
        return await _collection.Find(Builders<T>.Filter.Empty).ToListAsync();
    }

    public virtual async Task<T> GetByIdAsync(string id) {
        var filter = Builders<T>.Filter.Eq("Id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task CreateAsync(T entity) {
        await _collection.InsertOneAsync(entity);
    }

    public virtual async Task UpdateAsync(string id, T entity) {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _collection.ReplaceOneAsync(filter, entity);
    }

    public virtual async Task DeleteAsync(string id) {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _collection.DeleteOneAsync(filter);
    }

    public IMongoCollection<T> getCollection() {
        return _collection;
    }
}