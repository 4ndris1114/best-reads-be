using MongoDB.Driver;
using BestReads.Models;
using BestReads.Models.DTOs;
using BestReads.Database;
using MongoDB.Bson;

namespace BestReads.Repositories;

public class BookRepository : BaseRepository<Book> {
    private readonly ILogger<BookRepository> _logger;
    public BookRepository(MongoDbContext dbContext, ILogger<BookRepository> logger) : base(dbContext, "books") {
        _logger = logger;
    }

    public async Task<IEnumerable<Book>> GetBooksByTitleAsync(string title) {
        var filter = Builders<Book>.Filter.Regex("Title", new BsonRegularExpression(title, "i")); // i -> case insensitive & regex for partial match 
        return await getCollection().Find(filter).ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(string author) {
        var filter = Builders<Book>.Filter.Regex("Author", new BsonRegularExpression(author, "i")); // i -> case insensitive & regex for partial match
        return await getCollection().Find(filter).ToListAsync();
    }

    public async Task<List<BookSearchDto>> SearchBooksAsync(string query) {
        try {
            query = query?.Trim().ToLower() ?? string.Empty;

            var filter = Builders<Book>.Filter.Or(
                Builders<Book>.Filter.Regex(b => b.Title, new BsonRegularExpression(query, "i")),
                Builders<Book>.Filter.Regex(b => b.Author, new BsonRegularExpression(query, "i")),
                Builders<Book>.Filter.Regex(b => b.Isbn, new BsonRegularExpression(query.Replace("-", ""), "i")),
                Builders<Book>.Filter.ElemMatch(b => b.Genres, g => g.ToLower().Contains(query))
            );

                var projection = Builders<Book>.Projection.Expression(book => new BookSearchDto {
                    Id = book.Id!.ToString(),
                    Title = book.Title,
                    Author = book.Author,
                    Genres = book.Genres,
                    CoverImage = book.CoverImage,
                    AverageRating = book.AverageRating
                });

                var results = await getCollection().Find(filter)
                    .Project(projection)
                    .Limit(20)
                    .ToListAsync();

                return results;
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error searching for books with query {query}");
            return new List<BookSearchDto>();
        }
    }
}