using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealWorldApp.Application.Interfaces;
using RealWorldApp.Models;
using System.Security.Claims;

namespace RealWorldApp.Controllers
{
    [Route("api/articles/{slug}/comments")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(string slug, [FromBody] CommentRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _commentService.AddCommentAsync(slug, request.Comment, userId);

            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(string slug)
        {
            var result = await _commentService.GetCommentsAsync(slug);
            return Ok(new { comments = result });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(string slug, int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var success = await _commentService.DeleteCommentAsync(slug, id, userId);
            return success ? NoContent() : NotFound();
        }
    }
}
