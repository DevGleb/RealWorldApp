using RealWorldApp.DTOs;
using RealWorldApp.DTOs.Responses;
using RealWorldApp.Models;

namespace RealWorldApp.Application.Interfaces
{
    public interface IAuthService
    {
        Task<CurrentUserResponse> RegisterAsync(RegisterRequest request);        
        Task<CurrentUserResponse?> LoginAsync(LoginRequest request);            
        Task<CurrentUserResponse?> GetCurrentUserAsync(int userId);           
    }

}
