using RealWorldApp.Application.Interfaces;
using RealWorldApp.Domain.Interfaces;

namespace RealWorldApp.Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<string>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAllTagsAsync();
        }
    }
}
