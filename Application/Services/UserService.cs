using RealWorldApp.Application.Interfaces;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.Models;
using RealWorldApp.Services;
using BCrypt.Net;
using RealWorldApp.DTOs.Responses;

namespace RealWorldApp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public UserService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<CurrentUserResponse?> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            return new CurrentUserResponse
            {
                User = new UserDto
                {
                    Username = user.Username,
                    Email = user.Email,
                    Bio = user.Bio,
                    Image = user.Image,
                    Token = _jwtService.GenerateToken(user.Id, user.Email)
                }
            };
        }

        public async Task<CurrentUserResponse?> UpdateCurrentUserAsync(int userId, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(request.User.Email))
                user.Email = request.User.Email;

            if (!string.IsNullOrWhiteSpace(request.User.Username))
                user.Username = request.User.Username;

            if (!string.IsNullOrWhiteSpace(request.User.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.User.Password);

            if (!string.IsNullOrWhiteSpace(request.User.Bio))
                user.Bio = request.User.Bio;

            if (!string.IsNullOrWhiteSpace(request.User.Image))
                user.Image = request.User.Image;

            await _userRepository.UpdateAsync(user);

            return new CurrentUserResponse
            {
                User = new UserDto
                {
                    Username = user.Username,
                    Email = user.Email,
                    Bio = user.Bio,
                    Image = user.Image,
                    Token = _jwtService.GenerateToken(user.Id, user.Email)
                }
            };
        }

    }
}
