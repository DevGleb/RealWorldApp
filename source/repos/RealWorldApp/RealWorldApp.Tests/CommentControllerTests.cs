using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.Controllers;
using RealWorldApp.Data;
using RealWorldApp.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RealWorldApp.Tests
{
    public class CommentControllerTests
    {
        private static AppDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        private static ClaimsPrincipal CreateUserPrincipal(int userId)
        {
            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private static CommentController CreateController(AppDbContext context, int? userId = null)
        {
            var controller = new CommentController(context);
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
        public async Task AddComment_Succeeds()
        {
            var context = CreateInMemoryContext(nameof(AddComment_Succeeds));
            var user = new User { Id = 1, Username = "gleb" };
            var article = new Article { Id = 1, Slug = "slug", Title = "T", Body = "B", Description = "D", AuthorId = 1 };

            context.Users.Add(user);
            context.Articles.Add(article);
            context.SaveChanges();

            var controller = CreateController(context, 1);
            var comment = new Comment { Body = "Great post!" };

            var result = await controller.AddComment("slug", comment) as CreatedResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
        }

        [Fact]
        public async Task AddComment_ReturnsBadRequest_WhenBodyMissing()
        {
            var context = CreateInMemoryContext(nameof(AddComment_ReturnsBadRequest_WhenBodyMissing));
            context.Users.Add(new User { Id = 1, Username = "gleb" });
            context.Articles.Add(new Article { Id = 1, Slug = "slug", AuthorId = 1 });
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.AddComment("slug", new Comment { Body = "" }) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task AddComment_ReturnsNotFound_WhenArticleMissing()
        {
            var context = CreateInMemoryContext(nameof(AddComment_ReturnsNotFound_WhenArticleMissing));
            context.Users.Add(new User { Id = 1, Username = "gleb" });
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.AddComment("missing", new Comment { Body = "Comment" }) as NotFoundResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetComments_ReturnsList()
        {
            var context = CreateInMemoryContext(nameof(GetComments_ReturnsList));
            var user = new User { Id = 1, Username = "gleb" };
            var article = new Article { Id = 1, Slug = "slug", AuthorId = 1 };
            context.Users.Add(user);
            context.Articles.Add(article);
            context.Comments.Add(new Comment { Body = "One", ArticleId = 1, AuthorId = 1 });
            context.Comments.Add(new Comment { Body = "Two", ArticleId = 1, AuthorId = 1 });
            context.SaveChanges();

            var controller = CreateController(context);

            var result = await controller.GetComments("slug") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetComments_ReturnsNotFound_WhenArticleMissing()
        {
            var context = CreateInMemoryContext(nameof(GetComments_ReturnsNotFound_WhenArticleMissing));
            var controller = CreateController(context);

            var result = await controller.GetComments("missing") as NotFoundResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteComment_Succeeds_WhenAuthor()
        {
            var context = CreateInMemoryContext(nameof(DeleteComment_Succeeds_WhenAuthor));
            var user = new User { Id = 1, Username = "gleb" };
            var article = new Article { Id = 1, Slug = "slug", AuthorId = 1 };
            var comment = new Comment { Id = 1, ArticleId = 1, AuthorId = 1, Body = "B" };

            context.Users.Add(user);
            context.Articles.Add(article);
            context.Comments.Add(comment);
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.DeleteComment("slug", 1) as NoContentResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteComment_ReturnsNotFound_WhenCommentMissing()
        {
            var context = CreateInMemoryContext(nameof(DeleteComment_ReturnsNotFound_WhenCommentMissing));
            var article = new Article { Id = 1, Slug = "slug", AuthorId = 1 };
            context.Articles.Add(article);
            context.Users.Add(new User { Id = 1 });
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.DeleteComment("slug", 999) as NotFoundResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteComment_ReturnsForbid_WhenNotAuthor()
        {
            var context = CreateInMemoryContext(nameof(DeleteComment_ReturnsForbid_WhenNotAuthor));
            var article = new Article { Id = 1, Slug = "slug", AuthorId = 1 };
            var comment = new Comment { Id = 1, ArticleId = 1, AuthorId = 2, Body = "Nope" };

            context.Users.Add(new User { Id = 1 });
            context.Users.Add(new User { Id = 2 });
            context.Articles.Add(article);
            context.Comments.Add(comment);
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.DeleteComment("slug", 1) as ForbidResult;
            Assert.NotNull(result);
        }
    }
}
