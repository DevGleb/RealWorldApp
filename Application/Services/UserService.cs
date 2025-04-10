using RealWorldApp.Application.Interfaces;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.Models;
using RealWorldApp.Services;
using BCrypt.Net;

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

        public async Task<object?> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            return new
            {
                user = new
                {
                    username = user.Username,
                    email = user.Email,
                    bio = user.Bio,
                    image = user.Image,
                    token = _jwtService.GenerateToken(user.Id, user.Email)
                }
            };
        }

        public async Task<object?> UpdateCurrentUserAsync(int userId, UpdateUserRequest request)
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

            return new
            {
                user = new
                {
                    username = user.Username,
                    email = user.Email,
                    bio = user.Bio,
                    image = user.Image,
                    token = _jwtService.GenerateToken(user.Id, user.Email)
                }
            };
        }
    }
}
