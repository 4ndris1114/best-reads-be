using BestReads.Repositories;
using Microsoft.AspNetCore.Mvc;
using BestReads.Models;

namespace BestReads.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase {
    private readonly StatsRepository _statsRepository;

    public StatsController(StatsRepository statsRepository) {
        _statsRepository = statsRepository;
        }

    [HttpGet("{userId}")]
    public async Task<ActionResult<ReadingProgress>> GetAllReadingProgressById(string userId) {
        try{
            var readingProgress = await _statsRepository.GetAllReadingProgressAsync(userId);
            if(readingProgress == null) 
                return NotFound("Reading progress not found");
            return Ok(readingProgress);
        } catch (Exception ex) {
            return StatusCode(500, "Error getting reading progress");
        }
    }

    [HttpPut("{progressId}/update")]
    public async Task<ActionResult<ReadingProgress>> UpdateReadingProgress(string userId, ReadingProgress readingProgress) {
        try {
            var updatedStats = await _statsRepository.UpdateReadingProgressAsync(userId, readingProgress);
            if (updatedStats == null)
                return NotFound("User not found");
            return Ok(updatedStats);
            } catch (Exception ex) {
            return StatusCode(500, "Error updating reading progress");
        }
    }
}