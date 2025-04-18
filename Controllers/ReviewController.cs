using BestReads.Models;
using BestReads.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BestReads.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewRepository _reviewRepository;
        private readonly BookRepository _bookRepository;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(ReviewRepository reviewRepository, BookRepository bookRepository, ILogger<ReviewController> logger)
        {
            _reviewRepository = reviewRepository;
            _bookRepository = bookRepository;
            _logger = logger;
        }

        // GET: api/review/{userId}/book/{bookId}
        /// <summary>
        /// Get all reviews for a specific book
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <returns>A list of reviews</returns>
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsForBook(string bookId)
        {
            try
            {
                if (!ValidateInputs(out var missing, (userId, "userId"), (bookId, "bookId")))
                {
                    return BadRequest($"Missing or invalid required parameter: {missing}");
                }

                var reviews = await _reviewRepository.GetReviewsByBookIdAsync(bookId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve reviews for book {bookId} for user {userId}");
                return StatusCode(500, "An error occurred while retrieving reviews.");
            }
        }

        // POST: api/review/{userId}/book/{bookId}
        /// <summary>
        /// Add a review to a specific book
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <param name="newReview">The review object to add.</param>
        /// <returns></returns>
        [HttpPost("book/{bookId}")]
        public async Task<ActionResult> AddReviewToBook(string bookId, [FromBody] Review newReview)
        {
            try
            {
                if (!ValidateInputs(out var missing, (userId, "userId"), (bookId, "bookId")))
                {
                    return BadRequest($"Missing or invalid required parameter: {missing}");
                }
                if (newReview == null || newReview.RatingValue < 1 || newReview.RatingValue > 5)
                {
                    return BadRequest("Invalid review value. Review must be between 1 and 5.");
                }

                // Check if the book exists
                var book = await _bookRepository.GetByIdAsync(bookId);
                if (book == null)
                {
                    return NotFound($"Book with ID '{bookId}' does not exist.");
                }

                // Add the review
                await _reviewRepository.AddReviewToBookAsync(bookId, newReview);
                return CreatedAtAction(nameof(GetReviewsForBook), new { userId, bookId }, newReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add review to book {bookId} for user {userId}");
                return StatusCode(500, "An error occurred while adding the review.");
            }
        }

        // PUT: api/review/{userId}/book/{bookId}
        /// <summary>
        /// Update the review of a specific user on a specific book
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <param name="updatedReview">The updated review object.</param>
        /// <returns></returns>
        [HttpPut("book/{bookId}")]
        public async Task<ActionResult> UpdateUserReview(string bookId, [FromBody] Review updatedReview)
        {
            try
            {
                if (!ValidateInputs(out var missing, (userId, "userId"), (bookId, "bookId")))
                {
                    return BadRequest($"Missing or invalid required parameter: {missing}");
                }
                if (updatedReview == null || updatedReview.RatingValue < 1 || updatedReview.RatingValue > 5)
                {
                    return BadRequest("Invalid review value. Review must be between 1 and 5.");
                }

                // Check if the book exists
                var book = await _bookRepository.GetByIdAsync(bookId);
                if (book == null)
                {
                    return NotFound($"Book with ID '{bookId}' does not exist.");
                }

                // Update the review
                await _reviewRepository.UpdateUserReviewAsync(bookId, userId, updatedReview);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update review for book {bookId} by user {userId}");
                return StatusCode(500, "An error occurred while updating the review.");
            }
        }

        // DELETE: api/review/{userId}/book/{bookId}
        /// <summary>
        /// Remove the review of a specific user on a specific book
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <returns></returns>
        [HttpDelete("book/{bookId}")]
        public async Task<ActionResult> RemoveUserReview(string bookId)
        {
            try
            {
                if (!ValidateInputs(out var missing, (userId, "userId"), (bookId, "bookId")))
                {
                    return BadRequest($"Missing or invalid required parameter: {missing}");
                }

                // Check if the book exists
                var book = await _bookRepository.GetByIdAsync(bookId);
                if (book == null)
                {
                    return NotFound($"Book with ID '{bookId}' does not exist.");
                }

                // Remove the review
                await _reviewRepository.RemoveReviewFromBookAsync(bookId, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to remove review for book {bookId} by user {userId}");
                return StatusCode(500, "An error occurred while removing the review.");
            }
        }

        // GET: api/review/{userId}/book/{bookId}/average
        /// <summary>
        /// Get the average review for a specific book
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <returns></returns>
        [HttpGet("book/{bookId}/average")]
        public async Task<ActionResult<double?>> GetAverageRatingForBook(string bookId)
        {
            try
            {
                if (!ValidateInputs(out var missing, (userId, "userId"), (bookId, "bookId")))
                {
                    return BadRequest($"Missing or invalid required parameter: {missing}");
                }

                var avgRating = await _reviewRepository.GetAverageRatingForBookAsync(bookId);
                if (avgRating == null)
                {
                    return NotFound($"No reviews found for book {bookId}.");
                }

                return Ok(avgRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve average review for book {bookId} for user {userId}");
                return StatusCode(500, "An error occurred while retrieving the average review.");
            }
        }

        private bool ValidateInputs(out string? missingParam, params (string? Value, string Name)[] inputs)
        {
            foreach (var (value, name) in inputs)
            {
                if (string.IsNullOrWhiteSpace(value) || !ObjectId.TryParse(value, out _))
                {
                    missingParam = name;
                    return false;
                }
            }

            missingParam = null;
            return true;
        }
    }
}
