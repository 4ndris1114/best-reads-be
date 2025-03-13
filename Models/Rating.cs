using System.ComponentModel.DataAnnotations;

public class Rating {
    public ObjectId UserId { get; set; }

    [Range(1, 5)]
    public double RatingValue { get; set; }

    [MaxLength(1000)]
    public string Review { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
