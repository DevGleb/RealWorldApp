using RealWorldApp.Models;

namespace RealWorldApp.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<List<int>> GetFollowingIdsAsync(int userId);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> IsFollowingAsync(int followerId, int followingId);
        Task FollowAsync(int followerId, int followingId);
        Task UnfollowAsync(int followerId, int followingId);

    }
}
