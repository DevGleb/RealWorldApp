namespace RealWorldApp.Application.Interfaces
{
    public interface ITagService
    {
        Task<IEnumerable<string>> GetAllTagsAsync();
    }
}
