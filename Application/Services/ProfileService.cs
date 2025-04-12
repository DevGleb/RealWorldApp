using RealWorldApp.Application.Interfaces;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.DTOs.Responses;
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

        public async Task<ProfileResponse?> GetProfileAsync(string username, int? currentUserId)
        {
            var targetUser = await _userRepository.GetByUsernameAsync(username);
            if (targetUser == null) return null;

            var isFollowing = currentUserId.HasValue &&
                              await _userRepository.IsFollowingAsync(currentUserId.Value, targetUser.Id);

            return new ProfileResponse
            {
                Profile = new AuthorResponse
                {
                    Username = targetUser.Username,
                    Bio = targetUser.Bio,
                    Image = targetUser.Image,
                    Following = isFollowing
                }
            };
        }

        public async Task<ProfileResponse?> FollowUserAsync(string username, int currentUserId)
        {
            var targetUser = await _userRepository.GetByUsernameAsync(username);
            if (targetUser == null || targetUser.Id == currentUserId) return null;

            await _userRepository.FollowAsync(currentUserId, targetUser.Id);

            return new ProfileResponse
            {
                Profile = new AuthorResponse
                {
                    Username = targetUser.Username,
                    Bio = targetUser.Bio,
                    Image = targetUser.Image,
                    Following = true
                }
            };
        }

        public async Task<ProfileResponse?> UnfollowUserAsync(string username, int currentUserId)
        {
            var targetUser = await _userRepository.GetByUsernameAsync(username);
            if (targetUser == null || targetUser.Id == currentUserId) return null;

            await _userRepository.UnfollowAsync(currentUserId, targetUser.Id);

            return new ProfileResponse
            {
                Profile = new AuthorResponse
                {
                    Username = targetUser.Username,
                    Bio = targetUser.Bio,
                    Image = targetUser.Image,
                    Following = false
                }
            };
        }
    }
}
