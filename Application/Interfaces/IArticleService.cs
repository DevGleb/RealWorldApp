using RealWorldApp.DTOs;
using RealWorldApp.DTOs.Responses;
using RealWorldApp.Models;

namespace RealWorldApp.Application.Interfaces
{
    public interface IArticleService
    {
        Task<ArticleListResponse> GetAllArticlesAsync(int limit, int offset, int? userId);
        Task<ArticleResponse?> GetArticleBySlugAsync(string slug, int? userId);
        Task<ArticleResponse> CreateArticleAsync(ArticleData data, int userId);
        Task<ArticleResponse?> UpdateArticleAsync(string slug, ArticleData updated, int userId);

        Task<bool> DeleteArticleAsync(string slug, int userId);
        Task<ArticleListResponse> GetFeedArticlesAsync(int limit, int offset, int userId);
        Task<ArticleResponse?> FavoriteArticleAsync(string slug, int userId);
        Task<ArticleResponse?> UnfavoriteArticleAsync(string slug, int userId);
    }
}
