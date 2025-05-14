using System.Text.Json;
using BestReads.Models;
using BestReads.Models.DTOs;
using BestReads.Repositories;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.IO;

namespace BestReads.Services;

public class BookService {
    private readonly HttpClient _httpClient;
    private readonly BookRepository _bookRepository;
    private readonly Cloudinary _cloudinary;

    public BookService(BookRepository bookRepository) {
        _httpClient = new HttpClient();
        _bookRepository = bookRepository;

        // Initialize Cloudinary
        var cloudinaryAccount = new Account(
            Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME"),
            Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY"),
            Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
        );
        _cloudinary = new Cloudinary(cloudinaryAccount);
    }

    public async Task<Book?> SearchAndAddFromOpenLibraryAsync(string query) {
        var searchUrl = $"https://openlibrary.org/search.json?title={Uri.EscapeDataString(query)}&limit=1";

        try {
            var searchResponse = await _httpClient.GetAsync(searchUrl);
            if (!searchResponse.IsSuccessStatusCode) return null;

            var searchJson = await searchResponse.Content.ReadAsStringAsync();
            var searchData = JsonSerializer.Deserialize<OpenLibrarySearchResponse>(searchJson);

            var doc = searchData?.Docs.FirstOrDefault();
            if (doc == null) return null;

            var workKey = doc.WorkKey?.Replace("/works/", "");
            var editionKey = doc.CoverEditionKey;

            var workDetails = await GetJsonAsync<WorkDetails>($"https://openlibrary.org/works/{workKey}.json");
            var editionDetails = await GetJsonAsync<EditionDetails>($"https://openlibrary.org/books/{editionKey}.json");

            var book = new Book {
                ApiId = workKey!,
                Title = doc.Title!,
                Author = doc.AuthorName!.FirstOrDefault() ?? "Unknown",
                Description = CleanDescription(ExtractDescription(workDetails?.Description)),
                Genres = ExtractSubjects(workDetails),
                NumberOfPages = editionDetails?.NumberOfPages ?? 0,
                Isbn = editionDetails?.Isbn13?.FirstOrDefault() ?? editionDetails?.Isbn10?.FirstOrDefault() ?? "",
                PublishedDate = TryParseDate(editionDetails?.PublishDate),
                CoverImage = await UploadCoverImageToCloudinary(editionDetails?.Covers?.FirstOrDefault()),
                Reviews = new(),
                AverageRating = 0
            };

            await _bookRepository.CreateAsync(book);
            return book;
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    private async Task<string> UploadCoverImageToCloudinary(int? coverId) {
        if (!coverId.HasValue) return "";

        var coverUrl = $"https://covers.openlibrary.org/b/id/{coverId.Value}-L.jpg";
        var coverImageStream = await _httpClient.GetStreamAsync(coverUrl);

        // Upload to Cloudinary
        var uploadParams = new ImageUploadParams {
            File = new FileDescription("cover.jpg", coverImageStream)
        };
        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        return uploadResult?.SecureUrl?.ToString().Split('/').Last() ?? "";
    }
    
    private async Task<T?> GetJsonAsync<T>(string url) {
        var res = await _httpClient.GetAsync(url);
        if (!res.IsSuccessStatusCode) return default;
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }

    private DateTime TryParseDate(string? dateStr) {
        return DateTime.TryParse(dateStr, out var dt)
            ? dt
            : new DateTime(2000, 1, 1);
    }

    private string ExtractDescription(JsonElement? descriptionElement) {
        if (!descriptionElement.HasValue)
            return "";

        var element = descriptionElement.Value;

        return element.ValueKind switch {
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.Object when element.TryGetProperty("value", out var val) => val.GetString() ?? "",
            _ => ""
        };
    }

    private List<string> ExtractSubjects(WorkDetails? workDetails) {
        if (workDetails?.Subjects != null) {
            return workDetails.Subjects
                             .Where(s => !string.IsNullOrWhiteSpace(s))
                             .Take(5)
                             .ToList();
        }

        return new List<string>();
    }

    public static string CleanDescription(string rawDescription) {
        if (string.IsNullOrWhiteSpace(rawDescription))
            return "";

        rawDescription = Regex.Replace(rawDescription, @"\[[^\]]+\]\[\d+\]", "");
        rawDescription = Regex.Replace(rawDescription, @"\[\d+\]:\s*https?:\/\/\S+\r?\n?", "");
        rawDescription = rawDescription
            .Replace("\\r", " ")
            .Replace("\\n", " ")
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\t", " ");
        rawDescription = Regex.Replace(rawDescription, @"\s{2,}", " ").Trim();

        return rawDescription;
    }
}

