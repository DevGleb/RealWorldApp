using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RealWorldApp.Application.Services;
using RealWorldApp.Domain.Interfaces;
using Xunit;

namespace RealWorldApp.Tests.Services
{
    public class TagServiceTests
    {
        private readonly Mock<ITagRepository> _tagRepoMock;
        private readonly TagService _tagService;

        public TagServiceTests()
        {
            _tagRepoMock = new Mock<ITagRepository>();
            _tagService = new TagService(_tagRepoMock.Object);
        }

        [Fact]
        public async Task GetAllTagsAsync_ReturnsTagList()
        {
            var tags = new List<string> { "angular", "react", "vue" };
            _tagRepoMock.Setup(r => r.GetAllTagsAsync()).ReturnsAsync(tags);

            var result = await _tagService.GetAllTagsAsync();

            Assert.Equal(3, result.Count());
            Assert.Contains("angular", result);
            Assert.Contains("react", result);
            Assert.Contains("vue", result);
        }
    }
}
