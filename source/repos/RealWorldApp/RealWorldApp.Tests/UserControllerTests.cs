using Xunit;
using Moq;
using RealWorldApp.Controllers;
using RealWorldApp.Data;
using RealWorldApp.Models;
using RealWorldApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RealWorldApp.Tests
{
    public class UserControllerTests
    {
        private static ClaimsPrincipal CreateUserPrincipal(int userId)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private static UserController CreateController(AppDbContext context, IJwtService jwtService, int? userId = 1)
        {
            var controller = new UserController(context, jwtService);
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

        private static AppDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private T DeserializeResult<T>(object? resultValue)
        {
            var json = JsonSerializer.Serialize(resultValue);
            return JsonSerializer.Deserialize<T>(json)!;
        }

        private class UserWrapper
        {
            [JsonPropertyName("user")]
            public UserDto User { get; set; } = default!;
        }

        private class UserDto
        {
            [JsonPropertyName("email")]
            public string Email { get; set; } = default!;

            [JsonPropertyName("username")]
            public string Username { get; set; } = default!;

            [JsonPropertyName("token")]
            public string Token { get; set; } = default!;

            [JsonPropertyName("bio")]
            public string Bio { get; set; } = default!;

            [JsonPropertyName("image")]
            public string Image { get; set; } = default!;
        }

        [Fact]
        public void UpdateCurrentUser_UpdatesSuccessfully_WhenValid()
        {
            var context = CreateInMemoryContext(nameof(UpdateCurrentUser_UpdatesSuccessfully_WhenValid));
            var user = new User { Id = 1, Email = "old@example.com", Username = "olduser", PasswordHash = "", Bio = "", Image = "" };
            context.Users.Add(user);
            context.SaveChanges();

            var jwtMock = new Mock<IJwtService>();
            jwtMock.Setup(j => j.GenerateToken(1, "new@example.com")).Returns("new-token");

            var controller = CreateController(context, jwtMock.Object);

            var request = new UpdateUserRequest
            {
                User = new UpdateUserData
                {
                    Email = "new@example.com",
                    Username = "newuser",
                    Password = "newpass",
                    Bio = "new bio",
                    Image = "new image"
                }
            };

            var result = controller.UpdateCurrentUser(request) as OkObjectResult;
            Assert.NotNull(result);

            var parsed = DeserializeResult<UserWrapper>(result!.Value);
            Assert.Equal("new@example.com", parsed.User.Email);
            Assert.Equal("newuser", parsed.User.Username);
            Assert.Equal("new bio", parsed.User.Bio);
            Assert.Equal("new image", parsed.User.Image);
            Assert.Equal("new-token", parsed.User.Token);
        }

        [Fact]
        public void UpdateCurrentUser_ReturnsBadRequest_WhenEmailTaken()
        {
            var context = CreateInMemoryContext(nameof(UpdateCurrentUser_ReturnsBadRequest_WhenEmailTaken));
            context.Users.Add(new User { Id = 1, Email = "user@example.com", Username = "user1", PasswordHash = "" });
            context.Users.Add(new User { Id = 2, Email = "taken@example.com", Username = "other", PasswordHash = "" });
            context.SaveChanges();

            var jwtMock = new Mock<IJwtService>();
            var controller = CreateController(context, jwtMock.Object);

            var request = new UpdateUserRequest
            {
                User = new UpdateUserData { Email = "taken@example.com" }
            };

            var result = controller.UpdateCurrentUser(request) as BadRequestObjectResult;
            Assert.NotNull(result);
            var response = JsonSerializer.Serialize(result!.Value);
            Assert.Contains("is already taken", response);
        }

        [Fact]
        public void UpdateCurrentUser_ReturnsBadRequest_WhenUsernameTaken()
        {
            var context = CreateInMemoryContext(nameof(UpdateCurrentUser_ReturnsBadRequest_WhenUsernameTaken));
            context.Users.Add(new User { Id = 1, Email = "user@example.com", Username = "oldname" });
            context.Users.Add(new User { Id = 2, Email = "other@example.com", Username = "takenuser" });
            context.SaveChanges();

            var jwtMock = new Mock<IJwtService>();
            var controller = CreateController(context, jwtMock.Object);

            var request = new UpdateUserRequest
            {
                User = new UpdateUserData { Username = "takenuser" }
            };

            var result = controller.UpdateCurrentUser(request) as BadRequestObjectResult;
            Assert.NotNull(result);
            var response = JsonSerializer.Serialize(result!.Value);
            Assert.Contains("is already taken", response);
        }

        [Fact]
        public void UpdateCurrentUser_UpdatesPartialFields()
        {
            var context = CreateInMemoryContext(nameof(UpdateCurrentUser_UpdatesPartialFields));
            context.Users.Add(new User { Id = 1, Email = "a@a.com", Username = "u", Bio = "", Image = "" });
            context.SaveChanges();

            var jwtMock = new Mock<IJwtService>();
            jwtMock.Setup(j => j.GenerateToken(1, "a@a.com")).Returns("token");

            var controller = CreateController(context, jwtMock.Object);

            var request = new UpdateUserRequest
            {
                User = new UpdateUserData { Bio = "updated bio" }
            };

            var result = controller.UpdateCurrentUser(request) as OkObjectResult;
            Assert.NotNull(result);

            var parsed = DeserializeResult<UserWrapper>(result!.Value);
            Assert.Equal("updated bio", parsed.User.Bio);
            Assert.Equal("token", parsed.User.Token);
        }

        [Fact]
        public void UpdateCurrentUser_ReturnsUnauthorized_WhenUserNotAuthenticated()
        {
            var context = CreateInMemoryContext(nameof(UpdateCurrentUser_ReturnsUnauthorized_WhenUserNotAuthenticated));
            var jwtMock = new Mock<IJwtService>();

            var controller = new UserController(context, jwtMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext() 
                }
            };

            var request = new UpdateUserRequest
            {
                User = new UpdateUserData { Username = "x" }
            };

            var result = controller.UpdateCurrentUser(request) as UnauthorizedResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void UpdateCurrentUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            var context = CreateInMemoryContext(nameof(UpdateCurrentUser_ReturnsNotFound_WhenUserDoesNotExist));
            var jwtMock = new Mock<IJwtService>();
            var controller = CreateController(context, jwtMock.Object);

            var request = new UpdateUserRequest
            {
                User = new UpdateUserData { Username = "ghost" }
            };

            var result = controller.UpdateCurrentUser(request) as NotFoundResult;
            Assert.NotNull(result);
        }
    }
}
