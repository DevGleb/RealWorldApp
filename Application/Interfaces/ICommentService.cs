using RealWorldApp.DTOs.Responses;
using RealWorldApp.Models;

namespace RealWorldApp.Application.Interfaces
{
    public interface ICommentService
    {
        Task<CommentResponse?> AddCommentAsync(string slug, Comment comment, int userId);
        Task<IEnumerable<CommentResponse>> GetCommentsAsync(string slug);
        Task<bool> DeleteCommentAsync(string slug, int commentId, int userId);
    }

}
