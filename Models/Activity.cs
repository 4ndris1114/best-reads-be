using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BestReads.Models;

public class Activity {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    [BsonElement("userId")]
    public ObjectId UserId { get; set; }

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty; // "review", "rating", "shelf_update", "progress_update"

    [BsonElement("bookId")]
    public ObjectId BookId { get; set; }

    [BsonElement("content")]
    
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty; // Review text, rating, or update message

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
