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

    [BsonElement("payload")]
    public object Payload { get; set; } = new object();

    [BsonElement("likes")]
    public List<string> Likes { get; set; } = new List<string>();

    [BsonElement("comments")]
    public List<Comment> Comments { get; set; } = new List<Comment>();

    public enum ActivityType {
        AddedBookToShelf,
        RatedBook
    }
}
