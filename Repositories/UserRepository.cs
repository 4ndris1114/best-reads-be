using BestReads.Models;
using BestReads.Database;
using MongoDB.Driver;

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
                .Set(u => u.ProfilePicture, user.ProfilePicture)
                .Set(u => u.Following, user.Following);
            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };

            return await _users.FindOneAndUpdateAsync(filter, update, options);
        } catch (Exception ex) {
            throw new Exception("Error updating user", ex);
        }
    }

    public async Task<User?> GetByUsernameAsync(string username) {
        return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<User?> FollowUserAsync(string userId, string friendId) {
        var user = await GetByIdAsync(userId);
        var friend = await GetByIdAsync(friendId);

        if (user == null || friend == null) return null;

        if (user.Following.Contains(friendId)) return user; // Already following

        user.Following.Add(friendId);
        return await UpdateAsync(userId, user); // Could reuse underlying Mongo update logic
    }
}