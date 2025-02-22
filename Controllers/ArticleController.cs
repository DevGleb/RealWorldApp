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

        [HttpGet]
        public async Task<IActionResult> GetAllArticles()
        {
            var articles = await _context.Articles.Include(a => a.Author).ToListAsync();
            return Ok(articles);
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
