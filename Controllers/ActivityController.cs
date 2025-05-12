using Microsoft.AspNetCore.Mvc;
using BestReads.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BestReads.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityController : ControllerBase {
    private readonly ActivityRepository _activityRepository;
    private readonly UserRepository _userRepository;

    public ActivityController(ActivityRepository activityRepository, UserRepository userRepository) {
        _activityRepository = activityRepository;
        _userRepository = userRepository;
    }
/// <summary>
/// Get the user's activity feed.
/// </summary>
/// <param name="skip"></param>
/// <param name="limit"></param>
/// <returns> A list of user's activities</returns>
    [HttpGet("feed")]
    [Authorize]
    public async Task<IActionResult> GetActivityFeed([FromQuery] int skip = 0, [FromQuery] int limit = 20) {
        var userId = User.FindFirstValue("userId");

        if (userId == null)
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound();

        var activities = await _activityRepository.GetRecentActivitiesAsync(user.Following, skip, limit);

        return Ok(activities);
    }
}