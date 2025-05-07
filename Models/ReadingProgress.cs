using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BestReads.Models;

public class ReadingProgress {

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("bookId")]
    public string? BookId { get; set; }

    [BsonElement("currentPage")]
    [Range(0, int.MaxValue, ErrorMessage = "Current page must not be negative")]
    public int CurrentPage { get; set; }

    [BsonElement("totalPages")]
    [Range(0, int.MaxValue, ErrorMessage = "Total pages must not be negative")]
    public int TotalPages { get; set; }

    [BsonElement("updatedAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}