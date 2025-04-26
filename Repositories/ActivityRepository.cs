using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using BestReads.Models;
using BestReads.Database;

namespace BestReads.Repositories;

public class ActivityRepository : BaseRepository<Activity>{
    public ActivityRepository(MongoDbContext context) : base(context, "activities") { }

    public async Task<List<Activity>> GetRecentActivitiesAsync(IEnumerable<string> followingUserIds, int limit = 20) {
        var filter = Builders<Activity>.Filter.In("UserId", followingUserIds);
        var sort = Builders<Activity>.Sort.Descending(a => a.CreatedAt);

        var activities = await getCollection()
            .Find(filter)
            .Sort(sort)
            .Limit(limit)
            .ToListAsync();

        return activities;
    }
}