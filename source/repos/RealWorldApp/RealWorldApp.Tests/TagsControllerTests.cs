using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealWorldApp.Controllers;
using RealWorldApp.Data;
using RealWorldApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace RealWorldApp.Tests
{
    public class TagsControllerTests
    {
        private static AppDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public void GetTags_ReturnsAllTags()
        {
            var context = CreateInMemoryContext(nameof(GetTags_ReturnsAllTags));
            context.Tags.AddRange(
                new Tag { Name = "react" },
                new Tag { Name = "angular" },
                new Tag { Name = "vue" },
                new Tag { Name = "react" } 
            );
            context.SaveChanges();

            var controller = new TagsController(context);


            var result = controller.GetTags() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var tagResult = result.Value!.GetType().GetProperty("tags")?.GetValue(result.Value) as IEnumerable<string>;
            Assert.NotNull(tagResult);

            var tagList = tagResult!.ToList();
            Assert.Equal(3, tagList.Count); 
            Assert.Equal(new List<string> { "angular", "react", "vue" }, tagList); 
        }
    }
}
