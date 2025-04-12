namespace RealWorldApp.DTOs.Responses
{
    public class CommentResponse
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Body { get; set; } = string.Empty;
        public AuthorResponse Author { get; set; } = new();
    }
}
