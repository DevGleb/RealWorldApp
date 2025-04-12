using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealWorldApp.Application.Interfaces;
using RealWorldApp.Models;
using System.Security.Claims;

namespace RealWorldApp.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return result is null
                ? Unauthorized(new { errors = new { login = new[] { "Invalid email or password." } } })
                : Ok(new { user = result });
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(new { user = result });
            }
            catch (Exception ex)
            {
                return Conflict(new { errors = new { register = new[] { ex.Message } } });
            }
        }
    }
}
