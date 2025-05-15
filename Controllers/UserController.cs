using BestReads.Repositories;
using Microsoft.AspNetCore.Mvc;
using BestReads.Models;
using BestReads.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace BestReads.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly UserRepository _userRepository;

    public UserController(ILogger<UserController> logger, UserRepository userRepository) {
        _logger = logger;
        _userRepository = userRepository;
    }
/// <summary>
/// Get a specific user by ID.
/// </summary>
/// <param name="id">The unique identifier for the user</param>
/// <returns>A user object</returns>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(string id) {
        try {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound("User not found");
            return Ok(user);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error getting user with id {id}");
            return StatusCode(500, $"Couldn't get user with id {id}");
        }
    }

/// <summary>
/// Get multiple users by their ids.
/// </summary>
/// <param name="ids">A comma-separated list of user ids</param>
/// <returns>A list of user objects</returns>
    [Authorize]
    [HttpGet("batch")]
    public async Task<IActionResult> GetUsersByIds([FromQuery] string ids) {
        if (string.IsNullOrEmpty(ids))
            return BadRequest("Ids query parameter is required.");

        var idList = ids.Split(',');
        var users = await _userRepository.GetUsersByIdsAsync(idList);

        return Ok(users);
    }

/// <summary>
/// Update a specific user.
/// </summary>
/// <param name="id">The unique identifier for the user</param>
/// <param name="user">The user object to update</param>
/// <returns>A updated user object</returns>
    [Authorize]
    [HttpPut("{id}/edit")]
    public async Task<ActionResult<User>> EditUser(string id, UpdateUserDTO user) {
        try {
            var updatedUser = await _userRepository.EditUserAsync(id, user);
            if (updatedUser == null)
                return NotFound("User not found");
            return Ok(updatedUser);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error updating user with id {id}");
            return StatusCode(500, $"Couldn't update user with id {id}");
        }
    }

/// <summary>
/// Follow a specific user.
/// </summary>
/// <param name="id">The unique identifier for the user</param>
/// <param name="friendId">The unique identifier for the friend</param>
/// <returns>An updated user object</returns>
    [HttpPost("{id}/follow/{friendId}")]
    public async Task<ActionResult<User>> FollowUser(string id, string friendId) {
        try {
            var updatedUser = await _userRepository.FollowUserAsync(id, friendId);
            if (updatedUser == null)
                return NotFound("User or friend not found");
            return Ok(updatedUser);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error following user with id {id}");
            return StatusCode(500, "Couldn't follow user");
        }
    }

/// <summary>
/// Unfollow a specific user.
/// </summary>
/// <param name="id">The unique identifier for the user</param>
/// <param name="friendId">The unique identifier for the friend</param>
/// <returns>An updated user object</returns>
    [HttpDelete("{id}/unfollow/{friendId}")]
    public async Task<ActionResult<User>> UnfollowUser(string id, string friendId) {
        try {
            var updatedUser = await _userRepository.UnfollowUserAsync(id, friendId);
            if (updatedUser == null)
                return NotFound("User or friend not found");
            return Ok(updatedUser);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error unfollowing user with id {id}");
            return StatusCode(500, "Couldn't unfollow user");
        }
    }

    /// <summary>
    /// Search for users by username.
    /// </summary>
    /// <param name="query">The username to search for</param>
    /// <returns>A list of users</returns>
    // GET /api/users/search?query=usernameToSearchFor
    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty.");

        var users = await _userRepository.SearchUsersByUsernameAsync(query);
        
        // avoid overfetching or exposing sensitive information
        var result = users.Select(u => new {
            u.Id,
            u.Username,
            u.ProfilePicture,
            u.Bio
        });

        return Ok(result);
    }
}