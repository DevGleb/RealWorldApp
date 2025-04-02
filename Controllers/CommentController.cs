using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.Data;
using RealWorldApp.Models;
using System.Security.Claims;

namespace RealWorldApp.Controllers
{
    [Route("api/articles/{slug}/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(string slug, [FromBody] Comment comment)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Slug == slug);
            if (article == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(comment.Body))
                return BadRequest(new { errors = new { body = new[] { "Comment body is required." } } });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            comment.ArticleId = article.Id;
            comment.AuthorId = userId;
            comment.CreatedAt = DateTime.UtcNow;
            comment.UpdatedAt = DateTime.UtcNow;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var responseBody = new
            {
                comment = new
                {
                    id = comment.Id,
                    createdAt = comment.CreatedAt,
                    updatedAt = comment.UpdatedAt,
                    body = comment.Body,
                    author = new
                    {
                        username = user!.Username,
                        bio = user.Bio ?? "",
                        image = user.Image ?? "",
                        following = false
                    }
                }
            };

            return Created($"/api/articles/{slug}/comments/{comment.Id}", responseBody);
        }


        [HttpGet]
        public async Task<IActionResult> GetComments(string slug)
        {
            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Slug == slug);
            if (article == null)
                return NotFound();

            var comments = await _context.Comments
                .Where(c => c.ArticleId == article.Id)
                .Include(c => c.Author)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var result = comments.Select(c => new
            {
                id = c.Id,
                createdAt = c.CreatedAt,
                updatedAt = c.UpdatedAt,
                body = c.Body,
                author = new
                {
                    username = c.Author!.Username,
                    bio = "",
                    image = "",
                    following = false
                }
            });

            return Ok(new { comments = result });
        }

        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(string slug, int commentId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var article = await _context.Articles.FirstOrDefaultAsync(a => a.Slug == slug);
            if (article == null)
                return NotFound();

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.ArticleId == article.Id);
            if (comment == null)
                return NotFound();

            if (comment.AuthorId != userId)
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent(); 
        }
    }
}
