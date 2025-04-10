using RealWorldApp.Application.Interfaces;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.Models;

namespace RealWorldApp.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;

        public ProfileService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<object?> GetProfileAsync(string username, int? currentUserId)
        {
            var targetUser = await _userRepository.GetByUsernameAsync(username);
            if (targetUser == null) return null;

            var isFollowing = currentUserId.HasValue &&
                              await _userRepository.IsFollowingAsync(currentUserId.Value, targetUser.Id);

            return new
            {
                profile = new
                {
                    username = targetUser.Username,
                    bio = targetUser.Bio,
                    image = targetUser.Image,
                    following = isFollowing
                }
            };
        }

        public async Task<object?> FollowUserAsync(string username, int currentUserId)
        {
            var targetUser = await _userRepository.GetByUsernameAsync(username);
            if (targetUser == null || targetUser.Id == currentUserId) return null;

            await _userRepository.FollowAsync(currentUserId, targetUser.Id);

            return new
            {
                profile = new
                {
                    username = targetUser.Username,
                    bio = targetUser.Bio,
                    image = targetUser.Image,
                    following = true
                }
            };
        }

        public async Task<object?> UnfollowUserAsync(string username, int currentUserId)
        {
            var targetUser = await _userRepository.GetByUsernameAsync(username);
            if (targetUser == null || targetUser.Id == currentUserId) return null;

            await _userRepository.UnfollowAsync(currentUserId, targetUser.Id);

            return new
            {
                profile = new
                {
                    username = targetUser.Username,
                    bio = targetUser.Bio,
                    image = targetUser.Image,
                    following = false
                }
            };
        }
    }
}
