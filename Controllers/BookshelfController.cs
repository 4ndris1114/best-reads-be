using BestReads.Models;
using BestReads.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Linq;

namespace BestReads.Controllers;

[ApiController]
[Route("api/[controller]/{userId}")]
public class BookshelfController : ControllerBase {
    private readonly BookshelfRepository _bookshelfRepository;
    private readonly BookRepository _bookRepository;

    private readonly ILogger<BookshelfController> _logger;

    public BookshelfController(BookshelfRepository bookshelfRepository, BookRepository bookRepository, ILogger<BookshelfController> logger) {
        _bookshelfRepository = bookshelfRepository;
        _bookRepository = bookRepository;
        _logger = logger;
    }

    // GET: api/bookshelf/{userId}
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Bookshelf>>> GetAllBookshelves(string userId) {
        try {
            if (!ValidateInputs(out var missing, (userId, "userId"))) {
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            var bookshelves = await _bookshelfRepository.GetAllBookshelvesAsync(userId);
            return Ok(bookshelves);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to retrieve bookshelves for user {userId}");
            return StatusCode(500, "An error occurred while retrieving bookshelves.");
        }
    }

    // GET: api/bookshelf/{userId}/{bookshelfId}
    [HttpGet("{shelfId}")]
    public async Task<ActionResult<Bookshelf>> GetBookshelf(string userId, string shelfId) {
        try {
            if (!ValidateInputs(out var missing, (userId, "userId"), (shelfId, "shelfId"))) {
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            var bookshelf = await _bookshelfRepository.GetBookshelfByIdAsync(userId, shelfId);

            if (bookshelf == null) {
                return NotFound($"Bookshelf with ID {shelfId} not found for user {userId}");
            }
            return Ok(bookshelf);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to retrieve bookshelf {shelfId} for user {userId}");
            return StatusCode(500, "An error occurred while retrieving the bookshelf.");
        }
    }

    // POST: api/bookshelf/{userId}
    [HttpPost]
    public async Task<ActionResult> CreateBookshelf(string userId, [FromBody] Bookshelf newShelf) {
        try {
            if (!ValidateInputs(out var missing, (userId, "userId"))) {
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            if (newShelf == null || string.IsNullOrWhiteSpace(newShelf.Name)) {
                return BadRequest("Bookshelf must have a valid name.");
            }
            var bookshelves = await _bookshelfRepository.GetAllBookshelvesAsync(userId);
            if (bookshelves.Any(b => b.Name == newShelf.Name)) {
                return Conflict($"You already have a bookshelf named {newShelf.Name}.");
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
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            var bookshelf = await _bookshelfRepository.GetBookshelfByIdAsync(userId, shelfId);
            if (bookshelf == null) {
                return NotFound($"Bookshelf with ID {shelfId} not found for user {userId}");
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
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            if (string.IsNullOrWhiteSpace(newName)) {
                return BadRequest("New name cannot be invalid.");
            }
            var bookshelves = await _bookshelfRepository.GetAllBookshelvesAsync(userId);
            if (bookshelves.Any(b => b.Name == newName)) {
                return Conflict($"You already have a bookshelf named {newName}.");
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
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            if (string.IsNullOrWhiteSpace(bookId)) {
                return BadRequest("Book ID is required.");
            }

            // Check if the book exists
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null) {
                return NotFound($"Book with ID '{bookId}' does not exist.");
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
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            var bookShelf = await _bookshelfRepository.GetBookshelfByIdAsync(userId, shelfId);
            if (bookShelf?.Books != null && !bookShelf.Books.Contains(bookId)) {
                return Conflict($"Book with ID {bookId} is not part of bookshelf {shelfId}.");
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
                return BadRequest($"Missing or invalid required parameter: {missing}");
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
            if (string.IsNullOrWhiteSpace(value) || !ObjectId.TryParse(value, out _)) {
                missingParam = name;
                return false;
            }
        }

        missingParam = null;
        return true;
    }
}
