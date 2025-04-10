public class ArticleResponse
{
    public string Slug { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Body { get; set; } = default!;
    public List<string> TagList { get; set; } = new();
    public bool Favorited { get; set; }
    public int FavoritesCount { get; set; }
    public AuthorDto Author { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AuthorDto
{
    public string Username { get; set; } = default!;
    public string Bio { get; set; } = default!;
    public string Image { get; set; } = default!;
    public bool Following { get; set; }
}
