using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using BestReads.Models;
using BestReads.Database;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace BestReads.Repositories;

public class ActivityRepository : BaseRepository<Activity>{
    public ActivityRepository(MongoDbContext context) : base(context, "activities") { }

    public async Task<List<Activity>> GetRecentActivitiesAsync(IEnumerable<string> followingUserIds, int limit = 20) {

        var filter = Builders<Activity>.Filter.In(a => a.UserId, followingUserIds);
        var sort = Builders<Activity>.Sort.Descending(a => a.CreatedAt);

        var activities = await getCollection()
            .Find(filter)
            .Sort(sort)
            .Limit(limit)
            .ToListAsync();

        return activities;
    }
    
    public async Task<Activity> GetActivityByUserAndBookIdAsync(string userId, string bookId) {
        var filter = Builders<Activity>.Filter.And(
            Builders<Activity>.Filter.Eq(a => a.UserId, userId),
            Builders<Activity>.Filter.Eq(a => a.BookId, bookId)
        );
        return await getCollection().Find(filter).FirstOrDefaultAsync();
    }

    public async Task<string?> AddActivityAsync(Activity activity) {
        try {
            await getCollection().InsertOneAsync(activity);

            if (activity.Id != null) {
                return activity.Id;
            } else {
                throw new Exception("Failed to insert activity");
            }
        } catch (Exception ex) {
            throw new Exception("Error adding activity", ex);
        }
    }

public async Task<string?> UpdateActivityAsync(string id, Activity activity)
{
    var filter = Builders<Activity>.Filter.Eq(a => a.Id, id);

    if (activity.Type == Activity.ActivityType.RatedBook)
    {
        // Deserialize Payload into expected type
        var json = JsonConvert.SerializeObject(activity.Payload);
        var reviewPayload = JsonConvert.DeserializeObject<ReviewPayload>(json);

        if (reviewPayload == null)
            throw new Exception("Invalid payload for RatedBook");

        var update = Builders<Activity>.Update
            .Set("Payload.Rating", reviewPayload.Rating)
            .Set("Payload.ReviewText", reviewPayload.ReviewText)
            .Set("Payload.IsUpdate", reviewPayload.IsUpdate)
            .Set(a => a.UpdatedAt, DateTime.UtcNow);

        var result = await getCollection().UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0 ? id : null;
    }

    throw new Exception("Unsupported activity type or payload");
}

}