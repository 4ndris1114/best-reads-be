using System.Text.Json.Serialization;
using System.Text.Json;

namespace BestReads.Models.DTOs;

public class OpenLibrarySearchResponse {
    [JsonPropertyName("docs")]
    public List<OpenLibrarySearchDoc> Docs { get; set; } = new();
}

public class OpenLibrarySearchDoc {
    [JsonPropertyName("key")]
    public string WorkKey { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("author_name")]
    public List<string> AuthorName { get; set; } = new();

    [JsonPropertyName("first_publish_year")]
    public int? FirstPublishYear { get; set; }

    [JsonPropertyName("cover_edition_key")]
    public string CoverEditionKey { get; set; } = string.Empty;
}

public class WorkDetails {
    [JsonPropertyName("description")]
    public JsonElement? Description { get; set; }

    [JsonPropertyName("subjects")]
    public List<string>? Subjects { get; set; }
}

public class EditionDetails {
    [JsonPropertyName("number_of_pages")]
    public int? NumberOfPages { get; set; }

    [JsonPropertyName("isbn_10")]
    public List<string>? Isbn10 { get; set; }

    [JsonPropertyName("isbn_13")]
    public List<string>? Isbn13 { get; set; }

    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; set; }

    [JsonPropertyName("covers")]
    public List<int>? Covers { get; set; }
}
