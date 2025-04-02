using Xunit;
using Moq;
using RealWorldApp.Controllers;
using RealWorldApp.Data;
using RealWorldApp.Models;
using RealWorldApp.Services;
using RealWorldApp.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace RealWorldApp.Tests
{
    public class AuthControllerTests
    {
        private static AppDbContext CreateInMemoryContext(string name)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public void Register_Succeeds()
        {
            var context = CreateInMemoryContext(nameof(Register_Succeeds));
            var jwtMock = new Mock<IJwtService>();
            jwtMock.Setup(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>()))
                   .Returns("jwt-token");

            var controller = new AuthController(context, jwtMock.Object);
            var request = new RegisterRequest
            {
                User = new RegisterUserDto
                {
                    Email = "test@example.com",
                    Username = "testuser",
                    Password = "123456"
                }
            };

            var result = controller.Register(request) as CreatedResult;
            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
        }

        [Fact]
        public void Register_Fails_WhenEmailExists()
        {
            var context = CreateInMemoryContext(nameof(Register_Fails_WhenEmailExists));
            context.Users.Add(new User { Email = "existing@example.com", Username = "user1", PasswordHash = "pwd" });
            context.SaveChanges();

            var jwtMock = new Mock<IJwtService>();
            var controller = new AuthController(context, jwtMock.Object);

            var request = new RegisterRequest
            {
                User = new RegisterUserDto
                {
                    Email = "existing@example.com",
                    Username = "newuser",
                    Password = "password"
                }
            };

            var result = controller.Register(request) as BadRequestObjectResult;
            Assert.NotNull(result);

            var json = JsonSerializer.Serialize(result!.Value);
            var doc = JsonDocument.Parse(json);

            var emailError = doc.RootElement
                .GetProperty("errors")
                .GetProperty("email")[0]
                .GetString();

            Assert.Equal("is already taken", emailError);
        }


        private class ErrorResponseWrapper
        {
            public Dictionary<string, string[]> Errors { get; set; } = new();
        }

        [Fact]
        public void Register_Fails_WhenUsernameExists()
        {
            var context = CreateInMemoryContext(nameof(Register_Fails_WhenUsernameExists));
            context.Users.Add(new User { Email = "email@domain.com", Username = "takenuser", PasswordHash = "pwd" });
            context.SaveChanges();

            var jwtMock = new Mock<IJwtService>();
            var controller = new AuthController(context, jwtMock.Object);

            var request = new RegisterRequest
            {
                User = new RegisterUserDto
                {
                    Email = "new@email.com",
                    Username = "takenuser",
                    Password = "password"
                }
            };

            var result = controller.Register(request) as BadRequestObjectResult;
            Assert.NotNull(result);
            var json = JsonSerializer.Serialize(result!.Value);
            Assert.Contains("is already taken", json);

        }

        [Fact]
        public void Login_Succeeds()
        {
            var context = CreateInMemoryContext(nameof(Login_Succeeds));
            var password = "123456";
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            context.Users.Add(user);
            context.SaveChanges();

            var jwtMock = new Mock<IJwtService>();
            jwtMock.Setup(j => j.GenerateToken(user.Id, user.Email)).Returns("jwt-token");

            var controller = new AuthController(context, jwtMock.Object);

            var request = new LoginRequest
            {
                User = new LoginUserDto
                {
                    Email = user.Email,
                    Password = password
                }
            };

            var result = controller.Login(request) as OkObjectResult;
            Assert.NotNull(result);
            Assert.Equal(200, result!.StatusCode);
        }

        [Fact]
        public void Login_Fails_WhenWrongPassword()
        {
            var context = CreateInMemoryContext(nameof(Login_Fails_WhenWrongPassword));
            var user = new User
            {
                Email = "test@example.com",
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
            };
            context.Users.Add(user);
            context.SaveChanges();

            var jwtMock = new Mock<IJwtService>();
            var controller = new AuthController(context, jwtMock.Object);

            var request = new LoginRequest
            {
                User = new LoginUserDto
                {
                    Email = user.Email,
                    Password = "wrongpassword"
                }
            };

            var result = controller.Login(request) as UnauthorizedObjectResult;
            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }

        [Fact]
        public void Login_Fails_WhenEmailNotFound()
        {
            var context = CreateInMemoryContext(nameof(Login_Fails_WhenEmailNotFound));
            var jwtMock = new Mock<IJwtService>();
            var controller = new AuthController(context, jwtMock.Object);

            var request = new LoginRequest
            {
                User = new LoginUserDto
                {
                    Email = "unknown@example.com",
                    Password = "any"
                }
            };

            var result = controller.Login(request) as UnauthorizedObjectResult;
            Assert.NotNull(result);
            Assert.Equal(401, result!.StatusCode);
        }
    }
}
