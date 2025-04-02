using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RealWorldApp.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User? Author { get; set; }

        [JsonIgnore]
        public List<Comment> Comments { get; set; } = new();

        public List<ArticleTag> ArticleTags { get; set; } = new();
    }
}
