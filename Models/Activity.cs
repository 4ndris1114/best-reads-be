using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BestReads.Models;

public class Activity {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public ObjectId UserId { get; set; }

    [BsonElement("type")]
    public ActivityType Type { get; set; }

    [BsonElement("bookId")]
    public ObjectId BookId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public object Payload { get; set; }

    public enum ActivityType {
        AddedBookToShelf,
        RatedBook
    }
}
