using MongoDB.Driver;
using BestReads.Models;
using BestReads.Database;
using MongoDB.Bson;

namespace BestReads.Repositories;

public class BookRepository : BaseRepository<Book> {
    public BookRepository(MongoDbContext dbContext) : base(dbContext, "Books") {}

    public async Task<IEnumerable<Book>> GetBooksByTitleAsync(string title) {
        var filter = Builders<Book>.Filter.Regex("Title", new BsonRegularExpression(title, "i")); // i -> case insensitive & regex for partial match 
        return await getCollection().Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(string author) {
        var filter = Builders<Book>.Filter.Regex("Author", new BsonRegularExpression(author, "i")); // i -> case insensitive & regex for partial match
        return await getCollection().Find(filter).ToListAsync();
    }
}