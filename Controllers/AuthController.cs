using Microsoft.AspNetCore.Mvc;
using RealWorldApp.Data;
using RealWorldApp.Models;
using RealWorldApp.Services;
using RealWorldApp.DTOs;
using Serilog;

namespace RealWorldApp.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthController(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("api/users")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            var data = request.User;

            if (_context.Users.Any(u => u.Email == data.Email))
            {
                return BadRequest(new
                {
                    errors = new { email = new[] { "is already taken" } }
                });
            }

            if (_context.Users.Any(u => u.Username == data.Username))
            {
                return BadRequest(new
                {
                    errors = new { username = new[] { "is already taken" } }
                });
            }

            var user = new User
            {
                Email = data.Email,
                Username = data.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(data.Password),
                Bio = "",
                Image = ""
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var token = _jwtService.GenerateToken(user.Id, user.Email);

            var response = new
            {
                user = new
                {
                    email = user.Email,
                    username = user.Username,
                    token,
                    bio = user.Bio,
                    image = user.Image
                }
            };

            return Created("/api/user", response);
        }

        [HttpPost("api/users/login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var data = request.User;

            var dbUser = _context.Users.FirstOrDefault(u => u.Email == data.Email);
            if (dbUser == null || !BCrypt.Net.BCrypt.Verify(data.Password, dbUser.PasswordHash))
            {
                return Unauthorized(new
                {
                    errors = new { credentials = new[] { "Invalid email or password" } }
                });
            }

            var token = _jwtService.GenerateToken(dbUser.Id, dbUser.Email);

            return Ok(new
            {
                user = new
                {
                    email = dbUser.Email,
                    username = dbUser.Username,
                    token,
                    bio = dbUser.Bio ?? "",
                    image = dbUser.Image ?? ""
                }
            });
        }
    }
}
