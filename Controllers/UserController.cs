using Microsoft.AspNetCore.Mvc;
using RealWorldApp.Models;
using RealWorldApp.Data;

namespace RealWorldApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest("Email is already in use.");
            }

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok("User registered successfully");
        }
    }
}
