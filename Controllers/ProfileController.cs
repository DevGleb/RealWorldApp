using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealWorldApp.Application.Interfaces;
using System.Security.Claims;

namespace RealWorldApp.Controllers
{
    [Route("api/profiles")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            int? currentUserId = User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
                : null;

            var result = await _profileService.GetProfileAsync(username, currentUserId);
            return result is null ? NotFound() : Ok(new { profile = result });
        }

        [HttpPost("{username}/follow")]
        [Authorize]
        public async Task<IActionResult> FollowUser(string username)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _profileService.FollowUserAsync(username, currentUserId);
            return result is null ? NotFound() : Ok(new { profile = result });
        }

        [HttpDelete("{username}/follow")]
        [Authorize]
        public async Task<IActionResult> UnfollowUser(string username)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _profileService.UnfollowUserAsync(username, currentUserId);
            return result is null ? NotFound() : Ok(new { profile = result });
        }
    }
}
