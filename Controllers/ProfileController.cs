using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.Data;
using System.Security.Claims;

namespace RealWorldApp.Controllers
{
    [ApiController]
    [Route("api/profiles")]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetProfile(string username)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound(new
                {
                    errors = new
                    {
                        profile = new[] { "User not found" }
                    }
                });
            }

            bool following = false;

            var currentUserId = User.Identity?.IsAuthenticated == true
                ? int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value)
                : (int?)null;

            if (currentUserId != null)
            {
                following = await _context.Follows
                    .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == user.Id);
            }

            return Ok(new
            {
                profile = new
                {
                    username = user.Username,
                    bio = user.Bio ?? "",
                    image = user.Image ?? "",
                    following
                }
            });
        }

        [Authorize]
        [HttpPost("{username}/follow")]
        public async Task<IActionResult> FollowUser(string username)
        {
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (targetUser == null)
            {
                return NotFound(new
                {
                    errors = new
                    {
                        profile = new[] { "User not found" }
                    }
                });
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (targetUser.Id == currentUserId)
            {
                return BadRequest(new
                {
                    errors = new
                    {
                        follow = new[] { "You cannot follow yourself." }
                    }
                });
            }

            var alreadyFollowing = await _context.Follows.AnyAsync(f =>
                f.FollowerId == currentUserId && f.FollowingId == targetUser.Id);

            if (!alreadyFollowing)
            {
                _context.Follows.Add(new Models.Follow
                {
                    FollowerId = currentUserId,
                    FollowingId = targetUser.Id
                });
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                profile = new
                {
                    username = targetUser.Username,
                    bio = targetUser.Bio ?? "",
                    image = targetUser.Image ?? "",
                    following = true
                }
            });
        }

        [Authorize]
        [HttpDelete("{username}/follow")]
        public async Task<IActionResult> UnfollowUser(string username)
        {
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (targetUser == null)
            {
                return NotFound(new
                {
                    errors = new
                    {
                        profile = new[] { "User not found" }
                    }
                });
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var follow = await _context.Follows.FirstOrDefaultAsync(f =>
                f.FollowerId == currentUserId && f.FollowingId == targetUser.Id);

            if (follow != null)
            {
                _context.Follows.Remove(follow);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                profile = new
                {
                    username = targetUser.Username,
                    bio = targetUser.Bio ?? "",
                    image = targetUser.Image ?? "",
                    following = false
                }
            });
        }
    }
}
