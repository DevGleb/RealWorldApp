using RealWorldApp.Models;

namespace RealWorldApp.Application.Interfaces
{
    public interface ICommentService
    {
        Task<object?> AddCommentAsync(string slug, Comment comment, int userId);
        Task<IEnumerable<object>> GetCommentsAsync(string slug);
        Task<bool> DeleteCommentAsync(string slug, int commentId, int userId);
    }
}
