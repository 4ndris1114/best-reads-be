using BestReads.Repositories;
using Microsoft.AspNetCore.Mvc;
using BestReads.Models;

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

    [HttpGet("{userId}")]
    public async Task<ActionResult<ReadingProgress>> GetAllReadingProgress(string userId) {
        try {
            var readingProgress = await _statsRepository.GetAllReadingProgressAsync(userId);
            if (readingProgress == null)
                return NotFound("Reading progress not found");
            return Ok(readingProgress);
        }
        catch (Exception ex) {
            return StatusCode(500, "Error getting reading progress");
        }
    }

    [HttpGet("{progressId}")]
    public async Task<ActionResult<ReadingProgress>> GetReadingProgressById(string userId, string progressId) {
        try {
            var readingProgress = await _statsRepository.GetReadingProgressByIdAsync(userId, progressId);
            if (readingProgress == null)
                return NotFound("Reading progress not found");
            return Ok(readingProgress);
        }
        catch (Exception ex) {
            return StatusCode(500, "Error getting reading progress");
        }
    }

    [HttpPost("{userId}/add")]
    public async Task<ActionResult<ReadingProgress>> AddReadingProgress(string userId, ReadingProgress readingProgress) {
        try {
            var addedStats = await _statsRepository.AddReadingProgressAsync(userId, readingProgress);
            if (addedStats == null)
                return NotFound("User not found");
            return Ok(addedStats);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error adding reading progress: {ex.Message}");
        }
    }

    [HttpPut("{progressId}/update")]
    public async Task<ActionResult<ReadingProgress>> UpdateReadingProgress(string userId, ReadingProgress readingProgress) {
        try {
            var updatedStats = await _statsRepository.UpdateReadingProgressAsync(userId, readingProgress);
            if (updatedStats == null)
                return NotFound("User not found");
            return Ok(updatedStats);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error updating reading progress: {ex.Message}");
        }
    }
}