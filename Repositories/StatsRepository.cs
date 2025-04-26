using BestReads.Models;
using BestReads.Database;
using MongoDB.Driver;

namespace BestReads.Repositories;

public class StatsRepository : BaseRepository<ReadingProgress>
{
    private readonly IMongoCollection<User> _users;

    public StatsRepository(MongoDbContext dbContext) : base(dbContext, "users")
    {
        _users = dbContext.Database.GetCollection<User>("users");
    }

    public async Task<List<ReadingProgress>?> GetAllReadingProgressAsync(string userId)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return user?.ReadingProgress;
    }

    public async Task<ReadingProgress?> GetReadingProgressByIdAsync(string userId, string progressId)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return user?.ReadingProgress?.FirstOrDefault(rp => rp.Id == progressId);
    }

    public async Task<ReadingProgress?> AddReadingProgressAsync(string userId, ReadingProgress readingProgress)
    {
        try
        {
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
        }
        catch (Exception ex)
        {
            throw new Exception("Error adding reading progress", ex);
        }
    }

    public async Task<ReadingProgress?> UpdateReadingProgressAsync(string userId, ReadingProgress readingProgress)
    {
        try
        {
            // Filter to find the user with the given userId
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(u => u.ReadingProgress, rp => rp.Id == readingProgress.Id) // Match progress by ID
            );

            // Update only the specific ReadingProgress in the list
            var update = Builders<User>.Update
                .Set(u => u.ReadingProgress![-1].CurrentPage, readingProgress.CurrentPage)
                .Set(u => u.ReadingProgress![-1].Progress, readingProgress.Progress)
                .Set(u => u.ReadingProgress![-1].UpdatedAt, DateTime.UtcNow);

            // Perform the update operation
            var result = await _users.UpdateOneAsync(filter, update);

            // Return the updated reading progress if successful
            if (result.ModifiedCount > 0)
            {
                return readingProgress; // Return the updated reading progress
            }

            return null; // If no match was found or nothing was updated
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating reading progress", ex);
        }
    }
}