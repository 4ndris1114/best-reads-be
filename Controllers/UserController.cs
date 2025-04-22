using BestReads.Repositories;
using Microsoft.AspNetCore.Mvc;
using BestReads.Models;

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
/// Get a specific user
/// </summary>
/// <param name="id">The unique identifier for the user</param>
/// <returns>A user object</returns>
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
/// Update a specific user
/// </summary>
/// <param name="id">The unique identifier for the user</param>
/// <param name="user">The user object to update</param>
/// <returns>A updated user object</returns>
    [HttpPut("{id}/edit")]
    public async Task<ActionResult<User>> EditUser(string id, User user) {
        try {
            var userById = await _userRepository.GetByIdAsync(id);
            if (userById != null) {
                user.Password = userById.Password;
            }
            var updatedUser = await _userRepository.EditUserAsync(id, user);
            if (updatedUser == null)
                return NotFound("User not found");
            return Ok(updatedUser);
        } catch (Exception ex) {
            _logger.LogError(ex, $"Error updating user with id {id}");
            return StatusCode(500, $"Couldn't update user with id {id}");
        }
    }
}