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
}
