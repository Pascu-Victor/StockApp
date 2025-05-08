namespace BankApi.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using BankApi.Models.Articles;

    public class NewsArticleStock
    {
        [Required]
        public int ArticleId { get; set; }

        [ForeignKey(nameof(ArticleId))]
        public NewsArticle Article { get; set; } = new();

        [Required]
        public int StockId { get; set; }

        [ForeignKey(nameof(StockId))]
        public BaseStock Stock { get; set; } = new();

        public List<NewsArticle> RelatedNewsArticles = [];

        public List<BaseStock> RelatedStocks = [];
    }
}
