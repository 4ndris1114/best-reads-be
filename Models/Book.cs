using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BestReads.Models;
public class Book {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    [StringLength(50, MinimumLength = 1)]
    public string ApiId { get; set; } = string.Empty; // ID from external book API

    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 1)]
    public string Author { get; set; } = string.Empty;

    [StringLength(20, MinimumLength = 10)]
    public string Isbn { get; set; } = string.Empty;

    public string PublishedDate { get; set; } = string.Empty;

    [BsonElement("coverImage")]
    public string CoverImage { get; set; } = string.Empty;

    public List<string> Genres { get; set; } = new();

    public List<Rating> Ratings { get; set; } = new();

    [Range(0, 5)]
    public double AverageRating { get; set; }
}
