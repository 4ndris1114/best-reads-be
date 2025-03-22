using BestReads.Models;
using BestReads.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using MongoDB.Bson;

namespace BestReads.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase {
    private readonly UserRepository _userRepository;
    private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;


    public AuthController(UserRepository userRepository, ILogger<AuthController> logger, IConfiguration configuration) {
        _userRepository = userRepository;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="user">The user details to register.</param>
    /// <returns>A success message if the registration is successful.</returns>
    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser(UserRegister user) {
        try {
            //validate user info
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                return BadRequest("Email and password are required");
            
            //check for existing email a.
            var existingUser = await _userRepository.GetByEmailAsync(user.Email);
            if (existingUser != null) {
                return BadRequest("Email already exists");
            } else {
                var existingUsername = await _userRepository.GetByUsernameAsync(user.Username);
                if (existingUsername != null) {
                    return BadRequest("Username already exists");
                }
            }
            
            //hash and salt pass
            user.Password = HashPassword(user.Password);

            var userToCreate = new User {
                Username = user.Username,
                Email = user.Email,
                Password = user.Password,
                ProfilePicture = "default_profile_picture.jpg",
                Bio = $"{user.Username}'s bio",
                Bookshelves = new List<Bookshelf> {
                    new Bookshelf { Id = ObjectId.GenerateNewId(), Name = "Currently Reading", Books = new List<ObjectId>() },
                    new Bookshelf { Id = ObjectId.GenerateNewId(), Name = "Want to Read", Books = new List<ObjectId>() },
                    new Bookshelf {Id = ObjectId.GenerateNewId(),  Name = "Read", Books = new List<ObjectId>() },
                },
                ReadingProgress = new List<ReadingProgress>(),
                Followers = new List<string>(),
                Following = new List<string>(),
                ReadingStats = new ReadingStats(),
            };

            await _userRepository.CreateAsync(userToCreate);
            return Ok("User registered successfully");
        } catch (Exception ex) {
            _logger.LogError(ex, "Error registering user");
            return StatusCode(500, "Error registering user");
        }
    }

    /// <summary>
    /// Logs in a registered user. 
    /// </summary>
    /// <param name="userLogin">The user details to log in.</param>
    /// <returns>A success message if the login is successful.</returns>
    [HttpPost("login")]
    public async Task<ActionResult> LoginUser(UserLogin userLogin) {
        try {
            //validate user info
            if (string.IsNullOrEmpty(userLogin.Email) || string.IsNullOrEmpty(userLogin.Password))
                return BadRequest("Email and password are required");
            
            //check for existing email
            var user = await _userRepository.GetByEmailAsync(userLogin.Email);
            if (user == null)
                return BadRequest("Email not found");

            //verify password
            if (!VerifyPassword(userLogin.Password, user.Password!))
                return BadRequest("Incorrect password");
            
            //create jwt token
            var token = GenerateJwtToken(user);
            return Ok(token);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error logging in user");
            return StatusCode(500, "Error logging in user");
        }
    }

    /// <summary>
    /// Verifies the validity of a JWT token.
    /// </summary>
    /// <returns>A success message if the token is valid.</returns>
    [HttpGet("verifyToken")]
    public IActionResult VerifyToken() {
        try {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                return Unauthorized("No token was provided");
            
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
                return Unauthorized("Invalid token");

            return Ok("Token is valid"); 
        } catch (Exception ex) {
            _logger.LogError(ex, "Error verifying token");
            return StatusCode(500, "Error verifying token");
        }
    }

    //helper methods:

    //method to hash and saltt password
    private string HashPassword(string password) {
        var salt = GenerateSalt();
        var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        // Store the salt and hash togethe
        var saltAndHash = Convert.ToBase64String(salt) + "." + hash;
        return saltAndHash;
    }

    //verify password method
    private bool VerifyPassword(string inputPassword, string storedSaltAndHash) {
        var parts = storedSaltAndHash.Split('.');
        if (parts.Length != 2) {
            return false; // Invalid stored format
        }

        var storedSalt = Convert.FromBase64String(parts[0]);
        var storedHash = parts[1];

        var hashedInputPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: inputPassword,
            salt: storedSalt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

        return hashedInputPassword == storedHash;
    }

    //generate salt
    private byte[] GenerateSalt() {
        var salt = new byte[16]; 
        using (var rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(salt);
        }
        return salt;
    }

    //generate jwt token
    private string GenerateJwtToken(User user) {
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", user.Id!.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_SECRET"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "localhost",
            audience: "localhost",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Get Principal from the expired JWT token
    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JWT_SECRET"]!);
        try {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false, // Allow expired token to be valid
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out var validatedToken);

            return principal;
        } catch {
            return null;
        }
    }
}