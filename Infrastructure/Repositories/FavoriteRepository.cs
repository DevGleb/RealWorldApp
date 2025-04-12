using Microsoft.EntityFrameworkCore;
using RealWorldApp.Data;
using RealWorldApp.Domain.Interfaces;

namespace RealWorldApp.Infrastructure.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AppDbContext _context;

        public FavoriteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountByArticleIdAsync(int articleId)
        {
            return await _context.Favorites.CountAsync(f => f.ArticleId == articleId);
        }

        public async Task<bool> IsFavoritedAsync(int articleId, int userId)
        {
            return await _context.Favorites.AnyAsync(f => f.ArticleId == articleId && f.UserId == userId);
        }
        public async Task AddAsync(int articleId, int userId)
        {
            _context.Favorites.Add(new Models.Favorite
            {
                ArticleId = articleId,
                UserId = userId
            });
            await _context.SaveChangesAsync();
        }
        public async Task RemoveAsync(int articleId, int userId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.ArticleId == articleId && f.UserId == userId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
        }

    }
}
