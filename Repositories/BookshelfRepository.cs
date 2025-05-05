using BestReads.Database;
using BestReads.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BestReads.Repositories;

public class BookshelfRepository {
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<BookshelfRepository> _logger;

    public BookshelfRepository(MongoDbContext dbContext, ILogger<BookshelfRepository> logger) {
        _logger = logger;
        _users = dbContext.Database.GetCollection<User>("users");
    }

    // Get all bookshelves
    public async Task<List<Bookshelf>> GetAllBookshelvesAsync(string userId) {
        try {
            var shelf = await _users.Find(u => u.Id == userId)
                                   .Project(u => u.Bookshelves)
                                   .FirstOrDefaultAsync();

            return shelf ?? new List<Bookshelf>();
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error getting bookshelves for user with id {userId}");
            throw;
        }
    }

    // Get a specific bookshelf
    public async Task<Bookshelf?> GetBookshelfByIdAsync(string userId, string shelfId) {
        try {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var projection = Builders<User>.Projection.Expression(u =>
                u.Bookshelves!.FirstOrDefault(b => b.Id == shelfId)
            );

            var bookshelf = await _users.Find(filter).Project(projection).FirstOrDefaultAsync();
            return bookshelf;
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error getting bookshelf {shelfId} for user {userId}");
            throw;
        }
    }

    // Create a new bookshelf for a user
    public async Task CreateBookshelfAsync(string userId, Bookshelf newShelf) {
        try {
            newShelf.Id = ObjectId.GenerateNewId().ToString();
            if (newShelf.Books == null) {
                newShelf.Books = new List<string>();
            }
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Push(u => u.Bookshelves, newShelf);
            await _users.UpdateOneAsync(filter, update);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error creating bookshelf for user with id {userId}");
            throw;
        }
    }

    // Delete a bookshelf
    public async Task DeleteBookshelfAsync(string userId, string shelfId) {
        try {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.PullFilter(u => u.Bookshelves,
                shelf => shelf.Id == shelfId);
            await _users.UpdateOneAsync(filter, update);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error deleting bookshelf {shelfId} for user {userId}");
            throw;
        }
    }

    // Update bookshelf name
    public async Task RenameBookshelfAsync(string userId, string shelfId, string newName) {
        try {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(u => u.Bookshelves, b => b.Id == shelfId)
            );

            var update = Builders<User>.Update.Set("bookshelves.$.name", newName);
            await _users.UpdateOneAsync(filter, update);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error renaming bookshelf {shelfId} for user {userId}");
            throw;
        }
    }

    // Add a book to a bookshelf - can optionally take session in case of transaction (from moveBook method)
    public async Task<bool> AddBookToBookshelfAsync(string userId, string shelfId, string bookId, IClientSessionHandle? session = null) {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.ElemMatch(u => u.Bookshelves, b => b.Id == shelfId)
        );

        var update = Builders<User>.Update.Push("Bookshelves.$.Books", bookId);

        UpdateResult result = session != null
            ? await _users.UpdateOneAsync(session, filter, update)
            : await _users.UpdateOneAsync(filter, update);

        return result.MatchedCount > 0 && result.ModifiedCount > 0;
    }

    // Remove a book from a bookshelf - can optionally take session in case of transaction (from moveBook method)
    public async Task RemoveBookFromBookshelfAsync(string userId, string shelfId, string bookId, IClientSessionHandle? session = null) {
        try {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(u => u.Bookshelves, b => b.Id == shelfId)
            );

            var update = Builders<User>.Update.Pull("bookshelves.$.books", bookId);
            
            var updateTask = session != null
                ? _users.UpdateOneAsync(session, filter, update)
                : _users.UpdateOneAsync(filter, update);

            await updateTask;
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error removing book {bookId} from bookshelf {shelfId} for user {userId}");
            throw;
        }
    }

    // Move a book from one shelf to another
    public async Task<bool> MoveBookToAnotherBookshelfAsync(string userId, string fromShelfId, string bookId, string toShelfId) {
        try {
            using (var session = await _users.Database.Client.StartSessionAsync()) {
                session.StartTransaction();
                try {
                    // Pass the session to ensure it's within the transaction
                    await RemoveBookFromBookshelfAsync(userId, fromShelfId, bookId, session);
                    await AddBookToBookshelfAsync(userId, toShelfId, bookId, session);

                    await session.CommitTransactionAsync();
                    return true;
                } catch (Exception) {
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error moving book {bookId} from shelf {fromShelfId} to shelf {toShelfId} for user {userId}");
            throw;
        }
    }
}
