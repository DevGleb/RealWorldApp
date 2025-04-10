using RealWorldApp.Models;

namespace RealWorldApp.Domain.Interfaces
{
    public interface ICommentRepository
    {
        Task AddAsync(Comment comment);
        Task<List<Comment>> GetByArticleIdAsync(int articleId);
        Task<Comment?> GetByIdAsync(int commentId);
        Task DeleteAsync(Comment comment);
    }
}
