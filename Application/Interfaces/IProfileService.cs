namespace RealWorldApp.Application.Interfaces
{
    public interface IProfileService
    {
        Task<object?> GetProfileAsync(string username, int? currentUserId);
        Task<object?> FollowUserAsync(string username, int currentUserId);
        Task<object?> UnfollowUserAsync(string username, int currentUserId);
    }
}
