using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RealWorldApp.Application.Services;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.DTOs.Responses;
using RealWorldApp.Models;
using Xunit;

namespace RealWorldApp.Tests.Services
{
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _commentRepoMock;
        private readonly Mock<IArticleRepository> _articleRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly CommentService _commentService;

        public CommentServiceTests()
        {
            _commentRepoMock = new Mock<ICommentRepository>();
            _articleRepoMock = new Mock<IArticleRepository>();
            _userRepoMock = new Mock<IUserRepository>();

            _commentService = new CommentService(
                _commentRepoMock.Object,
                _articleRepoMock.Object,
                _userRepoMock.Object
            );
        }

        [Fact]
        public async Task AddCommentAsync_ReturnsComment_WhenArticleExists()
        {
            var slug = "test-article";
            var userId = 5;
            var article = new Article { Id = 1, Slug = slug };
            var comment = new Comment { Body = "Nice post!" };
            var user = new User { Id = userId, Username = "gleb", Bio = "bio", Image = "img" };

            _articleRepoMock.Setup(r => r.GetBySlugAsync(slug)).ReturnsAsync(article);
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _commentRepoMock.Setup(r => r.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);

            var result = await _commentService.AddCommentAsync(slug, comment, userId);

            Assert.NotNull(result);
            Assert.Equal("Nice post!", result!.Body);
            Assert.Equal("gleb", result.Author.Username);
        }


        [Fact]
        public async Task GetCommentsAsync_ReturnsComments_WhenArticleExists()
        {
            var slug = "test-article";
            var article = new Article { Id = 1, Slug = slug };
            var comments = new List<Comment>
            {
                new Comment
                {
                    Id = 1,
                    Body = "Nice post!",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Author = new User { Username = "john", Bio = "bio", Image = "img" }
                }
            };

            _articleRepoMock.Setup(r => r.GetBySlugAsync(slug)).ReturnsAsync(article);
            _commentRepoMock.Setup(r => r.GetByArticleIdAsync(article.Id)).ReturnsAsync(comments);

            var result = await _commentService.GetCommentsAsync(slug);

            Assert.Single(result);
            var comment = result.First();
            Assert.Equal("Nice post!", comment.Body);
            Assert.Equal("john", comment.Author.Username);
        }

        [Fact]
        public async Task DeleteCommentAsync_ReturnsTrue_WhenValid()
        {
            var slug = "test-article";
            var commentId = 1;
            var userId = 5;
            var article = new Article { Id = 1, Slug = slug };
            var comment = new Comment { Id = commentId, ArticleId = article.Id, AuthorId = userId };

            _articleRepoMock.Setup(r => r.GetBySlugAsync(slug)).ReturnsAsync(article);
            _commentRepoMock.Setup(r => r.GetByIdAsync(commentId)).ReturnsAsync(comment);
            _commentRepoMock.Setup(r => r.DeleteAsync(comment)).Returns(Task.CompletedTask);

            var result = await _commentService.DeleteCommentAsync(slug, commentId, userId);

            Assert.True(result);
        }
    }
}
