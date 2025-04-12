using System.Threading.Tasks;
using Moq;
using RealWorldApp.Application.Services;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.Models;
using Xunit;

namespace RealWorldApp.Tests.Services
{
    public class ProfileServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly ProfileService _profileService;

        public ProfileServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _profileService = new ProfileService(_userRepoMock.Object);
        }

        [Fact]
        public async Task GetProfileAsync_ReturnsProfile_WhenUserExists()
        {
            var user = new User
            {
                Id = 1,
                Username = "gleb",
                Bio = "bio",
                Image = "img"
            };

            _userRepoMock.Setup(r => r.GetByUsernameAsync("gleb"))
                         .ReturnsAsync(user);
            _userRepoMock.Setup(r => r.IsFollowingAsync(2, 1))
                         .ReturnsAsync(true);

            var result = await _profileService.GetProfileAsync("gleb", 2);

            Assert.NotNull(result);
            Assert.Equal("gleb", result!.Profile.Username);
            Assert.Equal("bio", result.Profile.Bio);
            Assert.Equal("img", result.Profile.Image);
            Assert.True(result.Profile.Following);
        }

        [Fact]
        public async Task FollowUserAsync_ReturnsProfile_WhenSuccess()
        {
            var user = new User
            {
                Id = 1,
                Username = "gleb",
                Bio = "bio",
                Image = "img"
            };

            _userRepoMock.Setup(r => r.GetByUsernameAsync("gleb"))
                         .ReturnsAsync(user);

            _userRepoMock.Setup(r => r.FollowAsync(2, 1))
                         .Returns(Task.CompletedTask);

            var result = await _profileService.FollowUserAsync("gleb", 2);

            Assert.NotNull(result);
            Assert.Equal("gleb", result!.Profile.Username);
            Assert.Equal("bio", result.Profile.Bio);
            Assert.Equal("img", result.Profile.Image);
            Assert.True(result.Profile.Following);
        }

        [Fact]
        public async Task UnfollowUserAsync_ReturnsProfile_WhenSuccess()
        {
            var user = new User
            {
                Id = 1,
                Username = "gleb",
                Bio = "bio",
                Image = "img"
            };

            _userRepoMock.Setup(r => r.GetByUsernameAsync("gleb"))
                         .ReturnsAsync(user);

            _userRepoMock.Setup(r => r.UnfollowAsync(2, 1))
                         .Returns(Task.CompletedTask);

            var result = await _profileService.UnfollowUserAsync("gleb", 2);

            Assert.NotNull(result);
            Assert.Equal("gleb", result!.Profile.Username);
            Assert.Equal("bio", result.Profile.Bio);
            Assert.Equal("img", result.Profile.Image);
            Assert.False(result.Profile.Following);
        }
    }
}
