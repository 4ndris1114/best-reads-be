using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BestReads.Models;

public class Rating {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }

    [Range(1, 5)]
    public double RatingValue { get; set; }

    [MaxLength(1000)]
    public string Review { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
