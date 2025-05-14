using BestReads.Models;
using BestReads.Models.DTOs;
using BestReads.Database;
using MongoDB.Driver;
using MongoDB.Bson;

namespace BestReads.Repositories;
public class UserRepository : BaseRepository<User> {
    private readonly IMongoCollection<User> _users;
    

    public UserRepository(MongoDbContext dbContext) : base(dbContext, "users") {
        _users = dbContext.Database.GetCollection<User>("users");
        
    }

    public async Task<User?> GetByEmailAsync(string email) {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> EditUserAsync(string id, UpdateUserDTO user) {
        try {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update
                .Set(u => u.Username, user.Username)
                .Set(u => u.Email, user.Email)
                .Set(u => u.Bio, user.Bio)
                .Set(u => u.ProfilePicture, user.ProfilePicture);
            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };

            return await _users.FindOneAndUpdateAsync(filter, update, options);
        } catch (Exception ex) {
            throw new Exception("Error updating user", ex);
        }
    }

    public async Task<List<UserSummaryDto>> GetUsersByIdsAsync(IEnumerable<string> ids) {
        var filter = Builders<User>.Filter.In(u => u.Id, ids);
        var projection = Builders<User>.Projection.Expression(u => new UserSummaryDto {
            Id = u.Id,
            Username = u.Username,
            ProfilePicture = u.ProfilePicture
        });

        var result = await _users.Find(filter)
                                            .Project(projection)
                                            .ToListAsync();

        return result;
    }

    public async Task<User?> GetByUsernameAsync(string username) {
        return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<User?> FollowUserAsync(string userId, string friendId) {
        using var session = await _users.Database.Client.StartSessionAsync();

        session.StartTransaction();

        try {
            var user = await _users.Find(session, u => u.Id == userId).FirstOrDefaultAsync();
            var friend = await _users.Find(session, u => u.Id == friendId).FirstOrDefaultAsync();

            if (user == null || friend == null) {
                await session.AbortTransactionAsync();
                return null;
            }

            var userUpdate = Builders<User>.Update.AddToSet(u => u.Following, friendId);
            var friendUpdate = Builders<User>.Update.AddToSet(u => u.Followers, userId);

            var userFilter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var friendFilter = Builders<User>.Filter.Eq(u => u.Id, friendId);

            await _users.UpdateOneAsync(session, userFilter, userUpdate);
            await _users.UpdateOneAsync(session, friendFilter, friendUpdate);

            await session.CommitTransactionAsync();

            // Optional: return the updated user
            return await _users.Find(u => u.Id == friendId).FirstOrDefaultAsync();
        } catch (Exception ex) {
            await session.AbortTransactionAsync();
            throw new Exception("Transaction failed while following user", ex);
        }
    }

    public async Task<User?> UnfollowUserAsync(string userId, string friendId) {
        using var session = await _users.Database.Client.StartSessionAsync();

        session.StartTransaction();

        try {
            var user = await _users.Find(session, u => u.Id == userId).FirstOrDefaultAsync();
            var friend = await _users.Find(session, u => u.Id == friendId).FirstOrDefaultAsync();

            if (user == null || friend == null) {
                await session.AbortTransactionAsync();
                return null;
            }

            var userUpdate = Builders<User>.Update.Pull(u => u.Following, friendId);
            var friendUpdate = Builders<User>.Update.Pull(u => u.Followers, userId);

            var userFilter = Builders<User>.Filter.Eq(u => u.Id, userId);
            var friendFilter = Builders<User>.Filter.Eq(u => u.Id, friendId);

            await _users.UpdateOneAsync(session, userFilter, userUpdate);
            await _users.UpdateOneAsync(session, friendFilter, friendUpdate);

            await session.CommitTransactionAsync();

            // Optional: return the updated user
            return await _users.Find(u => u.Id == friendId).FirstOrDefaultAsync();
        }
        catch (Exception ex) {
            await session.AbortTransactionAsync();
            throw new Exception("Transaction failed while unfollowing user", ex);
        }
    }

    public async Task<List<User>> SearchUsersByUsernameAsync(string query)
    {
        var filter = Builders<User>.Filter.Regex(u => u.Username, new BsonRegularExpression(query, "i")); // case-insensitive
        return await _users.Find(filter).Limit(10).ToListAsync();
    }
}