using RealWorldApp.Application.Interfaces;
using RealWorldApp.Domain.Interfaces;
using RealWorldApp.DTOs;
using RealWorldApp.DTOs.Responses;
using RealWorldApp.Models;

namespace RealWorldApp.Application.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFavoriteRepository _favoriteRepository;

        public ArticleService(
            IArticleRepository articleRepository,
            IUserRepository userRepository,
            IFavoriteRepository favoriteRepository)
        {
            _articleRepository = articleRepository;
            _userRepository = userRepository;
            _favoriteRepository = favoriteRepository;
        }

        public async Task<ArticleListResponse> GetAllArticlesAsync(int limit, int offset, int? userId)
        {
            var totalCount = await _articleRepository.CountAsync();
            var articles = await _articleRepository.GetAllAsync(limit, offset);

            var result = new List<ArticleResponse>();
            foreach (var article in articles)
            {
                var articleData = await BuildArticleData(article, userId);
                result.Add(articleData);
            }

            return new ArticleListResponse
            {
                Articles = result,
                ArticlesCount = totalCount
            };
        }

        public async Task<ArticleResponse?> GetArticleBySlugAsync(string slug, int? userId)
        {
            var article = await _articleRepository.GetBySlugAsync(slug);
            if (article == null) return null;

            return await BuildArticleData(article, userId);
        }

        public async Task<ArticleResponse> CreateArticleAsync(ArticleData data, int userId)
        {
            var article = new Article
            {
                Title = data.Title,
                Description = data.Description,
                Body = data.Body,
                AuthorId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Slug = GenerateSlug(data.Title),
                ArticleTags = new List<ArticleTag>()
            };

            await _articleRepository.AddAsync(article);

            if (data.TagList != null)
            {
                foreach (var tagName in data.TagList.Distinct())
                {
                    await _articleRepository.AddOrAttachTagAsync(article, tagName);
                }
            }

            return await BuildArticleData(article, userId);
        }

        public async Task<ArticleResponse?> UpdateArticleAsync(string slug, ArticleData updated, int userId)
        {
            var article = await _articleRepository.GetBySlugAsync(slug);
            if (article == null || article.AuthorId != userId)
                return null;

            article.Title = updated.Title ?? article.Title;
            article.Description = updated.Description ?? article.Description;
            article.Body = updated.Body ?? article.Body;
            article.UpdatedAt = DateTime.UtcNow;

            await _articleRepository.UpdateAsync(article);

            return await BuildArticleData(article, userId);
        }


        public async Task<bool> DeleteArticleAsync(string slug, int userId)
        {
            var article = await _articleRepository.GetBySlugAsync(slug);
            if (article == null || article.AuthorId != userId)
                return false;

            await _articleRepository.DeleteAsync(article);
            return true;
        }

        public async Task<ArticleListResponse> GetFeedArticlesAsync(int limit, int offset, int userId)
        {
            var followingIds = await _userRepository.GetFollowingIdsAsync(userId);
            var totalCount = await _articleRepository.CountByAuthorsAsync(followingIds);
            var articles = await _articleRepository.GetByAuthorsAsync(followingIds, limit, offset);

            var result = new List<ArticleResponse>();
            foreach (var article in articles)
            {
                var articleData = await BuildArticleData(article, userId);
                result.Add(articleData);
            }

            return new ArticleListResponse
            {
                Articles = result,
                ArticlesCount = totalCount
            };
        }

        public async Task<ArticleResponse?> FavoriteArticleAsync(string slug, int userId)
        {
            var article = await _articleRepository.GetBySlugAsync(slug);
            if (article == null)
                return null;

            var alreadyFavorited = await _favoriteRepository.IsFavoritedAsync(article.Id, userId);
            if (!alreadyFavorited)
            {
                await _favoriteRepository.AddAsync(article.Id, userId);
            }

            return await BuildArticleData(article, userId);
        }

        public async Task<ArticleResponse?> UnfavoriteArticleAsync(string slug, int userId)
        {
            var article = await _articleRepository.GetBySlugAsync(slug);
            if (article == null)
                return null;

            var favorited = await _favoriteRepository.IsFavoritedAsync(article.Id, userId);
            if (favorited)
            {
                await _favoriteRepository.RemoveAsync(article.Id, userId);
            }

            return await BuildArticleData(article, userId);
        }

        private async Task<ArticleResponse> BuildArticleData(Article article, int? userId)
        {
            var author = await _userRepository.GetByIdAsync(article.AuthorId);
            var tagList = await _articleRepository.GetTagsAsync(article.Id);
            var favoritesCount = await _favoriteRepository.CountByArticleIdAsync(article.Id);
            var isFavorited = userId.HasValue && await _favoriteRepository.IsFavoritedAsync(article.Id, userId.Value);

            return new ArticleResponse
            {
                Slug = article.Slug,
                Title = article.Title,
                Description = article.Description,
                Body = article.Body,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt,
                TagList = tagList,
                Favorited = isFavorited,
                FavoritesCount = favoritesCount,
                Author = new AuthorResponse
                {
                    Username = author?.Username ?? "",
                    Bio = author?.Bio ?? "",
                    Image = author?.Image ?? "",
                    Following = false
                }
            };
        }

        private string GenerateSlug(string title)
        {
            return title.ToLower().Replace(" ", "-") + "-" + Guid.NewGuid().ToString("N")[..8];
        }
    }
}
