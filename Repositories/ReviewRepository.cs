using BestReads.Database;
using BestReads.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace BestReads.Repositories {

    public class ReviewRepository {
        private readonly IMongoCollection<Book> _books;  // Access the books collection
        private readonly ILogger<ReviewRepository> _logger;

        public ReviewRepository(MongoDbContext dbContext, ILogger<ReviewRepository> logger) {
            _logger = logger;
            _books = dbContext.Database.GetCollection<Book>("books");  // Use "books" collection
        }

        // Get all reviews for a book by bookId
        public async Task<List<Review>> GetReviewsByBookIdAsync(string bookId) {
            try {
                var filter = Builders<Book>.Filter.Eq(b => b.Id, bookId);
                var book = await _books.Find(filter).FirstOrDefaultAsync();

                return book?.Reviews ?? new List<Review>();  // Return reviews if book exists
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error getting reviews for book with id {bookId}");
                throw;
            }
        }

        // Add a review to a book
        public async Task<bool> PostReviewAsync(string bookId, Review newReview) {
            try {
                var filter = Builders<Book>.Filter.Eq(b => b.Id, bookId);
                var update = Builders<Book>.Update.Push(b => b.Reviews, newReview);

                var result = await _books.UpdateOneAsync(filter, update);

                return result.MatchedCount > 0 && result.ModifiedCount > 0;
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error adding review to book with id {bookId}");
                throw;
            }
        }

        public async Task<bool> UpdateReviewAsync(string bookId, string reviewId, Review updatedReview) {
            try {
                var filter = Builders<Book>.Filter.And(
                    Builders<Book>.Filter.Eq(b => b.Id, bookId),
                    Builders<Book>.Filter.ElemMatch(b => b.Reviews, r => r.Id == reviewId)
                );

                var update = Builders<Book>.Update
                    .Set("Reviews.$.RatingValue", updatedReview.RatingValue)
                    .Set("Reviews.$.ReviewText", updatedReview.ReviewText)
                    .Set("Reviews.$.isPublic", updatedReview.isPublic)
                    .Set("Reviews.$.UpdatedAt", DateTime.UtcNow);

                var result = await _books.UpdateOneAsync(filter, update);

                return result.MatchedCount > 0 && result.ModifiedCount > 0;
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error updating review {reviewId} on book {bookId}");
                throw;
            }
        }

        // Remove a review from a book
        public async Task DeleteReviewAsync(string reviewId, string bookId) {
            try {
                var filter = Builders<Book>.Filter.Eq(b => b.Id, bookId);
                var update = Builders<Book>.Update.PullFilter(b => b.Reviews, r => r.Id == reviewId);

                await _books.UpdateOneAsync(filter, update);
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error removing review on book {bookId}");
                throw;
            }
        }

        // Get the average review for a book
        public async Task<double?> GetAverageRatingForBookAsync(string bookId) {
            try {
                var filter = Builders<Book>.Filter.Eq(b => b.Id, bookId);
                var book = await _books.Find(filter).FirstOrDefaultAsync();

                return book?.AverageRating;  // Return the average review if book exists
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error getting average rating for book {bookId}");
                throw;
            }
        }
    }
}
