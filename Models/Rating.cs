using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Rating {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserId { get; set; }

    [Range(1, 5)]
    public double RatingValue { get; set; }

    [MaxLength(1000)]
    public string Review { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
