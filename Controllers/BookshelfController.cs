using BestReads.Models;
using BestReads.Repositories;
using BestReads.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Linq;

namespace BestReads.Controllers;

[ApiController]
[Route("api/[controller]/{userId}")]
public class BookshelfController : ControllerBase
{
    private readonly BookshelfRepository _bookshelfRepository;
    private readonly BookRepository _bookRepository;
    private readonly ActivityService _activityService;

    private readonly ILogger<BookshelfController> _logger;

    public BookshelfController(BookshelfRepository bookshelfRepository, BookRepository bookRepository, ActivityService activityService, ILogger<BookshelfController> logger)
    {
        _bookshelfRepository = bookshelfRepository;
        _bookRepository = bookRepository;
        _activityService = activityService;
        _logger = logger;
    }

    /// GET: api/users/{userId}/bookshelves
    /// <summary>
    /// Get all bookshelves for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <returns>A list of bookshelves for the user.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Bookshelf>>> GetAllBookshelves(string userId)
    {
        try
        {
            if (!ValidateInputs(out var missing, (userId, "userId")))
            {
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            var bookshelves = await _bookshelfRepository.GetAllBookshelvesAsync(userId);
            return Ok(bookshelves);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to retrieve bookshelves for user {userId}");
            return StatusCode(500, "An error occurred while retrieving bookshelves.");
        }
    }

    // GET: api/bookshelf/{userId}/{bookshelfId}
    /// <summary>
    /// Get a specific bookshelf by ID.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="shelfId">The unique identifier for the bookshelf.</param>
    /// <returns>A bookshelf object</returns>
    [HttpGet("{shelfId}")]
    public async Task<ActionResult<Bookshelf>> GetBookshelf(string userId, string shelfId)
    {
        try
        {
            if (!ValidateInputs(out var missing, (userId, "userId"), (shelfId, "shelfId")))
            {
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            var bookshelf = await _bookshelfRepository.GetBookshelfByIdAsync(userId, shelfId);

            if (bookshelf == null)
            {
                return NotFound($"Bookshelf with ID {shelfId} not found for user {userId}");
            }
            return Ok(bookshelf);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to retrieve bookshelf {shelfId} for user {userId}");
            return StatusCode(500, "An error occurred while retrieving the bookshelf.");
        }
    }

    // POST: api/bookshelf/{userId}
    /// <summary>
    /// Create a new bookshelf for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="newShelf">The bookshelf object to create.</param>
    /// <returns>A newly created bookshelf object.</returns>
    [HttpPost]
    public async Task<ActionResult> CreateBookshelf(string userId, [FromBody] Bookshelf newShelf)
    {
        try
        {
            if (!ValidateInputs(out var missing, (userId, "userId")))
            {
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            if (newShelf == null || string.IsNullOrWhiteSpace(newShelf.Name))
            {
                return BadRequest("Bookshelf must have a valid name.");
            }
            var bookshelves = await _bookshelfRepository.GetAllBookshelvesAsync(userId);
            if (bookshelves.Any(b => b.Name == newShelf.Name))
            {
                return Conflict($"You already have a bookshelf named {newShelf.Name}.");
            }

            await _bookshelfRepository.CreateBookshelfAsync(userId, newShelf);
            return CreatedAtAction(nameof(GetAllBookshelves), new { userId }, newShelf);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create bookshelf for user {userId}");
            return StatusCode(500, "An error occurred while creating the bookshelf.");
        }
    }

    // DELETE: api/bookshelf/{userId}/{bookshelfId}
    /// <summary>
    /// Delete a specific bookshelf by ID.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="shelfId">The unique identifier for the bookshelf.</param>
    /// <returns></returns>
    [HttpDelete("{shelfId}")]
    public async Task<ActionResult> DeleteBookshelf(string userId, string shelfId)
    {
        try
        {
            if (!ValidateInputs(out var missing, (userId, "userId"), (shelfId, "shelfId")))
            {
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            var bookshelf = await _bookshelfRepository.GetBookshelfByIdAsync(userId, shelfId);
            if (bookshelf == null)
            {
                return NotFound($"Bookshelf with ID {shelfId} not found for user {userId}");
            }

            await _bookshelfRepository.DeleteBookshelfAsync(userId, shelfId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to delete bookshelf {shelfId} for user {userId}");
            return StatusCode(500, "An error occurred while deleting the bookshelf.");
        }
    }

    /// PUT: api/users/{userId}/bookshelves/{shelfId}/rename
    /// <summary>
    /// Rename a specific bookshelf by ID.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="shelfId">The unique identifier for the bookshelf.</param>
    /// <param name="newName">The new name for the bookshelf.</param>
    /// <returns></returns>
    [HttpPut("{shelfId}/rename")]
    public async Task<ActionResult> RenameBookshelf(string userId, string shelfId, [FromBody] string newName)
    {
        try
        {
            if (!ValidateInputs(out var missing, (userId, "userId"), (shelfId, "shelfId")))
            {
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            if (string.IsNullOrWhiteSpace(newName))
            {
                return BadRequest("New name cannot be invalid.");
            }
            var bookshelves = await _bookshelfRepository.GetAllBookshelvesAsync(userId);
            if (bookshelves.Any(b => b.Name == newName))
            {
                return Conflict($"You already have a bookshelf named {newName}.");
            }

            await _bookshelfRepository.RenameBookshelfAsync(userId, shelfId, newName);
            return Ok(newName);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to rename bookshelf {shelfId} for user {userId}");
            return StatusCode(500, "An error occurred while renaming the bookshelf.");
        }
    }

    /// POST: api/bookshelf/{userId}/{shelfId}/books
    /// <summary>
    /// Add a book to a specific bookshelf.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="shelfId">The unique identifier for the bookshelf.</param>
    /// <param name="bookId">The unique identifier for the book.</param>
    /// <returns></returns>
    [HttpPost("{shelfId}/books")]
    public async Task<ActionResult> AddBookToBookshelf(string userId, string shelfId, [FromBody] string bookId)
    {
        try
        {
            if (!ValidateInputs(out var missing, (userId, "userId"), (shelfId, "shelfId")))
            {
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            if (string.IsNullOrWhiteSpace(bookId))
            {
                return BadRequest("Book ID is required.");
            }

            // Check if the book exists
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                return NotFound($"Book with ID '{bookId}' does not exist.");
            }

            var bookshelf = await _bookshelfRepository.GetBookshelfByIdAsync(userId, shelfId);
            if (bookshelf?.Books != null && bookshelf.Books.Contains(bookId)) {
                return BadRequest($"Book with ID '{bookId}' is already in the bookshelf.");
            }

            var result = await _bookshelfRepository.AddBookToBookshelfAsync(userId, shelfId, bookId);
            
            if (result) {
                await _activityService.LogBookAddedToShelfAsync(userId, bookId, book.Title, book.CoverImage, false, null, bookshelf.Name);
            }
            return Ok(bookId);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to add book {bookId} to shelf {shelfId} for user {userId}");
            return StatusCode(500, "An error occurred while adding the book.");
        }
    }

    // DELETE: api/bookshelf/{userId}/{shelfId}/books/{bookId}
    /// <summary>
    /// Remove a book from a specific bookshelf.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="shelfId">The unique identifier for the bookshelf.</param>
    /// <param name="bookId">The unique identifier for the book.</param>
    /// <returns></returns>
    [HttpDelete("{shelfId}/books/{bookId}")]
    public async Task<ActionResult> RemoveBookFromBookshelf(string userId, string shelfId, string bookId)
    {
        try
        {
            if (!ValidateInputs(out var missing, (userId, "userId"), (shelfId, "shelfId"), (bookId, "bookId")))
            {
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            var bookShelf = await _bookshelfRepository.GetBookshelfByIdAsync(userId, shelfId);
            if (bookShelf?.Books != null && !bookShelf.Books.Contains(bookId))
            {
                return Conflict($"Book with ID {bookId} is not part of bookshelf {shelfId}.");
            }

            await _bookshelfRepository.RemoveBookFromBookshelfAsync(userId, shelfId, bookId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to remove book {bookId} from shelf {shelfId} for user {userId}");
            return StatusCode(500, "An error occurred while removing the book.");
        }
    }

    // PUT: api/bookshelf/{userId}/{sourceShelfId}/move/{bookId}/to/{targetShelfId}
    /// <summary>
    /// Move a book from one bookshelf to another.
    /// </summary>
    /// <param name="userId">The unique identifier for the user.</param>
    /// <param name="sourceShelfId">The unique identifier for the source bookshelf.</param>
    /// <param name="bookId">The unique identifier for the book.</param>
    /// <param name="targetShelfId">The unique identifier for the target bookshelf.</param>
    /// <returns></returns>
    [HttpPut("{sourceShelfId}/move/{bookId}/to/{targetShelfId}")]
    public async Task<ActionResult> MoveBookToAnotherBookshelf(string userId, string sourceShelfId, string bookId, string targetShelfId)
    {
        try
        {
            if (!ValidateInputs(out var missing, (userId, "userId"), (sourceShelfId, "sourceShelfId"), (bookId, "bookId"), (targetShelfId, "targetShelfId")))
            {
                return BadRequest($"Missing or invalid required parameter: {missing}");
            }
            // Check if the book exists
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                return NotFound($"Book with ID '{bookId}' does not exist.");
            }

            var sourceShelf = await _bookshelfRepository.GetBookshelfByIdAsync(userId, sourceShelfId);
            var targetShelf = await _bookshelfRepository.GetBookshelfByIdAsync(userId, targetShelfId);
            if (sourceShelf?.Books != null && !sourceShelf.Books.Contains(bookId))
            {
                return Conflict($"Book with ID {bookId} is not part of bookshelf {sourceShelfId}.");
            }
            if (targetShelf?.Books != null && targetShelf.Books.Contains(bookId))
            {
                return Conflict($"Book with ID {bookId} is already in bookshelf {targetShelfId}.");
            }
            var result = await _bookshelfRepository.MoveBookToAnotherBookshelfAsync(userId, sourceShelfId, bookId, targetShelfId);
            
            if (result) {
                await _activityService.LogBookAddedToShelfAsync(userId, bookId, book.Title, book.CoverImage, true, sourceShelf.Name, targetShelf.Name);
            }
            return Ok(bookId);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Failed to move book {bookId} from shelf {sourceShelfId} to {targetShelfId} for user {userId}");
            return StatusCode(500, "An error occurred while moving the book.");
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
