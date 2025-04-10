using Microsoft.EntityFrameworkCore;
using RealWorldApp.Data;
using RealWorldApp.Domain.Interfaces;

namespace RealWorldApp.Infrastructure.Repositories
{
    public class TagRepository : Domain.Interfaces.ITagRepository
    {
        private readonly AppDbContext _context;

        public TagRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<string>> GetAllTagsAsync()
        {
            return await _context.Tags
                .Select(t => t.Name)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }
    }
}
