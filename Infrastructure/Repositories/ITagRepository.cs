namespace RealWorldApp.Domain.Interfaces
{
    public interface ITagRepository
    {
        Task<IEnumerable<string>> GetAllTagsAsync();
    }
}
