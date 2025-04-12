using RealWorldApp.DTOs.Responses;

namespace RealWorldApp.Application.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileResponse?> GetProfileAsync(string username, int? currentUserId);
        Task<ProfileResponse?> FollowUserAsync(string username, int currentUserId);
        Task<ProfileResponse?> UnfollowUserAsync(string username, int currentUserId);
    }
}
