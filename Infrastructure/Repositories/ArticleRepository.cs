using Microsoft.EntityFrameworkCore;
using RealWorldApp.Data;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.Models;

namespace RealWorldApp.Infrastructure.Repositories
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly AppDbContext _context;

        public ArticleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Articles.CountAsync();
        }

        public async Task<List<Article>> GetAllAsync(int limit, int offset)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .OrderByDescending(a => a.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<Article?> GetBySlugAsync(string slug)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .FirstOrDefaultAsync(a => a.Slug == slug);
        }

        public async Task<List<string>> GetTagsAsync(int articleId)
        {
            return await _context.ArticleTags
                .Where(at => at.ArticleId == articleId)
                .Include(at => at.Tag)
                .Select(at => at.Tag!.Name)
                .ToListAsync();
        }

        public async Task AddAsync(Article article)
        {
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Article article)
        {
            _context.Articles.Update(article);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Article article)
        {
            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrAttachTagAsync(Article article, string tagName)
        {
            var existing = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            var tagEntity = existing ?? new Tag { Name = tagName };

            if (existing == null)
                _context.Tags.Add(tagEntity);

            article.ArticleTags.Add(new ArticleTag
            {
                Article = article,
                Tag = tagEntity
            });

            await _context.SaveChangesAsync();
        }

        public async Task<int> CountByAuthorsAsync(List<int> authorIds)
        {
            return await _context.Articles
                .Where(a => authorIds.Contains(a.AuthorId))
                .CountAsync();
        }

        public async Task<List<Article>> GetByAuthorsAsync(List<int> authorIds, int limit, int offset)
        {
            return await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .Where(a => authorIds.Contains(a.AuthorId))
                .OrderByDescending(a => a.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }


    }
}
