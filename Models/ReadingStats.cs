using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BestReads.Models;

public class ReadingStats {
    [BsonElement("booksReadThisYear")]
    public int booksReadThisYear { get; set; }

    [BsonElement("avgRatingGiven")]
    public double avgRatingGiven { get; set; }

    [BsonElement("pagesRead")]
    public int pagesRead { get; set; }
}