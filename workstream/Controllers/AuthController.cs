using Microsoft.AspNetCore.Mvc;
using workstream.Data;
using workstream.Services;
using workstream.DTO;

namespace workstream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserRepo _userRepo;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserRepo userRepo, JwtService jwtService, ILogger<AuthController> logger)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
            _logger = logger;
        }

        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var user = await _userRepo.GetUserByUsernameAsync(loginDto.Username);

            if (user == null)
            {
                _logger.LogWarning($"Failed login attempt for username: {loginDto.Username}");
                return Unauthorized("Invalid username or password.");
            }

            // Verify the password (this assumes you've stored the password hash, and comparing it)
            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning($"Failed login attempt for username: {loginDto.Username}");
                return Unauthorized("Invalid username or password.");
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            // Return the token to the user
            return Ok(new { Token = token });
        }

        // Helper method to verify password (you should replace this with a proper hash comparison)
        private bool VerifyPassword(string password, string storedHash)
        {
            // In a real scenario, you should use something like BCrypt or PBKDF2 to verify the hashed password.
            return password == storedHash; // Simple comparison for the sake of example
        }
    }
}
