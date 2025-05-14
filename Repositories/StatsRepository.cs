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
        try {
            // Filter to find the user with the given userId
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(u => u.ReadingProgress, rp => rp.Id == readingProgress.Id) // Match progress by ID
            );

            var update = Builders<User>.Update
                .Set("ReadingProgress.$.CurrentPage", readingProgress.CurrentPage)
                .Set("ReadingProgress.$.UpdatedAt", DateTime.UtcNow);
           
            // Perform the update operation
            var result = await _users.UpdateOneAsync(filter, update);

            // 2. If update was successful and book is finished
        if (result.ModifiedCount > 0 && readingProgress.CurrentPage == readingProgress.TotalPages) {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return readingProgress;

            var currentlyReadingShelf = user.Bookshelves!.FirstOrDefault(s => s.Name == "Currently Reading");
            var readShelf = user.Bookshelves!.FirstOrDefault(s => s.Name == "Read");

            if (currentlyReadingShelf != null && readShelf != null && currentlyReadingShelf.Books!.Any(b => b.Id == readingProgress.BookId)) {
                // Remove from Currently Reading
                var removeFilter = Builders<User>.Filter.Eq(u => u.Id, userId);
                var removeUpdate = Builders<User>.Update.Pull("Bookshelves.$[s].Books", readingProgress.BookId);
                var removeOptions = new UpdateOptions {
                    ArrayFilters = new List<ArrayFilterDefinition> {
                        new JsonArrayFilterDefinition<BsonDocument>("{ 's.Name': 'Currently Reading' }")
                    }
                };
                await _users.UpdateOneAsync(removeFilter, removeUpdate, removeOptions);

                // Add to Read
                var addUpdate = Builders<User>.Update.Push("Bookshelves.$[s].Books", readingProgress.BookId);
                var addOptions = new UpdateOptions {
                    ArrayFilters = new List<ArrayFilterDefinition> {
                        new JsonArrayFilterDefinition<BsonDocument>("{ 's.Name': 'Read' }")
                    }
                };
                await _users.UpdateOneAsync(removeFilter, addUpdate, addOptions);
            }
            return readingProgress;
        }

        return result.ModifiedCount > 0 ? readingProgress : null;
    } catch (Exception ex) {
        throw new Exception($"Error updating reading progress: {ex.Message}");
    }
    }
}