using RealWorldApp.DTOs.Responses;
using RealWorldApp.Models;

namespace RealWorldApp.Application.Interfaces
{
    public interface IUserService
    {
        Task<CurrentUserResponse?> GetCurrentUserAsync(int userId);
        Task<CurrentUserResponse?> UpdateCurrentUserAsync(int userId, UpdateUserRequest request);
    }
}
