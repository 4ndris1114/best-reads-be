namespace BestReads.Models;

public class ReviewPayload {
    public string BookTitle { get; set; }
    public string CoverImage { get; set; }
    public double Rating { get; set; }
    public string ReviewText { get; set; }
    public bool IsUpdate { get; set; }
}
