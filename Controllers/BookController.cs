using System.ComponentModel.DataAnnotations;
using BestReads.Models;
using BestReads.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace BestReads.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookController : BaseController<Book> {

    private readonly BookRepository _bookRepository;
    private readonly ILogger<BookController> _logger;

    public BookController(BookRepository bookRepository, ILogger<BookController> logger)
        : base(bookRepository) {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public override async Task<ActionResult<IEnumerable<Book>>> GetAll() {
        try {
            var books = await _bookRepository.GetAllAsync();
            return Ok(books);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error fetching books");
            return StatusCode(500, "Error fetching books");
        }
    }

    public override async Task<ActionResult<Book>> GetById(string id) {
        try {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid book id format");

            var book = await _bookRepository.GetByIdAsync(id);
            return book == null ? NotFound("Book not found") : Ok(book);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error fetching book with id {id}");
            return StatusCode(500, $"Couldn't fetch book with id {id}");
        }
    }

    public override async Task<ActionResult<Book>> Create(Book book) {
        try {
            ValidateBook(book);

            await _bookRepository.CreateAsync(book);
            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        } catch (ValidationException ex) {
            return BadRequest(ex.Message);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error creating book");
            return StatusCode(500, "Error creating book");
        }
    }

    public override async Task<ActionResult<Book>> Update(string id, Book book) {
        try {
            if (GetById(id) == null)
                return BadRequest("Invalid book id format");
            ValidateBook(book);

            var existingBook = await _bookRepository.GetByIdAsync(id);
            if (existingBook == null)
                return NotFound("Book not found");
            
            await _bookRepository.UpdateAsync(id, book);
            return NoContent();
        } catch (ValidationException ex) {
            return BadRequest(ex.Message);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error updating book with id {id}");
            return StatusCode(500, $"Couldn't update book with id {id}");
        }
    }

    public override async Task<ActionResult<Book>> Delete(string id) {
        try {
            var existingBook = await _bookRepository.GetByIdAsync(id);
            if (existingBook == null)
                return NotFound("Book not found");
            
            await _bookRepository.DeleteAsync(id);
            return NoContent();
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error deleting book with id {id}");
            return StatusCode(500, $"Couldn't delete book with id {id}");
        }
    }

    [HttpGet("title/{title}")]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooksByTitle(string title) {
        try {
            var books = await _bookRepository.GetBooksByTitleAsync(title);
            return Ok(books);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error fetching books by title");
            return StatusCode(500, "Error fetching books by title");
        }
    }

    [HttpGet("author/{author}")]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooksByAuthor(string author) {
        try {
            var books = await _bookRepository.GetBooksByAuthorAsync(author);
            return Ok(books);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error fetching books by author");
            return StatusCode(500, "Error fetching books by author");
        }
    }

    //validation
    private void ValidateBook(Book book)
    {
        var context = new ValidationContext(book);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(book, context, results, true))
        {
            throw new ValidationException(string.Join("; ", results.Select(r => r.ErrorMessage)));
        }
    }
}