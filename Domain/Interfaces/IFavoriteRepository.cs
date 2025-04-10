namespace RealWorldApp.Domain.Interfaces
{
    public interface IFavoriteRepository
    {
        Task<int> CountByArticleIdAsync(int articleId);
        Task<bool> IsFavoritedAsync(int articleId, int userId);
        Task AddAsync(int articleId, int userId);
        Task RemoveAsync(int articleId, int userId);

    }
}
