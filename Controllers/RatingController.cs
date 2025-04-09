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
    [Route("api/[controller]/{userId}")]
    public class RatingController : ControllerBase
    {
        private readonly RatingRepository _ratingRepository;
        private readonly BookRepository _bookRepository;
        private readonly ILogger<RatingController> _logger;

        public RatingController(RatingRepository ratingRepository, BookRepository bookRepository, ILogger<RatingController> logger)
        {
            _ratingRepository = ratingRepository;
            _bookRepository = bookRepository;
            _logger = logger;
        }

        // GET: api/rating/{userId}/book/{bookId}
        /// <summary>
        /// Get all ratings for a specific book
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <returns>A list of ratings</returns>
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<Rating>>> GetRatingsForBook(string userId, string bookId)
        {
            try
            {
                if (!ValidateInputs(out var missing, (userId, "userId"), (bookId, "bookId")))
                {
                    return BadRequest($"Missing or invalid required parameter: {missing}");
                }

                var ratings = await _ratingRepository.GetRatingsByBookIdAsync(bookId);
                return Ok(ratings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve ratings for book {bookId} for user {userId}");
                return StatusCode(500, "An error occurred while retrieving ratings.");
            }
        }

        // POST: api/rating/{userId}/book/{bookId}
        /// <summary>
        /// Add a rating to a specific book
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <param name="newRating">The rating object to add.</param>
        /// <returns></returns>
        [HttpPost("book/{bookId}")]
        public async Task<ActionResult> AddRatingToBook(string userId, string bookId, [FromBody] Rating newRating)
        {
            try
            {
                if (!ValidateInputs(out var missing, (userId, "userId"), (bookId, "bookId")))
                {
                    return BadRequest($"Missing or invalid required parameter: {missing}");
                }
                if (newRating == null || newRating.RatingValue < 1 || newRating.RatingValue > 5)
                {
                    return BadRequest("Invalid rating value. Rating must be between 1 and 5.");
                }

                // Check if the book exists
                var book = await _bookRepository.GetByIdAsync(bookId);
                if (book == null)
                {
                    return NotFound($"Book with ID '{bookId}' does not exist.");
                }

                // Add the rating
                await _ratingRepository.AddRatingToBookAsync(bookId, newRating);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to add rating to book {bookId} for user {userId}");
                return StatusCode(500, "An error occurred while adding the rating.");
            }
        }

        // PUT: api/rating/{userId}/book/{bookId}
        /// <summary>
        /// Update the rating of a specific user on a specific book
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <param name="updatedRating">The updated rating object.</param>
        /// <returns></returns>
        [HttpPut("book/{bookId}")]
        public async Task<ActionResult> UpdateUserRating(string userId, string bookId, [FromBody] Rating updatedRating)
        {
            try
            {
                if (!ValidateInputs(out var missing, (userId, "userId"), (bookId, "bookId")))
                {
                    return BadRequest($"Missing or invalid required parameter: {missing}");
                }
                if (updatedRating == null || updatedRating.RatingValue < 1 || updatedRating.RatingValue > 5)
                {
                    return BadRequest("Invalid rating value. Rating must be between 1 and 5.");
                }

                // Check if the book exists
                var book = await _bookRepository.GetByIdAsync(bookId);
                if (book == null)
                {
                    return NotFound($"Book with ID '{bookId}' does not exist.");
                }

                // Update the rating
                await _ratingRepository.UpdateUserRatingAsync(bookId, userId, updatedRating);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update rating for book {bookId} by user {userId}");
                return StatusCode(500, "An error occurred while updating the rating.");
            }
        }

        // DELETE: api/rating/{userId}/book/{bookId}
        /// <summary>
        /// Remove the rating of a specific user on a specific book
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <returns></returns>
        [HttpDelete("book/{bookId}")]
        public async Task<ActionResult> RemoveUserRating(string userId, string bookId)
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

                // Remove the rating
                await _ratingRepository.RemoveRatingFromBookAsync(bookId, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to remove rating for book {bookId} by user {userId}");
                return StatusCode(500, "An error occurred while removing the rating.");
            }
        }

        // GET: api/rating/{userId}/book/{bookId}/average
        /// <summary>
        /// Get the average rating for a specific book
        /// </summary>
        /// <param name="userId">The unique identifier for the user.</param>
        /// <param name="bookId">The unique identifier for the book.</param>
        /// <returns></returns>
        [HttpGet("book/{bookId}/average")]
        public async Task<ActionResult<double?>> GetAverageRatingForBook(string userId, string bookId)
        {
            try
            {
                if (!ValidateInputs(out var missing, (userId, "userId"), (bookId, "bookId")))
                {
                    return BadRequest($"Missing or invalid required parameter: {missing}");
                }

                var avgRating = await _ratingRepository.GetAverageRatingForBookAsync(bookId);
                if (avgRating == null)
                {
                    return NotFound($"No ratings found for book {bookId}.");
                }

                return Ok(avgRating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve average rating for book {bookId} for user {userId}");
                return StatusCode(500, "An error occurred while retrieving the average rating.");
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
