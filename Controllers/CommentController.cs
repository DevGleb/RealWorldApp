using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.Models;
using System.Security.Claims;
using RealWorldApp.Data;

namespace RealWorldApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly ILogger<CommentController> _logger;

        public CommentController(AppDbContext context, ILogger<CommentController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [HttpPost("{articleId}")]
        [Authorize]
        public async Task<IActionResult> AddComment(int articleId, [FromBody] Comment comment)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var article = await _context.Articles.FindAsync(articleId);

            if (article == null)
                return NotFound("Article not found");

            comment.AuthorId = userId;
            comment.ArticleId = articleId;
            comment.CreatedAt = DateTime.UtcNow;
            comment.UpdatedAt = DateTime.UtcNow;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);
        }

        [HttpGet("{articleId}")]
        public async Task<IActionResult> GetComments(int articleId)
        {
            var comments = await _context.Comments
                .Where(c => c.ArticleId == articleId)
                .Include(c => c.Author)
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPut("{commentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] Comment updatedComment)
        {
            _logger.LogInformation($"Updating comment with ID {commentId}");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                _logger.LogWarning($"Comment with ID {commentId} not found.");
                return NotFound(new { message = $"Comment with ID {commentId} not found." });
            }

            if (comment.AuthorId != userId)
            {
                _logger.LogWarning($"User {userId} is not the owner of comment {commentId}");
                return Forbid();
            }

            comment.Body = updatedComment.Body;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Comment {commentId} updated successfully");

            return Ok(comment);
        }

        [HttpDelete("{commentId}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
                return NotFound("Comment not found");

            if (comment.AuthorId != userId)
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok("Comment deleted");
        }

        [HttpGet("{articleId}/comments")]
        public async Task<IActionResult> GetComments(
            int articleId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string order = "asc")
        {
            var query = _context.Comments
                .Where(c => c.ArticleId == articleId)
                .Include(c => c.Author) 
                .AsQueryable(); 

  
            query = sortBy.ToLower() switch
            {
                "createdat" => order.ToLower() == "asc" ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
                _ => query.OrderBy(c => c.CreatedAt)
            };


            var totalItems = await query.CountAsync();
            var comments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                comments
            });
        }

    }
}
