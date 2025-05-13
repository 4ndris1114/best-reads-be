using BestReads.Models;
using BestReads.Repositories;
using BestReads.Services;
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
        private readonly ActivityService _activityService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(ReviewRepository reviewRepository, BookRepository bookRepository, ActivityService activityService, ILogger<ReviewController> logger)
        {
            _reviewRepository = reviewRepository;
            _bookRepository = bookRepository;
            _activityService = activityService;
            _logger = logger;
        }

        // GET: api/review/book/{bookId}
        /// <summary>
        /// Get all reviews for a specific book.
        /// </summary>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <returns>A list of reviews</returns>
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsByBookId(string bookId)
        {
            try
            {
                if (!ValidateInputs(out var missing, (bookId, "bookId")))
                {
                    return BadRequest($"Missing or invalid required parameter: {missing}");
                }

                var reviews = await _reviewRepository.GetReviewsByBookIdAsync(bookId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve reviews for book {bookId}");
                return StatusCode(500, "An error occurred while retrieving reviews.");
            }
        }

        // POST: api/review/book/{bookId}
        /// <summary>
        /// Add a review to a specific book.
        /// </summary>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <param name="newReview">The review object to add.</param>
        /// <returns></returns>
        [HttpPost("book/{bookId}")]
        public async Task<ActionResult> PostReview(string bookId, [FromBody] Review newReview)
        {
            try
            {
                if (!ValidateInputs(out var missing, (bookId, "bookId")))
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
                newReview.Id = ObjectId.GenerateNewId().ToString();
                var result = await _reviewRepository.PostReviewAsync(bookId, newReview);

                if (result && newReview.IsPublic) {
                    await _activityService.LogBookReviewedAsync(newReview.UserId, bookId, book.Title, book.CoverImage, newReview.RatingValue, newReview.ReviewText, false);
                }
                return CreatedAtAction(nameof(GetReviewsByBookId), new { bookId }, newReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add review to book {bookId}");
                return StatusCode(500, "An error occurred while adding the review.");
            }
        }

        // // PUT: api/review/book/{bookId}
        // /// <summary>
        // /// Update the review of a specific user on a specific book.
        // /// </summary>
        // /// <param name="bookId">The unique identifier for the book.</param>
        // /// <param name="updatedReview">The updated review object.</param>
        // /// <returns></returns>
        [HttpPut("{reviewId}/book/{bookId}")]
        public async Task<ActionResult> UpdateReview(string reviewId, string bookId, [FromBody] Review updatedReview)
        {
            try
            {
                if (!ValidateInputs(out var missing, (bookId, "bookId"),(reviewId, "reviewId")))
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
                var result = await _reviewRepository.UpdateReviewAsync(bookId, reviewId, updatedReview);

                if (result && updatedReview.IsPublic) {
                    await _activityService.LogBookReviewedAsync(updatedReview.UserId, bookId, book.Title, book.CoverImage, updatedReview.RatingValue, updatedReview.ReviewText, true);
                }
                //return updated review and activityId (null if not updated)
                return Ok(updatedReview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update review for book {bookId}");
                return StatusCode(500, "An error occurred while updating the review.");
            }
        }

        // DELETE: api/review/{reviewId}/book/{bookId}
        /// <summary>
        /// Remove the review of a specific book.
        /// </summary>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <param name="reviewId">The unique identifier for the review.</param>
        /// <returns></returns>
        [HttpDelete("{reviewId}/book/{bookId}")]
        public async Task<ActionResult> DeleteReview(string reviewId, string bookId)
        {
            try
            {
                if (!ValidateInputs(out var missing, (bookId, "bookId"), (reviewId, "reviewId")))
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
                await _reviewRepository.DeleteReviewAsync(reviewId, bookId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to remove review for book {bookId}");
                return StatusCode(500, "An error occurred while removing the review.");
            }
        }

        // GET: api/review/book/{bookId}/average
        /// <summary>
        /// Get the average review for a specific book.
        /// </summary>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <returns></returns>
        [HttpGet("book/{bookId}/average")]
        public async Task<ActionResult<double?>> GetAverageRatingForBook(string bookId)
        {
            try
            {
                if (!ValidateInputs(out var missing, (bookId, "bookId")))
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
                _logger.LogError(ex, $"Failed to retrieve average review for book {bookId}");
                return StatusCode(500, "An error occurred while retrieving the average review.");
            }
        }
/// <summary>
/// Validate inputs.
/// </summary>
/// <param name="missingParam"></param>
/// <param name="inputs"></param>
/// <returns></returns>
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