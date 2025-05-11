using BestReads.Database;
using BestReads.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BestReads.Repositories;

public class ReadingChallengeRepository {
    private readonly IMongoCollection<User> _users;
    private readonly ILogger<BookshelfRepository> _logger;

    public ReadingChallengeRepository(MongoDbContext dbContext, ILogger<BookshelfRepository> logger) {
        _logger = logger;
        _users = dbContext.Database.GetCollection<User>("users");
    }

    // Get all reading challenges
    public async Task<List<ReadingChallenge>> GetAllReadingChallengesAsync(string userId) {
        try {
            var readingChallenges = await _users.Find(u => u.Id == userId)
                                   .Project(u => u.ReadingChallenges)
                                   .FirstOrDefaultAsync();
            return readingChallenges ?? new List<ReadingChallenge>();
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error getting reading challenges for user with id {userId}");
            throw;
        }
    }

    // Get a specific reading challenge by ID
    public async Task<ReadingChallenge?> GetReadingChallengeByIdAsync(string userId, string challengeId) {
        try {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var projection = Builders<User>.Projection.Expression(u =>
                u.ReadingChallenges!.FirstOrDefault(b => b.Id == challengeId)
            );

            var readingChallenge = await _users.Find(filter).Project(projection).FirstOrDefaultAsync();
            return readingChallenge;
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error getting reading challenge {challengeId} for user {userId}");
            throw;
        }
    }

    // Get a specific reading challenge by year
    public async Task<ReadingChallenge?> GetReadingChallengeByYearAsync(string userId, int year) {
        try {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var projection = Builders<User>.Projection.Expression(u =>
                u.ReadingChallenges!.FirstOrDefault(b => b.Year == year)
            );

            var readingChallenge = await _users.Find(filter).Project(projection).FirstOrDefaultAsync();
            return readingChallenge;
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error getting reading challenge by year {year} for user {userId}");
            throw;
        }
    }

    // Create a new reading challenge
    public async Task<ReadingChallenge> AddReadingChallengeAsync(string userId, ReadingChallenge readingChallenge) {
        try {
            readingChallenge.Id = ObjectId.GenerateNewId().ToString();
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.Push(u => u.ReadingChallenges, readingChallenge);
            await _users.UpdateOneAsync(filter, update);
            return readingChallenge;
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error creating reading challenge for user {userId}");
            throw;
        }
    }

    // Update an existing reading challenge
    public async Task<ReadingChallenge?> UpdateReadingChallengeAsync(string userId, ReadingChallenge readingChallenge) {
        try {
            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, userId),
                Builders<User>.Filter.ElemMatch(u => u.ReadingChallenges, rc => rc.Id == readingChallenge.Id)
            );

            var update = Builders<User>.Update
                .Set("ReadingChallenges.$.Goal", readingChallenge.Goal)
                .Set("ReadingChallenges.$.Progress", readingChallenge.Progress)
                .Set("ReadingChallenges.$.UpdatedAt", DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0) {
                return readingChallenge;
            }

            return null;
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error updating reading challenge for user {userId}");
            throw;
        }
    }

    // Delete a reading challenge
    public async Task<bool> DeleteReadingChallengeAsync(string userId, string challengeId) {
        try {
            var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var update = Builders<User>.Update.PullFilter(u => u.ReadingChallenges,
                challenge => challenge.Id == challengeId);
            var result = await _users.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error deleting reading challenge {challengeId} for user {userId}");
            throw;
        }
    }
}