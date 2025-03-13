using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BestReads.Models;

public class Bookshelf{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    [BsonElement("name")]
    [MinLength(1, ErrorMessage = "Bookshelf name must be at least 1 character long"), MaxLength(50, ErrorMessage = "Bookshelf name must be at most 50 characters long")]
    public string? Name { get; set; }
    
    [BsonElement("books")]
    public List<ObjectId>? Books { get; set; }
}