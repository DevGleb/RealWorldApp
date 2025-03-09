using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RealWorldApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonIgnore] 
        public List<Comment> Comments { get; set; } = new();
    }
}
