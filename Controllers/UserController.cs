using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealWorldApp.Models;
using RealWorldApp.Data;
using RealWorldApp.Services;
using System.Security.Claims;
using RealWorldApp.DTOs;

namespace RealWorldApp.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public UserController(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound();

            var token = _jwtService.GenerateToken(user.Id, user.Email);

            return Ok(new
            {
                user = new
                {
                    email = user.Email,
                    username = user.Username,
                    token,
                    bio = user.Bio ?? "",
                    image = user.Image ?? ""
                }
            });
        }

        [HttpPut]
        [Authorize]
        public IActionResult UpdateCurrentUser([FromBody] UpdateUserRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return NotFound();

            var data = request.User;

            if (!string.IsNullOrEmpty(data.Email) && data.Email != user.Email)
            {
                if (_context.Users.Any(u => u.Email == data.Email))
                {
                    return BadRequest(new { errors = new { email = new[] { "is already taken" } } });
                }
                user.Email = data.Email;
            }

            if (!string.IsNullOrEmpty(data.Username) && data.Username != user.Username)
            {
                if (_context.Users.Any(u => u.Username == data.Username))
                {
                    return BadRequest(new { errors = new { username = new[] { "is already taken" } } });
                }
                user.Username = data.Username;
            }

            if (!string.IsNullOrEmpty(data.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(data.Password);
            }

            if (data.Bio != null) user.Bio = data.Bio;
            if (data.Image != null) user.Image = data.Image;

            _context.SaveChanges();

            var token = _jwtService.GenerateToken(user.Id, user.Email);

            return Ok(new
            {
                user = new
                {
                    email = user.Email,
                    username = user.Username,
                    token,
                    bio = user.Bio ?? "",
                    image = user.Image ?? ""
                }
            });
        }
    }
}
