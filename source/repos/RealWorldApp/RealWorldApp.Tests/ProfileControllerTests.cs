using Xunit;
using Moq;
using RealWorldApp.Controllers;
using RealWorldApp.Data;
using RealWorldApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealWorldApp.Tests
{
    public class ProfileControllerTests
    {
        private static AppDbContext CreateInMemoryContext(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(options);
        }

        private static ClaimsPrincipal CreateUserPrincipal(int userId)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "TestAuth");

            return new ClaimsPrincipal(identity);
        }

        private static ProfileController CreateController(AppDbContext context, int? userId = null)
        {
            var controller = new ProfileController(context);

            if (userId.HasValue)
            {
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = CreateUserPrincipal(userId.Value)
                    }
                };
            }

            return controller;
        }

        [Fact]
        public async Task GetProfile_ReturnsProfile_WhenExists()
        {
            var context = CreateInMemoryContext(nameof(GetProfile_ReturnsProfile_WhenExists));
            var user = new User { Id = 1, Username = "gleb", Email = "g@e.com" };
            context.Users.Add(user);
            context.SaveChanges();

            var controller = CreateController(context, 2); 

            var result = await controller.GetProfile("gleb") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetProfile_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var context = CreateInMemoryContext(nameof(GetProfile_ReturnsNotFound_WhenUserDoesNotExist));
            var controller = CreateController(context, 1);

            var result = await controller.GetProfile("nonexistent") as NotFoundObjectResult;

            Assert.NotNull(result);                     
            Assert.Equal(404, result.StatusCode);       
        }



        [Fact]
        public async Task FollowUser_Succeeds_WhenValid()
        {
            var context = CreateInMemoryContext(nameof(FollowUser_Succeeds_WhenValid));
            context.Users.Add(new User { Id = 1, Username = "gleb" }); 
            context.Users.Add(new User { Id = 2, Username = "target" });
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.FollowUser("target") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task FollowUser_ReturnsBadRequest_WhenFollowingSelf()
        {
            var context = CreateInMemoryContext(nameof(FollowUser_ReturnsBadRequest_WhenFollowingSelf));
            context.Users.Add(new User { Id = 1, Username = "gleb" });
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.FollowUser("gleb") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UnfollowUser_Succeeds_WhenFollowing()
        {
            var context = CreateInMemoryContext(nameof(UnfollowUser_Succeeds_WhenFollowing));
            context.Users.Add(new User { Id = 1, Username = "gleb" });
            context.Users.Add(new User { Id = 2, Username = "target" });
            context.Follows.Add(new Follow { FollowerId = 1, FollowingId = 2 });
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.UnfollowUser("target") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task UnfollowUser_Succeeds_WhenNotFollowing()
        {
            var context = CreateInMemoryContext(nameof(UnfollowUser_Succeeds_WhenNotFollowing));
            context.Users.Add(new User { Id = 1, Username = "gleb" });
            context.Users.Add(new User { Id = 2, Username = "target" });
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.UnfollowUser("target") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }
    }
}
