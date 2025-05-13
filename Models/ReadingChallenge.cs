using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BestReads.Models;

public class ReadingChallenge {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required]
    [BsonElement("year")]
    public int Year { get; set; }

    [Required]
    [BsonElement("goal")]
    public int Goal { get; set; }

    [Required]
    [BsonElement("progress")]
    public int Progress { get; set; }

    [BsonElement("updatedAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}