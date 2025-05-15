using BestReads.Repositories;
using Microsoft.AspNetCore.Mvc;
using BestReads.Models;
using Microsoft.AspNetCore.Authorization;

namespace BestReads.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly StatsRepository _statsRepository;

    public StatsController(StatsRepository statsRepository)
    {
        _statsRepository = statsRepository;
    }
    /// <summary>
    /// Get all reading progress.
    /// </summary>
    /// <param name="userId">The unique identifier for the user</param>
    /// <returns>A list of reading progress</returns>
    [Authorize]
    [HttpGet("{userId}")]
    public async Task<ActionResult<ReadingProgress>> GetAllReadingProgress(string userId)
    {
        try
        {
            var readingProgress = await _statsRepository.GetAllReadingProgressAsync(userId);
            if (readingProgress == null)
                return NotFound("Reading progress not found");
            return Ok(readingProgress);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting reading progress: {ex.Message}");
        }
    }
    /// <summary>
    /// Get reading progress by ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="progressId"></param>
    /// <returns> A reading progress</returns>
    [HttpGet("{userId}/progress/{progressId}")]
    public async Task<ActionResult<ReadingProgress>> GetReadingProgressById(string userId, string progressId)
    {
        try
        {
            var readingProgress = await _statsRepository.GetReadingProgressByIdAsync(userId, progressId);
            if (readingProgress == null)
                return NotFound("Reading progress not found");
            return Ok(readingProgress);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error getting reading progress by Id: {ex.Message}");
        }
    }
    /// <summary>
    /// Add reading progress.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="readingProgress"></param>
    /// <returns> A newly added reading progress</returns>
    [HttpPost("{userId}/add")]
    public async Task<ActionResult<ReadingProgress>> AddReadingProgress(string userId, ReadingProgress readingProgress)
    {
        try
        {
            var addedStats = await _statsRepository.AddReadingProgressAsync(userId, readingProgress);
            if (addedStats == null)
                return NotFound("User not found");
            return Ok(addedStats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error adding reading progress: {ex.Message}");
        }
    }
    /// <summary>
    /// Update reading progress.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="progressId"></param>
    /// <param name="readingProgress"></param>
    /// <returns> An updated reading progress</returns>
    [Authorize]
    [HttpPut("{userId}/edit/{progressId}")]
    public async Task<ActionResult<ReadingProgress>> UpdateReadingProgress(string userId, string progressId, [FromBody] ReadingProgress readingProgress)
    {
        try
        {
            if (readingProgress == null || progressId != readingProgress.Id)
            {
                return BadRequest("Invalid reading progress data.");
            }

            if (readingProgress.CurrentPage > readingProgress.TotalPages)
            {
                return BadRequest("Current page cannot exceed total pages.");
            }

            var updatedStats = await _statsRepository.UpdateReadingProgressAsync(userId, readingProgress);
            if (updatedStats == null)
                return NotFound("User or reading progress not found.");

            return Ok(updatedStats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating reading progress: {ex.Message}");
        }
    }
}