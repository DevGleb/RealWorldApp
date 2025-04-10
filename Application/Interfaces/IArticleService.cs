using RealWorldApp.DTOs;
using RealWorldApp.Models;

namespace RealWorldApp.Application.Interfaces
{
    public interface IArticleService
    {
        Task<object> GetAllArticlesAsync(int limit, int offset, int? userId);
        Task<object?> GetArticleBySlugAsync(string slug, int? userId);
        Task<object> CreateArticleAsync(ArticleData data, int userId);
        Task<object?> UpdateArticleAsync(string slug, Article updated, int userId);
        Task<bool> DeleteArticleAsync(string slug, int userId);
        Task<object> GetFeedArticlesAsync(int limit, int offset, int userId);
        Task<object?> FavoriteArticleAsync(string slug, int userId);
        Task<object?> UnfavoriteArticleAsync(string slug, int userId);
    }
}
