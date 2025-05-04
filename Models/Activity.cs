using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BestReads.Models;

public class Activity {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public string? UserId { get; set; }

    [BsonElement("type")]
    public ActivityType Type { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    // Optional: only used for certain types like RatedBook
    [BsonElement("bookId")]
    public string? BookId { get; set; }

    public object Payload { get; set; }

    public enum ActivityType {
        AddedBookToShelf,
        RatedBook
    }
}
