using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RealWorldApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Body { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public User? Author { get; set; }

        public int ArticleId { get; set; }
        [ForeignKey("ArticleId")]
        [JsonIgnore]
        public Article? Article { get; set; }
    }

}
