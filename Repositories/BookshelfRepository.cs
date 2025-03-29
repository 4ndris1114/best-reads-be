using BestReads.Database;
using BestReads.Models;
using MongoDB.Driver;

namespace BestReads.Repositories;

public class BookshelfRepository {
    private readonly IMongoCollection<User> _users;

    public BookshelfRepository(MongoDbContext dbContext) {
        _users = dbContext.Database.GetCollection<User>("users");
    }

    // Get all bookshelves
    public async Task<List<Bookshelf>> GetAllBookshelvesAsync(string userId) {
        var user = await _users.Find(u => u.Id == userId)
                               .Project(u => u.Bookshelves)
                               .FirstOrDefaultAsync();

        return user ?? new List<Bookshelf>();
    }

    // Create a new bookshelf for a user
    public async Task CreateBookshelfAsync(string userId, Bookshelf newShelf) {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Push(u => u.Bookshelves, newShelf);
        await _users.UpdateOneAsync(filter, update);
    }

    // Delete a bookshelf
    public async Task DeleteBookshelfAsync(string userId, string shelfId) {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.PullFilter(u => u.Bookshelves,
            shelf => shelf.Id == shelfId);
        await _users.UpdateOneAsync(filter, update);
    }

    // Update bookshelf name
    public async Task RenameBookshelfAsync(string userId, string shelfId, string newName) {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.ElemMatch(u => u.Bookshelves, b => b.Id == shelfId)
        );

        var update = Builders<User>.Update.Set("bookshelves.$.name", newName);
        await _users.UpdateOneAsync(filter, update);
    }

    // Add a book to a bookshelf
    public async Task AddBookToBookshelfAsync(string userId, string shelfId, string bookId) {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.ElemMatch(u => u.Bookshelves, b => b.Id == shelfId)
        );

        var update = Builders<User>.Update.AddToSet("bookshelves.$.books", bookId);
        await _users.UpdateOneAsync(filter, update);
    }

    // Remove a book from a bookshelf
    public async Task RemoveBookFromBookshelfAsync(string userId, string shelfId, string bookId) {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.ElemMatch(u => u.Bookshelves, b => b.Id == shelfId)
        );

        var update = Builders<User>.Update.Pull("bookshelves.$.books", bookId);
        await _users.UpdateOneAsync(filter, update);
    }

    // Move a book from one bookshelf to another
    public async Task MoveBookAsync(string userId, string fromShelfId, string toShelfId, string bookId) {
        // Remove from original shelf
        await RemoveBookFromBookshelfAsync(userId, fromShelfId, bookId);

        // Add to target shelf
        await AddBookToBookshelfAsync(userId, toShelfId, bookId);
    }
}
