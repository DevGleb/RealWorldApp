using RealWorldApp.Application.Interfaces;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.DTOs.Responses;
using RealWorldApp.Models;
using RealWorldApp.Services;
using BCrypt.Net;

namespace RealWorldApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<CurrentUserResponse> RegisterAsync(RegisterRequest request)
        {
            var existing = await _userRepository.GetByEmailAsync(request.User.Email);
            if (existing != null)
            {
                throw new Exception("User with this email already exists.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.User.Password);

            var user = new User
            {
                Username = request.User.Username,
                Email = request.User.Email,
                PasswordHash = hashedPassword,
                Bio = "",
                Image = ""
            };

            await _userRepository.AddAsync(user);

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

        public async Task<CurrentUserResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.User.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.User.Password, user.PasswordHash))
            {
                return null;
            }

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
    }
}