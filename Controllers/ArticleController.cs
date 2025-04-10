using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealWorldApp.Application.Interfaces;
using RealWorldApp.DTOs;
using RealWorldApp.Models;
using System.Security.Claims;
using FluentValidation;
using FluentValidation.Results;

namespace RealWorldApp.Controllers
{
    [Route("api/articles")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticleController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllArticles([FromQuery] int limit = 20, [FromQuery] int offset = 0)
        {
            var userId = GetUserIdOrNull();
            var result = await _articleService.GetAllArticlesAsync(limit, offset, userId);
            return Ok(result);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetArticle(string slug)
        {
            var userId = GetUserIdOrNull();
            var result = await _articleService.GetArticleBySlugAsync(slug, userId);
            return result is null ? NotFound() : Ok(result);
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

            ValidationResult validationResult = await dtoValidator.ValidateAsync(data);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.ToDictionary()
                });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _articleService.CreateArticleAsync(data, userId);

            return Created($"/api/articles/{((dynamic)result).slug}", new { article = result });
        }

        [HttpPut("{slug}")]
        [Authorize]
        public async Task<IActionResult> UpdateArticle(string slug, [FromBody] Article updated)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _articleService.UpdateArticleAsync(slug, updated, userId);
            return result is null ? NotFound() : Ok(new { article = result });
        }

        [HttpDelete("{slug}")]
        [Authorize]
        public async Task<IActionResult> DeleteArticle(string slug)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var success = await _articleService.DeleteArticleAsync(slug, userId);
            return success ? NoContent() : NotFound();
        }

        [HttpGet("feed")]
        [Authorize]
        public async Task<IActionResult> GetFeedArticles([FromQuery] int limit = 20, [FromQuery] int offset = 0)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _articleService.GetFeedArticlesAsync(limit, offset, userId);
            return Ok(result);
        }

        [HttpPost("{slug}/favorite")]
        [Authorize]
        public async Task<IActionResult> FavoriteArticle(string slug)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _articleService.FavoriteArticleAsync(slug, userId);
            return result is null ? NotFound() : Ok(new { article = result });
        }

        [HttpDelete("{slug}/favorite")]
        [Authorize]
        public async Task<IActionResult> UnfavoriteArticle(string slug)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _articleService.UnfavoriteArticleAsync(slug, userId);
            return result is null ? NotFound() : Ok(new { article = result });
        }

        private int? GetUserIdOrNull()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return claim != null ? int.Parse(claim) : null;
        }
    }
}
