using Microsoft.AspNetCore.Mvc;
using RealWorldApp.Data;
using System.Linq;

namespace RealWorldApp.Controllers
{
    [Route("api/tags")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TagsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetTags()
        {
            var tags = _context.Tags
                .Select(t => t.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            return Ok(new { tags });
        }
    }
}
