using BestReads.Models;
using BestReads.Database;
using MongoDB.Driver;

namespace BestReads.Repositories;

public class StatsRepository : BaseRepository<ReadingProgress> {
    private readonly IMongoCollection<User> _users;

    public StatsRepository(MongoDbContext dbContext) : base(dbContext, "users") {
        _users = dbContext.Database.GetCollection<User>("users");
    }

    public async Task<ReadingProgress?> GetReadingProgressAsync(string userId) {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return user?.ReadingProgress ?? new ReadingProgress();
    }

    public async Task<ReadingProgress?> UpdateReadingProgressAsync(string userId, ReadingProgress readingProgress) {
        try{
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Set(u => u.ReadingProgress, readingProgress);
            await _users.UpdateOneAsync(filter, update);
        } catch (Exception ex) {
            throw new Exception("Error updating reading progress", ex);
        }
    }
}