using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BestReads.Models;
public class Book {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("apiId")]
    [StringLength(50, MinimumLength = 1)]
    public string ApiId { get; set; } = string.Empty; // ID from external book API

    [BsonElement("title")]
    [StringLength(100, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [BsonElement("author")]
    [StringLength(100, MinimumLength = 1)]
    public string Author { get; set; } = string.Empty;

    [BsonElement("isbn")]
    [StringLength(20, MinimumLength = 10)]
    public string Isbn { get; set; } = string.Empty;

    [BsonElement("publishedDate")]
    public DateTime PublishedDate { get; set; } = DateTime.Now;

    [BsonElement("coverImage")]
    public string CoverImage { get; set; } = string.Empty;

    [BsonElement("genres")]
    public List<string> Genres { get; set; } = new();

    [BsonElement("ratings")]
    public List<Rating> Ratings { get; set; } = new();

    [BsonElement("averageRating")]
    [Range(0, 5)]
    public double AverageRating { get; set; }
}

