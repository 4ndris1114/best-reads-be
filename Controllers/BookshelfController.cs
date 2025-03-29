using BestReads.Models;
using BestReads.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace BestReads.Controllers;

[ApiController]
[Route("api/[controller]/{userId}")]
public class BookshelfController : ControllerBase {
    private readonly BookshelfRepository _bookshelfRepository;
    private readonly ILogger<BookshelfController> _logger;

    public BookshelfController(BookshelfRepository bookshelfRepository, ILogger<BookshelfController> logger) {
        _bookshelfRepository = bookshelfRepository;
        _logger = logger;
    }

    // GET: api/bookshelf/{userId}
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Bookshelf>>> GetAllBookshelves(string userId) {
        try {
            if (!ValidateInputs(out var missing, (userId, "userId"))) {
                return BadRequest($"Missing or empty required parameter: {missing}");
            }
            var bookshelves = await _bookshelfRepository.GetAllBookshelvesAsync(userId);
            return Ok(bookshelves);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to retrieve bookshelves for user {userId}");
            return StatusCode(500, "An error occurred while retrieving bookshelves.");
        }
    }

    // POST: api/bookshelf/{userId}
    [HttpPost]
    public async Task<ActionResult> CreateBookshelf(string userId, [FromBody] Bookshelf newShelf) {
        try {
            if (!ValidateInputs(out var missing, (userId, "userId"))) {
                return BadRequest($"Missing or empty required parameter: {missing}");
            }
            if (newShelf == null || string.IsNullOrWhiteSpace(newShelf.Name)) {
                return BadRequest("Bookshelf must have a valid name.");
            }

            await _bookshelfRepository.CreateBookshelfAsync(userId, newShelf);
            return CreatedAtAction(nameof(GetAllBookshelves), new { userId }, newShelf);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to create bookshelf for user {userId}");
            return StatusCode(500, "An error occurred while creating the bookshelf.");
        }
    }

    // DELETE: api/bookshelf/{userId}/{bookshelfId}
    [HttpDelete("{shelfId}")]
    public async Task<ActionResult> DeleteBookshelf(string userId, string shelfId) {
        try {
            if (!ValidateInputs(out var missing, (userId, "userId"), (shelfId, "shelfId"))) {
                return BadRequest($"Missing or empty required parameter: {missing}");
            }
            await _bookshelfRepository.DeleteBookshelfAsync(userId, shelfId);
            return NoContent();
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to delete bookshelf {shelfId} for user {userId}");
            return StatusCode(500, "An error occurred while deleting the bookshelf.");
        }
    }

    // PUT: api/users/{userId}/bookshelves/{shelfId}/rename
    [HttpPut("{shelfId}/rename")]
    public async Task<ActionResult> RenameBookshelf(string userId, string shelfId, [FromBody] string newName) {
        try {
            if (!ValidateInputs(out var missing, (userId, "userId"), (shelfId, "shelfId"))) {
                return BadRequest($"Missing or empty required parameter: {missing}");
            }
            if (string.IsNullOrWhiteSpace(newName)) {
                return BadRequest("New name cannot be empty.");
            }

            await _bookshelfRepository.RenameBookshelfAsync(userId, shelfId, newName);
            return NoContent();
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to rename bookshelf {shelfId} for user {userId}");
            return StatusCode(500, "An error occurred while renaming the bookshelf.");
        }
    }

    // POST: api/bookshelf/{userId}/{shelfId}/books
    [HttpPost("{shelfId}/books")]
    public async Task<ActionResult> AddBookToBookshelf(string userId, string shelfId, [FromBody] string bookId) {
        try {
            if (!ValidateInputs(out var missing, (userId, "userId"), (shelfId, "shelfId"))) {
                return BadRequest($"Missing or empty required parameter: {missing}");
            }
            if (string.IsNullOrWhiteSpace(bookId)) {
                return BadRequest("Book ID is required.");
            }

            await _bookshelfRepository.AddBookToBookshelfAsync(userId, shelfId, bookId);
            return NoContent();
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to add book {bookId} to shelf {shelfId} for user {userId}");
            return StatusCode(500, "An error occurred while adding the book.");
        }
    }

    // DELETE: api/bookshelf/{userId}/{shelfId}/books/{bookId}
    [HttpDelete("{shelfId}/books/{bookId}")]
    public async Task<ActionResult> RemoveBookFromBookshelf(string userId, string shelfId, string bookId) {
        try {
            if (!ValidateInputs(out var missing, (userId, "userId"), (shelfId, "shelfId"), (bookId, "bookId"))) {
                return BadRequest($"Missing or empty required parameter: {missing}");
            }
            await _bookshelfRepository.RemoveBookFromBookshelfAsync(userId, shelfId, bookId);
            return NoContent();
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to remove book {bookId} from shelf {shelfId} for user {userId}");
            return StatusCode(500, "An error occurred while removing the book.");
        }
    }

    // PUT: api/bookshelf/{userId}/{sourceShelfId}/move/{bookId}/to/{targetShelfId}
    [HttpPut("{sourceShelfId}/move/{bookId}/to/{targetShelfId}")]
    public async Task<ActionResult> MoveBook(string userId, string sourceShelfId, string bookId, string targetShelfId) {
        try {
            if (!ValidateInputs(out var missing, (userId, "userId"), (sourceShelfId, "sourceShelfId"), (bookId, "bookId"), (targetShelfId, "targetShelfId"))) {
                return BadRequest($"Missing or empty required parameter: {missing}");
            }
            await _bookshelfRepository.MoveBookToAnotherBookshelfAsync(userId, sourceShelfId, bookId, targetShelfId);
            return NoContent();
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to move book {bookId} from shelf {sourceShelfId} to {targetShelfId} for user {userId}");
            return StatusCode(500, "An error occurred while moving the book.");
        }
    }

    private bool ValidateInputs(out string? missingParam, params (string? Value, string Name)[] inputs) {
        foreach (var (value, name) in inputs) {
            if (string.IsNullOrWhiteSpace(value)) {
                missingParam = name;
                return false;
            }
        }

        missingParam = null;
        return true;
    }
}
