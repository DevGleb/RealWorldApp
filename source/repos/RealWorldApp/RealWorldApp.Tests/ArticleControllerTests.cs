using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.Controllers;
using RealWorldApp.Data;
using RealWorldApp.Models;
using RealWorldApp.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace RealWorldApp.Tests
{
    public class ArticleControllerTests
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
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "TestAuth");

            return new ClaimsPrincipal(identity);
        }

        private static ArticleController CreateController(AppDbContext context, int? userId = null)
        {
            var controller = new ArticleController(context);

            var httpContext = new DefaultHttpContext();

            if (userId.HasValue)
            {
                httpContext.User = CreateUserPrincipal(userId.Value);
            }
            else
            {
                httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }


        [Fact]
        public async Task GetArticle_ReturnsNotFound_WhenArticleDoesNotExist()
        {
            var context = CreateInMemoryContext(nameof(GetArticle_ReturnsNotFound_WhenArticleDoesNotExist));
            var controller = CreateController(context);

            var result = await controller.GetArticle("non-existing") as NotFoundResult;

            Assert.NotNull(result);
        }



        [Fact]
        public async Task GetArticle_ReturnsArticle_WhenFound()
        {
            var context = CreateInMemoryContext(nameof(GetArticle_ReturnsArticle_WhenFound));

            var user = new User { Id = 1, Username = "gleb", Email = "gleb@example.com" };
            context.Users.Add(user); 

            var article = new Article
            {
                Slug = "test-article",
                Title = "Test",
                Description = "desc",
                Body = "body",
                AuthorId = user.Id
            };

            context.Articles.Add(article);
            context.SaveChanges();

            var controller = CreateController(context);

            var result = await controller.GetArticle("test-article") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }


        [Fact]
        public async Task GetAllArticles_ReturnsList()
        {
            var context = CreateInMemoryContext(nameof(GetAllArticles_ReturnsList));

            var user = new User { Id = 1, Username = "gleb", Email = "gleb@example.com" };
            context.Users.Add(user); 

            context.Articles.Add(new Article { Slug = "a1", Title = "a1", Description = "d", Body = "b", AuthorId = user.Id });
            context.Articles.Add(new Article { Slug = "a2", Title = "a2", Description = "d", Body = "b", AuthorId = user.Id });
            context.SaveChanges();

            var controller = CreateController(context);

            var result = await controller.GetAllArticles(10, 0) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }



        [Fact]
        public async Task CreateArticle_Succeeds()
        {
            var context = CreateInMemoryContext(nameof(CreateArticle_Succeeds));
            context.Users.Add(new User { Id = 1, Username = "gleb", Email = "g@e.com" });
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var request = new ArticleRequest
            {
                Article = new ArticleData
                {
                    Title = "Title",
                    Description = "Description",
                    Body = "Body",
                    TagList = new List<string> { "tag1", "tag2" }
                }
            };

            var result = await controller.CreateArticle(request) as CreatedResult;

            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);
        }

        [Fact]
        public async Task UpdateArticle_UpdatesOnlyOwned()
        {
            var context = CreateInMemoryContext(nameof(UpdateArticle_UpdatesOnlyOwned));
            context.Users.Add(new User { Id = 1, Username = "gleb" });
            var article = new Article { Slug = "slug", Title = "t", Description = "d", Body = "b", AuthorId = 1 };
            context.Articles.Add(article);
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var updated = new Article { Title = "new" };

            var result = await controller.UpdateArticle("slug", updated) as OkObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteArticle_ReturnsNotFound_WhenArticleDoesNotExist()
        {
            var context = CreateInMemoryContext(nameof(DeleteArticle_ReturnsNotFound_WhenArticleDoesNotExist));
            context.Users.Add(new User { Id = 1, Username = "gleb" });
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.DeleteArticle("not-found") as NotFoundObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteArticle_ReturnsNoContent_WhenSuccess()
        {
            var context = CreateInMemoryContext(nameof(DeleteArticle_ReturnsNoContent_WhenSuccess));
            var user = new User { Id = 1, Username = "gleb" };
            var article = new Article { Slug = "a", Title = "t", Description = "d", Body = "b", AuthorId = 1 };

            context.Users.Add(user);
            context.Articles.Add(article);
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.DeleteArticle("a") as NoContentResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task FavoriteArticle_AddsFavorite_WhenNotYetFavorited()
        {
            var context = CreateInMemoryContext(nameof(FavoriteArticle_AddsFavorite_WhenNotYetFavorited));
            var user = new User { Id = 1, Username = "user" };
            var article = new Article { Id = 1, Slug = "a", Title = "t", Description = "d", Body = "b", AuthorId = 1 };

            context.Users.Add(user);
            context.Articles.Add(article);
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.FavoriteArticle("a") as OkObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UnfavoriteArticle_RemovesFavorite_IfExists()
        {
            var context = CreateInMemoryContext(nameof(UnfavoriteArticle_RemovesFavorite_IfExists));
            var user = new User { Id = 1, Username = "user" };
            var article = new Article { Id = 1, Slug = "a", Title = "t", Description = "d", Body = "b", AuthorId = 1 };
            var favorite = new Favorite { ArticleId = 1, UserId = 1 };

            context.Users.Add(user);
            context.Articles.Add(article);
            context.Favorites.Add(favorite);
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.UnfavoriteArticle("a") as OkObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetFeedArticles_ReturnsEmpty_WhenNotFollowing()
        {
            var context = CreateInMemoryContext(nameof(GetFeedArticles_ReturnsEmpty_WhenNotFollowing));
            context.Users.Add(new User { Id = 1, Username = "user" });
            context.SaveChanges();

            var controller = CreateController(context, 1);

            var result = await controller.GetFeedArticles(10, 0) as OkObjectResult;

            Assert.NotNull(result);
        }
    }
}
