using BestReads.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BestReads.Models;

namespace BestReads.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingChallengeController :  ControllerBase {
    private readonly ReadingChallengeRepository _readingChallengeRepository;
    private readonly ILogger<ReadingChallengeController> _logger;

    public ReadingChallengeController(ReadingChallengeRepository readingChallengeRepository, ILogger<ReadingChallengeController> logger) {
        _readingChallengeRepository = readingChallengeRepository;
        _logger = logger;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<ReadingChallenge>> GetAllReadingChallenges(string userId) {
        try {
            var readingChallenges = await _readingChallengeRepository.GetAllReadingChallengesAsync(userId);
            if (readingChallenges == null)
                return NotFound("Reading challenge not found");
            return Ok(readingChallenges);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error getting reading challenge: {ex.Message}");
        }
    }

    [HttpGet("{userId}/{challengeId}")]
    public async Task<ActionResult<ReadingChallenge>> GetReadingChallengeById(string userId, string challengeId) {
        try {
            var readingChallenge = await _readingChallengeRepository.GetReadingChallengeByIdAsync(userId, challengeId);
            if (readingChallenge == null)
                return NotFound("Reading challenge not found");
            return Ok(readingChallenge);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error getting reading challenge by Id: {ex.Message}");
        }
    }

    [HttpGet("{userId}/year/{year}")]
    public async Task<ActionResult<ReadingChallenge>> GetReadingChallengeByYear(string userId, int year) {
        try {
            var readingChallenge = await _readingChallengeRepository.GetReadingChallengeByYearAsync(userId, year);
            if (readingChallenge == null)
                return NotFound("Reading challenge not found");
            return Ok(readingChallenge);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error getting reading challenge by year: {ex.Message}");
        }
    }

    [HttpPost("{userId}")]
    public async Task<ActionResult<ReadingChallenge>> AddReadingChallenge(string userId, [FromBody] ReadingChallenge readingChallenge) {
        try {
            var addedChallenge = await _readingChallengeRepository.AddReadingChallengeAsync(userId, readingChallenge);
            if (addedChallenge == null)
                return NotFound("User not found");
            return Ok(addedChallenge);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error adding reading challenge: {ex.Message}");
        }
    }

    [HttpPut("{userId}/{challengeId}")]
    public async Task<ActionResult<ReadingChallenge>> UpdateReadingChallenge(string userId, [FromBody] ReadingChallenge readingChallenge) {
        try {
            var updatedChallenge = await _readingChallengeRepository.UpdateReadingChallengeAsync(userId, readingChallenge);
            if (updatedChallenge == null)
                return NotFound("Reading challenge or User not found");
            return Ok(updatedChallenge);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error updating reading challenge: {ex.Message}");
        }
    }

    [HttpDelete("{userId}/{challengeId}")]
    public async Task<ActionResult<bool>> DeleteReadingChallenge(string userId, string challengeId) {
        try {
            var wasDeleted = await _readingChallengeRepository.DeleteReadingChallengeAsync(userId, challengeId);
            return Ok(wasDeleted);
        }
        catch (Exception ex) {
            return StatusCode(500, $"Error deleting reading challenge: {ex.Message}");
        }
    }
}