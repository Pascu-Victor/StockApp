namespace StockApp.Models.Articles
{
    using System;

    /// <summary>
    /// Represents a news article with additional metadata such as source, read status, and category.
    /// </summary>
    public class NewsArticle : BaseArticle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewsArticle"/> class.
        /// </summary>
        public NewsArticle()
        {
            this.Source = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsArticle"/> class.
        /// </summary>
        /// <param name="title">The title of the article.</param>
        /// <param name="summary">A brief summary of the article.</param>
        /// <param name="content">The full content of the article.</param>
        /// <param name="publishedOn">The publication date of the article.</param>
        /// <param name="source">The source of the article.</param>
        public NewsArticle(
            string title,
            string summary,
            string content,
            DateTime publishedOn,
            int source)
            : base(title, summary, content, publishedOn)
        {
            this.Source = source;
        }

        /// <summary>
        /// Gets or sets the source of the news article.
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the article has been read.
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the article is related to the watchlist.
        /// </summary>
        public bool IsWatchlistRelated { get; set; } = false;

        /// <summary>
        /// Gets or sets the category of the article.
        /// </summary>
        public string? Category { get; set; }
    }
}
