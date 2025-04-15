using BestReads.Database;
using BestReads.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace BestReads.Repositories {

    public class RatingRepository {
        private readonly IMongoCollection<Book> _books;  // Access the books collection
        private readonly ILogger<RatingRepository> _logger;

        public RatingRepository(MongoDbContext dbContext, ILogger<RatingRepository> logger) {
            _logger = logger;
            _books = dbContext.Database.GetCollection<Book>("books");  // Use "books" collection
        }

        // Get all ratings for a book by bookId
        public async Task<List<Rating>> GetRatingsByBookIdAsync(string bookId) {
            try {
                var filter = Builders<Book>.Filter.Eq(b => b.Id, bookId);
                var book = await _books.Find(filter).FirstOrDefaultAsync();

                return book?.Ratings ?? new List<Rating>();  // Return ratings if book exists
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error getting ratings for book with id {bookId}");
                throw;
            }
        }

        // Add a rating to a book
        public async Task AddRatingToBookAsync(string bookId, Rating newRating) {
            try {
                var filter = Builders<Book>.Filter.Eq(b => b.Id, bookId);
                var update = Builders<Book>.Update.Push(b => b.Ratings, newRating);

                await _books.UpdateOneAsync(filter, update);
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error adding rating to book with id {bookId}");
                throw;
            }
        }

        // Update the rating of a specific user on a book
        public async Task UpdateUserRatingAsync(string bookId, string userId, Rating updatedRating) {
            try {
                var filter = Builders<Book>.Filter.And(
                    Builders<Book>.Filter.Eq(b => b.Id, bookId),
                    Builders<Book>.Filter.ElemMatch(b => b.Ratings, r => r.UserId == userId)
                );

                var update = Builders<Book>.Update.Set("ratings.$.rating", updatedRating.RatingValue); // Update specific user's rating

                await _books.UpdateOneAsync(filter, update);
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error updating rating for user {userId} on book {bookId}");
                throw;
            }
        }

        // Remove a rating from a book
        public async Task RemoveRatingFromBookAsync(string bookId, string userId) {
            try {
                var filter = Builders<Book>.Filter.Eq(b => b.Id, bookId);
                var update = Builders<Book>.Update.PullFilter(b => b.Ratings, r => r.UserId == userId);

                await _books.UpdateOneAsync(filter, update);
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error removing rating for user {userId} on book {bookId}");
                throw;
            }
        }

        // Get the average rating for a book
        public async Task<double?> GetAverageRatingForBookAsync(string bookId) {
            try {
                var filter = Builders<Book>.Filter.Eq(b => b.Id, bookId);
                var book = await _books.Find(filter).FirstOrDefaultAsync();

                return book?.AverageRating;  // Return the average rating if book exists
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error getting average rating for book {bookId}");
                throw;
            }
        }
    }
}
