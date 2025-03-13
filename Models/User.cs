using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class User {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [Required]
    [BsonElement("username")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long."), MaxLength(20, ErrorMessage = "Username cannot exceed 20 characters")]
    public string? Username { get; set; }


    [EmailAddress(ErrorMessage = "Invalid email address")]
    [BsonElement("email")]
    public string? Email { get; set; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [BsonElement("password")]
    public string? Password { get; set; }

    [BsonElement("profilePicture")]
    public string? ProfilePicture { get; set; }

    [BsonElement("bio")]
    [MaxLength(200, ErrorMessage = "Bio cannot exceed 200 characters")]
    public string? Bio { get; set; }

    [BsonElement("bookshelves")]
    public List<Bookshelf>? Bookshelves { get; set; }

    [BsonElement("readingProgress")]
    public List<ReadingProgress>? ReadingProgress { get; set; }

    [BsonElement("followers")]
    public List<string>? Followers { get; set; }

    [BsonElement("following")]
    public List<string>? Following { get; set; }

    [BsonElement("readingStats")]
    public ReadingStats? ReadingStats { get; set; }

    [BsonElement("createdAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}