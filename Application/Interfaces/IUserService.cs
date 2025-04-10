using RealWorldApp.DTOs;
using RealWorldApp.Models;

namespace RealWorldApp.Application.Interfaces
{
    public interface IUserService
    {
        Task<object?> GetCurrentUserAsync(int userId);
        Task<object?> UpdateCurrentUserAsync(int userId, UpdateUserRequest request);
    }
}
