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
}