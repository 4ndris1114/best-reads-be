namespace BestReads.Models;

public class AddedBookToShelfPayload {
    public string? BookTitle { get; set; }
    public string? CoverImage { get; set; }
    public string? SourceShelfName { get; set; }
    public string? TargetShelfName { get; set; }
    public bool IsUpdate { get; set; } = false;
}
