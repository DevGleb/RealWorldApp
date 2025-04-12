using RealWorldApp.Application.Interfaces;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.DTOs.Responses;
using RealWorldApp.Models;

namespace RealWorldApp.Application.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IArticleRepository _articleRepository;
        private readonly IUserRepository _userRepository;

        public CommentService(
            ICommentRepository commentRepository,
            IArticleRepository articleRepository,
            IUserRepository userRepository)
        {
            _commentRepository = commentRepository;
            _articleRepository = articleRepository;
            _userRepository = userRepository;
        }

        public async Task<CommentResponse?> AddCommentAsync(string slug, Comment comment, int userId)
        {
            var article = await _articleRepository.GetBySlugAsync(slug);
            if (article == null) return null;

            comment.ArticleId = article.Id;
            comment.AuthorId = userId;
            comment.CreatedAt = DateTime.UtcNow;
            comment.UpdatedAt = DateTime.UtcNow;

            await _commentRepository.AddAsync(comment);

            var author = await _userRepository.GetByIdAsync(userId);

            return new CommentResponse
            {
                Id = comment.Id,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                Body = comment.Body,
                Author = new AuthorResponse
                {
                    Username = author?.Username ?? "",
                    Bio = author?.Bio ?? "",
                    Image = author?.Image ?? "",
                    Following = false
                }
            };

        }

        public async Task<IEnumerable<CommentResponse>> GetCommentsAsync(string slug)
        {
            var article = await _articleRepository.GetBySlugAsync(slug);
            if (article == null) return Enumerable.Empty<CommentResponse>();


            var comments = await _commentRepository.GetByArticleIdAsync(article.Id);

            return comments.Select(comment => new CommentResponse
            {
                Id = comment.Id,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                Body = comment.Body,
                Author = new AuthorResponse
                {
                    Username = comment.Author?.Username ?? "",
                    Bio = comment.Author?.Bio ?? "",
                    Image = comment.Author?.Image ?? "",
                    Following = false
                }
            });

        }

        public async Task<bool> DeleteCommentAsync(string slug, int commentId, int userId)
        {
            var article = await _articleRepository.GetBySlugAsync(slug);
            if (article == null) return false;

            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.ArticleId != article.Id || comment.AuthorId != userId)
                return false;

            await _commentRepository.DeleteAsync(comment);
            return true;
        }
    }
}
