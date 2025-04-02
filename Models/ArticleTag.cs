using System.ComponentModel.DataAnnotations.Schema;

namespace RealWorldApp.Models
{
    public class ArticleTag
    {
        public int ArticleId { get; set; }
        [ForeignKey("ArticleId")]
        public Article? Article { get; set; }

        public int TagId { get; set; }
        [ForeignKey("TagId")]
        public Tag? Tag { get; set; }
    }
}
