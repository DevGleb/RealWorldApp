using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.Data;
using RealWorldApp.Models;
using System.Security.Claims;

namespace RealWorldApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ArticleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllArticles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string order = "desc",
            [FromQuery] int? authorId = null)
        {
            var query = _context.Articles.Include(a => a.Author).AsQueryable();

            if (authorId.HasValue)
            {
                query = query.Where(a => a.AuthorId == authorId);
            }

            query = sortBy.ToLower() switch
            {
                "title" => order.ToLower() == "asc" ? query.OrderBy(a => a.Title) : query.OrderByDescending(a => a.Title),
                "createdat" => order.ToLower() == "asc" ? query.OrderBy(a => a.CreatedAt) : query.OrderByDescending(a => a.CreatedAt),
                _ => query.OrderByDescending(a => a.CreatedAt)
            };

            var totalItems = await query.CountAsync();
            var articles = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                articles
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetArticle(int id)
        {
            var article = await _context.Articles.Include(a => a.Author).FirstOrDefaultAsync(a => a.Id == id);
            if (article == null)
                return NotFound();

            return Ok(article);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateArticle([FromBody] Article article)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            article.AuthorId = userId;
            article.CreatedAt = DateTime.UtcNow;
            article.UpdatedAt = DateTime.UtcNow;

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateArticle(int id, [FromBody] Article updatedArticle)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (article.AuthorId != userId)
                return Forbid(); 

            article.Title = updatedArticle.Title;
            article.Description = updatedArticle.Description;
            article.Body = updatedArticle.Body;
            article.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(article);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (article.AuthorId != userId)
                return Forbid();

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
