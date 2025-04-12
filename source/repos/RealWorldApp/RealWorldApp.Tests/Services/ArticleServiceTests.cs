using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using RealWorldApp.Application.Services;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.DTOs;
using RealWorldApp.DTOs.Responses;
using RealWorldApp.Models;
using Xunit;

namespace RealWorldApp.Tests.Services
{
    public class ArticleServiceTests
    {
        private readonly Mock<IArticleRepository> _articleRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IFavoriteRepository> _favoriteRepoMock;
        private readonly ArticleService _articleService;

        public ArticleServiceTests()
        {
            _articleRepoMock = new Mock<IArticleRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _favoriteRepoMock = new Mock<IFavoriteRepository>();

            _articleService = new ArticleService(
                _articleRepoMock.Object,
                _userRepoMock.Object,
                _favoriteRepoMock.Object
            );
        }

        [Fact]
        public async Task GetAllArticlesAsync_ReturnsArticles_WhenTheyExist()
        {
            var articles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Title = "Test Article",
                    Slug = "test-article",
                    Body = "This is a test.",
                    AuthorId = 10,
                    Author = new User
                    {
                        Id = 10,
                        Username = "gleb",
                        Bio = "",
                        Image = ""
                    }
                }
            };

            _articleRepoMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(articles);

            _articleRepoMock
                .Setup(repo => repo.CountAsync())
                .ReturnsAsync(1);

            _articleRepoMock
                .Setup(repo => repo.GetTagsAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "tag1", "tag2" });

            _userRepoMock
                .Setup(repo => repo.GetFollowingIdsAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<int>());

            _userRepoMock
                .Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new User
                {
                    Id = 10,
                    Username = "gleb",
                    Bio = "",
                    Image = ""
                });

            _favoriteRepoMock
                .Setup(repo => repo.IsFavoritedAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            _favoriteRepoMock
                .Setup(repo => repo.CountByArticleIdAsync(It.IsAny<int>()))
                .ReturnsAsync(1);

            var result = await _articleService.GetAllArticlesAsync(10, 0, 5);

            var articleList = result.Articles;

            Assert.NotNull(articleList);
            Assert.Single(articleList);

            var article = articleList[0];
            Assert.Equal("test-article", article.Slug);
            Assert.Equal("gleb", article.Author.Username);

        }

        [Fact]
        public async Task GetArticleBySlugAsync_ReturnsArticle_WhenFound()
        {
            var article = new Article
            {
                Id = 1,
                Slug = "test-article",
                Title = "Test Article",
                Description = "desc",
                Body = "body",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AuthorId = 10
            };

            _articleRepoMock
                .Setup(repo => repo.GetBySlugAsync(article.Slug))
                .ReturnsAsync(article);

            _articleRepoMock
                .Setup(repo => repo.GetTagsAsync(article.Id))
                .ReturnsAsync(new List<string> { "tag1", "tag2" });

            _userRepoMock
                .Setup(repo => repo.GetByIdAsync(article.AuthorId))
                .ReturnsAsync(new User
                {
                    Id = 10,
                    Username = "gleb",
                    Bio = "bio",
                    Image = "img"
                });

            _favoriteRepoMock
                .Setup(repo => repo.CountByArticleIdAsync(article.Id))
                .ReturnsAsync(2);

            _favoriteRepoMock
                .Setup(repo => repo.IsFavoritedAsync(article.Id, 5))
                .ReturnsAsync(true);

            var result = await _articleService.GetArticleBySlugAsync("test-article", 5);

            Assert.NotNull(result);
            var articleResult = result as ArticleResponse;
            Assert.NotNull(articleResult);
            Assert.Equal("test-article", articleResult!.Slug);
            Assert.Equal("gleb", articleResult.Author.Username);
            Assert.True(articleResult.Favorited);
        }
        [Fact]
        public async Task CreateArticleAsync_CreatesAndReturnsArticle()
        {
            var userId = 5;
            var data = new ArticleData
            {
                Title = "New Article",
                Description = "Short description",
                Body = "Full body",
                TagList = new List<string> { "tag1", "tag2" }
            };

            var createdArticle = new Article
            {
                Id = 1,
                Title = data.Title,
                Description = data.Description,
                Body = data.Body,
                Slug = "new-article-slug",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AuthorId = userId
            };

            _articleRepoMock
                .Setup(repo => repo.AddAsync(It.IsAny<Article>()))
                .Callback<Article>(a =>
                {
                    a.Id = createdArticle.Id;
                    a.Slug = createdArticle.Slug;
                    a.CreatedAt = createdArticle.CreatedAt;
                    a.UpdatedAt = createdArticle.UpdatedAt;
                })
                .Returns(Task.CompletedTask);

            _articleRepoMock
                .Setup(repo => repo.AddOrAttachTagAsync(It.IsAny<Article>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _articleRepoMock
                .Setup(repo => repo.GetTagsAsync(It.IsAny<int>()))
                .ReturnsAsync(data.TagList);

            _userRepoMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Username = "gleb",
                    Bio = "bio",
                    Image = "img"
                });

            _favoriteRepoMock
                .Setup(repo => repo.CountByArticleIdAsync(It.IsAny<int>()))
                .ReturnsAsync(0);

            _favoriteRepoMock
                .Setup(repo => repo.IsFavoritedAsync(It.IsAny<int>(), userId))
                .ReturnsAsync(false);

            var result = await _articleService.CreateArticleAsync(data, userId);

            var response = result as ArticleResponse;
            Assert.NotNull(response);
            Assert.Equal("New Article", response!.Title);
            Assert.Equal("gleb", response.Author.Username);
            Assert.Equal(2, response.TagList.Count);
        }
        [Fact]
        public async Task UpdateArticleAsync_UpdatesAndReturnsArticle_WhenAuthorized()
        {
            var slug = "test-article";
            var userId = 10;

            var existingArticle = new Article
            {
                Id = 1,
                Slug = slug,
                Title = "Old Title",
                Description = "Old Description",
                Body = "Old Body",
                AuthorId = userId,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedData = new ArticleData
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Body = "Updated Body"
            };

            _articleRepoMock
                .Setup(repo => repo.GetBySlugAsync(slug))
                .ReturnsAsync(existingArticle);

            _articleRepoMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<Article>()))
                .Returns(Task.CompletedTask);

            _articleRepoMock
                .Setup(repo => repo.GetTagsAsync(existingArticle.Id))
                .ReturnsAsync(new List<string>());

            _userRepoMock
                .Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Username = "gleb",
                    Bio = "bio",
                    Image = "img"
                });

            _favoriteRepoMock
                .Setup(repo => repo.CountByArticleIdAsync(existingArticle.Id))
                .ReturnsAsync(0);

            _favoriteRepoMock
                .Setup(repo => repo.IsFavoritedAsync(existingArticle.Id, userId))
                .ReturnsAsync(false);

            var result = await _articleService.UpdateArticleAsync(slug, updatedData, userId);


            var response = result as ArticleResponse;
            Assert.NotNull(response);
            Assert.Equal("Updated Title", response!.Title);
            Assert.Equal("Updated Description", response.Description);
            Assert.Equal("Updated Body", response.Body);
            Assert.Equal("gleb", response.Author.Username);
        }
        [Fact]
        public async Task DeleteArticleAsync_DeletesArticle_WhenUserIsAuthor()
        {
            var slug = "test-article";
            var userId = 10;

            var article = new Article
            {
                Id = 1,
                Slug = slug,
                AuthorId = userId
            };

            _articleRepoMock
                .Setup(repo => repo.GetBySlugAsync(slug))
                .ReturnsAsync(article);

            _articleRepoMock
                .Setup(repo => repo.DeleteAsync(article))
                .Returns(Task.CompletedTask);

            var result = await _articleService.DeleteArticleAsync(slug, userId);

            Assert.True(result);
        }
        [Fact]
        public async Task DeleteArticleAsync_ReturnsFalse_WhenArticleNotFound()
        {
 
            _articleRepoMock
                .Setup(repo => repo.GetBySlugAsync(It.IsAny<string>()))
                .ReturnsAsync((Article?)null);


            var result = await _articleService.DeleteArticleAsync("not-found", 1);

            Assert.False(result);
        }
        [Fact]
        public async Task GetFeedArticlesAsync_ReturnsArticles_FromFollowedAuthors()
        {
            var userId = 5;
            var followedAuthorId = 10;

            var articles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Slug = "feed-article",
                    Title = "Feed Article",
                    Description = "desc",
                    Body = "body",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    AuthorId = followedAuthorId
                }
            };

            _userRepoMock
                .Setup(repo => repo.GetFollowingIdsAsync(userId))
                .ReturnsAsync(new List<int> { followedAuthorId });

            _articleRepoMock
                .Setup(repo => repo.CountByAuthorsAsync(It.IsAny<List<int>>()))
                .ReturnsAsync(1);

            _articleRepoMock
                .Setup(repo => repo.GetByAuthorsAsync(It.IsAny<List<int>>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(articles);

            _articleRepoMock
                .Setup(repo => repo.GetTagsAsync(1))
                .ReturnsAsync(new List<string>());

            _userRepoMock
                .Setup(repo => repo.GetByIdAsync(followedAuthorId))
                .ReturnsAsync(new User
                {
                    Id = followedAuthorId,
                    Username = "gleb",
                    Bio = "bio",
                    Image = "img"
                });

            _favoriteRepoMock
                .Setup(repo => repo.CountByArticleIdAsync(1))
                .ReturnsAsync(0);

            _favoriteRepoMock
                .Setup(repo => repo.IsFavoritedAsync(1, userId))
                .ReturnsAsync(false);

            var result = await _articleService.GetFeedArticlesAsync(10, 0, userId);


            var response = result as ArticleListResponse;
            Assert.NotNull(response);
            Assert.Single(response!.Articles);

            var article = response.Articles.First();
            Assert.Equal("Feed Article", article.Title);
            Assert.Equal("gleb", article.Author.Username);
        }
        [Fact]
        public async Task FavoriteArticleAsync_AddsFavorite_IfNotAlreadyFavorited()
        {
            var userId = 5;
            var slug = "test-article";
            var isFavorited = false;

            var article = new Article
            {
                Id = 1,
                Slug = slug,
                Title = "Test Article",
                Description = "desc",
                Body = "body",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AuthorId = 10
            };

            _articleRepoMock
                .Setup(repo => repo.GetBySlugAsync(slug))
                .ReturnsAsync(article);

            _favoriteRepoMock
                .Setup(repo => repo.IsFavoritedAsync(article.Id, userId))
                .ReturnsAsync(() => isFavorited);

            _favoriteRepoMock
                .Setup(repo => repo.AddAsync(article.Id, userId))
                .Callback(() => isFavorited = true)
                .Returns(Task.CompletedTask);

            _favoriteRepoMock
                .Setup(repo => repo.CountByArticleIdAsync(article.Id))
                .ReturnsAsync(1);

            _articleRepoMock
                .Setup(repo => repo.GetTagsAsync(article.Id))
                .ReturnsAsync(new List<string>());

            _userRepoMock
                .Setup(repo => repo.GetByIdAsync(article.AuthorId))
                .ReturnsAsync(new User
                {
                    Id = 10,
                    Username = "gleb",
                    Bio = "bio",
                    Image = "img"
                });

            var result = await _articleService.FavoriteArticleAsync(slug, userId);

            var response = result as ArticleResponse;
            Assert.NotNull(response);
            Assert.True(response!.Favorited);
            Assert.Equal("Test Article", response.Title);
            Assert.Equal("gleb", response.Author.Username);
        }




        [Fact]
        public async Task UnfavoriteArticleAsync_RemovesFavorite_WhenAlreadyFavorited()
        {
            var userId = 5;
            var slug = "test-article";
            var isFavorited = true;

            var article = new Article
            {
                Id = 1,
                Slug = slug,
                Title = "Test Article",
                Description = "desc",
                Body = "body",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AuthorId = 10
            };

            _articleRepoMock
                .Setup(repo => repo.GetBySlugAsync(slug))
                .ReturnsAsync(article);

            _favoriteRepoMock
                .Setup(repo => repo.IsFavoritedAsync(article.Id, userId))
                .ReturnsAsync(() => isFavorited);

            _favoriteRepoMock
                .Setup(repo => repo.RemoveAsync(article.Id, userId))
                .Callback(() => isFavorited = false)
                .Returns(Task.CompletedTask);

            _favoriteRepoMock
                .Setup(repo => repo.CountByArticleIdAsync(article.Id))
                .ReturnsAsync(0);

            _articleRepoMock
                .Setup(repo => repo.GetTagsAsync(article.Id))
                .ReturnsAsync(new List<string>());

            _userRepoMock
                .Setup(repo => repo.GetByIdAsync(article.AuthorId))
                .ReturnsAsync(new User
                {
                    Id = 10,
                    Username = "gleb",
                    Bio = "bio",
                    Image = "img"
                });

            var result = await _articleService.UnfavoriteArticleAsync(slug, userId);

            var response = result as ArticleResponse;
            Assert.NotNull(response);
            Assert.False(response!.Favorited);
            Assert.Equal("gleb", response.Author.Username);
        }
    }
}
