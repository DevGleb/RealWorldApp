using Microsoft.EntityFrameworkCore;
using RealWorldApp.Data;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.Models;

namespace RealWorldApp.Infrastructure.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Comment>> GetByArticleIdAsync(int articleId)
        {
            return await _context.Comments
                .Where(c => c.ArticleId == articleId)
                .Include(c => c.Author)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment?> GetByIdAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == commentId);
        }

        public async Task DeleteAsync(Comment comment)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }
}
