using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.Data;
using RealWorldApp.Models;
using RealWorldApp.Validators;
using RealWorldApp.DTOs;
using System.Security.Claims;
using FluentValidation;

namespace RealWorldApp.Controllers
{
    [Route("api/articles")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ArticleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllArticles([FromQuery] int limit = 20, [FromQuery]  int offset = 0)
        {
            var userId = GetUserIdOrNull();

            var totalCount = await _context.Articles.CountAsync();

            var articles = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .OrderByDescending(a => a.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var result = await Task.WhenAll(articles.Select(a => BuildArticleData(a, userId)));
            return Ok(new { articles = result, articlesCount = totalCount });
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetArticle(string slug)
        {
            var article = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .FirstOrDefaultAsync(a => a.Slug == slug);

            if (article == null) return NotFound();

            var userId = GetUserIdOrNull();
            return await BuildArticleResponse(article, userId);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateArticle([FromBody] ArticleRequest request)
        {
            var data = request.Article;

            var dtoValidator = new InlineValidator<ArticleData>();
            dtoValidator.RuleFor(a => a.Title).NotEmpty().MinimumLength(3);
            dtoValidator.RuleFor(a => a.Description).NotEmpty();
            dtoValidator.RuleFor(a => a.Body).NotEmpty();
            await dtoValidator.ValidateAndThrowAsync(data);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var article = new Article
            {
                Title = data.Title,
                Description = data.Description,
                Body = data.Body,
                AuthorId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Slug = GenerateSlug(data.Title)
            };

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            if (data.TagList != null)
            {
                foreach (var tag in data.TagList.Distinct())
                {
                    var existing = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tag);
                    var tagEntity = existing ?? new Tag { Name = tag };
                    if (existing == null) _context.Tags.Add(tagEntity);
                    article.ArticleTags.Add(new ArticleTag { Article = article, Tag = tagEntity });
                }
                await _context.SaveChangesAsync();
            }

            var articleData = await BuildArticleData(article, userId);
            return Created($"/api/articles/{article.Slug}", new { article = articleData });
        }


        [HttpPut("{slug}")]
        [Authorize]
        public async Task<IActionResult> UpdateArticle(string slug, [FromBody] Article updated)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var article = await _context.Articles
                .Include(a => a.ArticleTags)
                .FirstOrDefaultAsync(a => a.Slug == slug);

            if (article == null) return NotFound();
            if (article.AuthorId != userId) return Forbid();

            article.Title = updated.Title ?? article.Title;
            article.Description = updated.Description ?? article.Description;
            article.Body = updated.Body ?? article.Body;
            article.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await BuildArticleResponse(article, userId);
        }

        [HttpDelete("{slug}")]
        [Authorize]
        public async Task<IActionResult> DeleteArticle(string slug)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Slug == slug);

            if (article == null)
            {
                return NotFound(new
                {
                    errors = new { article = new[] { "Article not found." } }
                });
            }

            if (article.AuthorId != userId)
                return Forbid();

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent(); 
        }


        [HttpGet("feed")]
        [Authorize]
        public async Task<IActionResult> GetFeedArticles([FromQuery] int limit = 20, [FromQuery] int offset = 0)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            var totalCount = await _context.Articles
                .Where(a => followingIds.Contains(a.AuthorId))
                .CountAsync();

            var articles = await _context.Articles
                .Include(a => a.Author)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .Where(a => followingIds.Contains(a.AuthorId))
                .OrderByDescending(a => a.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var result = await Task.WhenAll(articles.Select(a => BuildArticleData(a, userId, true)));
            return Ok(new { articles = result, articlesCount = totalCount });
        }

        [HttpPost("{slug}/favorite")]
        [Authorize]
        public async Task<IActionResult> FavoriteArticle(string slug)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Slug == slug);
            if (article == null) return NotFound();

            var alreadyFavorited = await _context.Favorites
                .AnyAsync(f => f.ArticleId == article.Id && f.UserId == userId);

            if (!alreadyFavorited)
            {
                _context.Favorites.Add(new Favorite { ArticleId = article.Id, UserId = userId });
                await _context.SaveChangesAsync();
            }

            return await BuildArticleResponse(article, userId);
        }

        [HttpDelete("{slug}/favorite")]
        [Authorize]
        public async Task<IActionResult> UnfavoriteArticle(string slug)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Slug == slug);

            if (article == null)
            {
                return NotFound(new
                {
                    errors = new { article = new[] { "Article not found." } }
                });
            }

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.ArticleId == article.Id && f.UserId == userId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return await BuildArticleResponse(article, userId);
        }


        private async Task<IActionResult> BuildArticleResponse(Article article, int? userId)
        {
            var articleData = await BuildArticleData(article, userId);
            return Ok(new { article = articleData });
        }

        private async Task<object> BuildArticleData(Article article, int? userId, bool following = false)
        {
            var author = await _context.Users.FindAsync(article.AuthorId);
            var favoritesCount = await _context.Favorites.CountAsync(f => f.ArticleId == article.Id);
            var isFavorited = userId.HasValue &&
                await _context.Favorites.AnyAsync(f => f.ArticleId == article.Id && f.UserId == userId.Value);

            var tagList = await _context.ArticleTags
                .Where(at => at.ArticleId == article.Id)
                .Include(at => at.Tag)
                .Select(at => at.Tag!.Name)
                .ToListAsync();

            return new
            {
                slug = article.Slug,
                title = article.Title,
                description = article.Description,
                body = article.Body,
                createdAt = article.CreatedAt,
                updatedAt = article.UpdatedAt,
                tagList,
                favorited = isFavorited,
                favoritesCount,
                author = new
                {
                    username = author!.Username,
                    bio = "",
                    image = "",
                    following
                }
            };
        }

        private string GenerateSlug(string title)
        {
            return title.ToLower().Replace(" ", "-") + "-" + Guid.NewGuid().ToString("N")[..8];
        }

        private int? GetUserIdOrNull()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? int.Parse(claim) : null;
        }
    }
}
