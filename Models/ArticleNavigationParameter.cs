namespace StockApp.Models
{
    /// <summary>  
    /// Data Transfer Object for passing article navigation parameters.  
    /// </summary>  
    public class ArticleNavigationParameter
    {
        /// <summary>  
        /// Gets or sets the ID of the article.  
        /// </summary>  
        public int ArticleId { get; set; }

        /// <summary>  
        /// Gets or sets a value indicating whether the article should be loaded in preview mode.  
        /// </summary>  
        public bool IsPreview { get; set; }
    }
}
