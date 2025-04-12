namespace RealWorldApp.DTOs.Responses
{
    public class ArticleResponse
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> TagList { get; set; } = new();
        public bool Favorited { get; set; }
        public int FavoritesCount { get; set; }

        public AuthorResponse Author { get; set; } = new();
    }

    public class AuthorResponse
    {
        public string Username { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool Following { get; set; }
    }
}
