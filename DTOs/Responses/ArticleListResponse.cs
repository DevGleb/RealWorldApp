namespace RealWorldApp.DTOs.Responses
{
    public class ArticleListResponse
    {
        public List<ArticleResponse> Articles { get; set; } = new();
        public int ArticlesCount { get; set; }
    }
}
