using BestReads.Models;
using BestReads.Database;
using MongoDB.Driver;
using MongoDB.Bson;
using DnsClient.Protocol;

namespace BestReads.Repositories;

public class StatsRepository
{
    private readonly IMongoCollection<User> _users;

    public StatsRepository(MongoDbContext dbContext) {
        _users = dbContext.Database.GetCollection<User>("users");
    }

    public async Task<List<ReadingProgress>?> GetAllReadingProgressAsync(string userId) {
        try {
            var readingProgress = await _users.Find(u => u.Id == userId)
                                   .Project(u => u.ReadingProgress)
                                   .FirstOrDefaultAsync();
            return readingProgress;
        } catch (Exception ex) {
            throw new Exception("Error getting reading progress", ex);
        }
    }

    public async Task<ReadingProgress?> GetReadingProgressByIdAsync(string userId, string progressId) {
        try {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var projection = Builders<User>.Projection.Expression(u =>
                u.ReadingProgress!.FirstOrDefault(rp => rp.Id == progressId)
            );            

            var readingProgress = await _users.Find(filter).Project(projection).FirstOrDefaultAsync();
            return readingProgress;
        } catch (Exception ex) {
            throw new Exception("Error getting reading progress", ex);
        }
    }

    public async Task<ReadingProgress?> AddReadingProgressAsync(string userId, ReadingProgress readingProgress) {
        try {
            readingProgress.Id = ObjectId.GenerateNewId().ToString();
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Push(u => u.ReadingProgress, readingProgress);
            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };

            // Perform the update operation and get the updated User object
            var updatedUser = await _users.FindOneAndUpdateAsync(filter, update, options);

            // Find the added ReadingProgress in the user's updated list
            return updatedUser?.ReadingProgress?.FirstOrDefault(rp => rp.Id == readingProgress.Id);
        } catch (Exception ex) {
            throw new Exception("Error adding reading progress", ex);
        }
    }

    public async Task<ReadingProgress?> UpdateReadingProgressAsync(string userId, ReadingProgress readingProgress) {
        using var session = await _users.Database.Client.StartSessionAsync();
        session.StartTransaction();

        try {
            // Filter to find the user with the given userId and the specific reading progress entry
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(u => u.ReadingProgress, rp => rp.Id == readingProgress.Id)
            );

var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
if (user == null || user.ReadingProgress == null) {
    await session.AbortTransactionAsync();
    return null;
}

var index = user.ReadingProgress.FindIndex(rp => rp.Id == readingProgress.Id);
if (index == -1) {
    await session.AbortTransactionAsync();
    return null;
}

var update = Builders<User>.Update
    .Set($"readingProgress.{index}.currentPage", readingProgress.CurrentPage)
    .Set($"readingProgress.{index}.updatedAt", DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(session, filter, update);

            if (result.ModifiedCount > 0 && readingProgress.CurrentPage == readingProgress.TotalPages) {
                var currentlyReadingShelf = user.Bookshelves!.FirstOrDefault(s => s.Name == "Currently Reading");
                var readShelf = user.Bookshelves!.FirstOrDefault(s => s.Name == "Read");

                if (currentlyReadingShelf != null && readShelf != null &&
                    currentlyReadingShelf.Books!.Any(b => b.Id == readingProgress.BookId)) {

                    var removeFilter = Builders<User>.Filter.Eq(u => u.Id, userId);
                    var removeUpdate = Builders<User>.Update.PullFilter("bookshelves.$[s].books",
                        Builders<BookshelfBook>.Filter.Eq(b => b.Id, readingProgress.BookId));
                    var removeOptions = new UpdateOptions {
                        ArrayFilters = new List<ArrayFilterDefinition> {
                            new JsonArrayFilterDefinition<BsonDocument>("{ 's.name': 'Currently Reading' }")
                        }
                    };
                    await _users.UpdateOneAsync(session, removeFilter, removeUpdate, removeOptions);

                    var addUpdate = Builders<User>.Update.Push("bookshelves.$[s].books", new BookshelfBook {
                        Id = readingProgress.BookId,
                        UpdatedAt = DateTime.UtcNow
                    });
                    var addOptions = new UpdateOptions {
                        ArrayFilters = new List<ArrayFilterDefinition> {
                            new JsonArrayFilterDefinition<BsonDocument>("{ 's.name': 'Read' }")
                        }
                    };
                    await _users.UpdateOneAsync(session, removeFilter, addUpdate, addOptions);
                }

                await session.CommitTransactionAsync();
                return readingProgress;
            }

            await session.CommitTransactionAsync();
            return result.ModifiedCount > 0 ? readingProgress : null;
        } catch (Exception ex) {
            await session.AbortTransactionAsync();
            throw new Exception($"Error updating reading progress: {ex.Message}");
        }
    }
}