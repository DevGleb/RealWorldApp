using Microsoft.AspNetCore.Mvc;
using RealWorldApp.Data;
using RealWorldApp.Models;
using RealWorldApp.Services;
using BCrypt.Net;

namespace RealWorldApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest(new { message = "Email already in use" });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var dbUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (dbUser == null || !BCrypt.Net.BCrypt.Verify(request.PasswordHash, dbUser.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var token = _jwtService.GenerateToken(dbUser.Id, dbUser.Email);
            return Ok(new { token });
        }

    }
}
