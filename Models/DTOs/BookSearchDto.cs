namespace BestReads.Models.DTOs;

public class BookSearchDto {
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public List<string>? Genres { get; set; }
    public string? CoverImage { get; set; }
    public double AverageRating { get; set; }
}