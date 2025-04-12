using RealWorldApp.Models;

namespace RealWorldApp.Domain.Interfaces
{
    public interface IArticleRepository
    {
        Task<int> CountAsync();
        Task<List<Article>> GetAllAsync(int limit, int offset);
        Task<Article?> GetBySlugAsync(string slug);
        Task<List<string>> GetTagsAsync(int articleId);
        Task AddOrAttachTagAsync(Article article, string tagName);
        Task<int> CountByAuthorsAsync(List<int> authorIds);
        Task<List<Article>> GetByAuthorsAsync(List<int> authorIds, int limit, int offset);

        Task AddAsync(Article article);
        Task UpdateAsync(Article article);
        Task DeleteAsync(Article article);
    }
}
