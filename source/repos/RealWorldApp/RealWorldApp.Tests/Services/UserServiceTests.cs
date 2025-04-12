using System.Threading.Tasks;
using Moq;
using RealWorldApp.Application.Services;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.DTOs;
using RealWorldApp.Models;
using RealWorldApp.Services;
using Xunit;

namespace RealWorldApp.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _jwtServiceMock = new Mock<IJwtService>();

            _userService = new UserService(
                _userRepoMock.Object,
                _jwtServiceMock.Object
            );
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

            var result = await _userService.GetCurrentUserAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal("gleb", result!.User.Username);
            Assert.Equal("gleb@example.com", result.User.Email);
            Assert.Equal("bio", result.User.Bio);
            Assert.Equal("img", result.User.Image);
            Assert.Equal("jwt-token", result.User.Token);
        }

        [Fact]
        public async Task UpdateCurrentUserAsync_UpdatesFields_WhenValid()
        {
            var user = new User
            {
                Id = 1,
                Username = "olduser",
                Email = "old@example.com",
                Bio = "oldbio",
                Image = "oldimg",
                PasswordHash = "hash"
            };

            var request = new UpdateUserRequest
            {
                User = new UpdateUserDto
                {
                    Username = "newuser",
                    Email = "new@example.com",
                    Password = "newpassword",
                    Bio = "newbio",
                    Image = "newimg"
                }
            };

            _userRepoMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
            _jwtServiceMock.Setup(j => j.GenerateToken(user.Id, request.User.Email)).Returns("jwt-token");

            var result = await _userService.UpdateCurrentUserAsync(user.Id, request);

            Assert.NotNull(result);
            Assert.Equal("newuser", result!.User.Username);
            Assert.Equal("new@example.com", result.User.Email);
            Assert.Equal("newbio", result.User.Bio);
            Assert.Equal("newimg", result.User.Image);
            Assert.Equal("jwt-token", result.User.Token);
        }
    }
}
