using System.Threading.Tasks;
using Moq;
using RealWorldApp.Application.Services;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.DTOs.Responses;
using RealWorldApp.Models;
using RealWorldApp.Services;
using Xunit;

namespace RealWorldApp.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _jwtServiceMock = new Mock<IJwtService>();

            _authService = new AuthService(
                _userRepoMock.Object,
                _jwtServiceMock.Object
            );
        }

        [Fact]
        public async Task RegisterAsync_CreatesUser_WhenEmailNotExists()
        {
            var request = new RegisterRequest
            {
                User = new RegisterUserDto
                {
                    Username = "gleb",
                    Email = "gleb@example.com",
                    Password = "password123"
                }
            };

            _userRepoMock.Setup(r => r.GetByEmailAsync(request.User.Email))
                         .ReturnsAsync((User?)null);

            _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>())).Returns("jwt-token");

            var result = await _authService.RegisterAsync(request);

            Assert.Equal("gleb", result.User.Username);
            Assert.Equal("gleb@example.com", result.User.Email);
            Assert.Equal("", result.User.Bio);
            Assert.Equal("", result.User.Image);
            Assert.Equal("jwt-token", result.User.Token);
        }

        [Fact]
        public async Task LoginAsync_ReturnsUser_WhenCredentialsAreCorrect()
        {
            var request = new LoginRequest
            {
                User = new LoginUserDto
                {
                    Email = "gleb@example.com",
                    Password = "password123"
                }
            };

            var user = new User
            {
                Id = 1,
                Username = "gleb",
                Email = request.User.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.User.Password),
                Bio = "bio",
                Image = "img"
            };

            _userRepoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _jwtServiceMock.Setup(j => j.GenerateToken(user.Id, user.Email)).Returns("jwt-token");

            var result = await _authService.LoginAsync(request);

            Assert.NotNull(result);
            Assert.Equal("gleb", result!.User.Username);
            Assert.Equal("gleb@example.com", result.User.Email);
            Assert.Equal("bio", result.User.Bio);
            Assert.Equal("img", result.User.Image);
            Assert.Equal("jwt-token", result.User.Token);

        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_WhenCredentialsIncorrect()
        {
            var request = new LoginRequest
            {
                User = new LoginUserDto
                {
                    Email = "gleb@example.com",
                    Password = "wrongpassword"
                }
            };

            var user = new User
            {
                Id = 1,
                Email = request.User.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
            };

            _userRepoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var result = await _authService.LoginAsync(request);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCurrentUserAsync_ReturnsUser_WhenExists()
        {
            var user = new User
            {
                Id = 1,
                Username = "gleb",
                Email = "gleb@example.com",
                Bio = "bio",
                Image = "img"
            };

            _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _jwtServiceMock.Setup(j => j.GenerateToken(user.Id, user.Email)).Returns("jwt-token");

            var result = await _authService.GetCurrentUserAsync(user.Id) as CurrentUserResponse;

            Assert.NotNull(result);
            Assert.Equal("gleb", result!.User.Username);
            Assert.Equal("gleb@example.com", result.User.Email);
            Assert.Equal("bio", result.User.Bio);
            Assert.Equal("img", result.User.Image);
            Assert.Equal("jwt-token", result.User.Token);
        }

        [Fact]
        public async Task GetCurrentUserAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);

            var result = await _authService.GetCurrentUserAsync(1);

            Assert.Null(result);
        }
    }
}