using Microsoft.AspNetCore.Mvc;
using BestReads.Services;

namespace BestReads.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CloudinaryController : ControllerBase {
    private readonly CloudinaryService _cloudinaryService;

    public CloudinaryController(CloudinaryService cloudinaryService) {
        _cloudinaryService = cloudinaryService;
    }

    // Endpoint to get signature and timestamp
    [HttpGet("{userId}/generate-signature")]
    public IActionResult GenerateSignature(string userId) {
        try {
            var (signature, timestamp) = _cloudinaryService.GenerateSignature(userId);
            return Ok(new { signature, timestamp });
        }
        catch (Exception ex) {
            return BadRequest(new { message = "Error generating signature", error = ex.Message });
        }
    }
}
