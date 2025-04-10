using RealWorldApp.DTOs;
using RealWorldApp.Models;

namespace RealWorldApp.Application.Interfaces
{
    public interface IAuthService
    {
        Task<object> RegisterAsync(RegisterRequest request);
        Task<object?> LoginAsync(LoginRequest request);
        Task<object?> GetCurrentUserAsync(int userId);
    }
}
