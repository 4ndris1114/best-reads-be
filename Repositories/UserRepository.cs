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

    public async Task<User?> EditUserAsync(string id, User user) {
        try {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var update = Builders<User>.Update
                .Set(u => u.Username, user.Username)
                .Set(u => u.Email, user.Email);
            return await _users.FindOneAndUpdateAsync(filter, update);
        } catch (Exception ex) {
            throw new Exception("Error updating user", ex);
        }
    }

    public async Task<User?> GetByUsernameAsync(string username) {
        return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
    }
}