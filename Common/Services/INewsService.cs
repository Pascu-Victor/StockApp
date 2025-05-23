using Common.Models;

namespace Common.Services
{
    public interface INewsService
    {
        Task<bool> ApproveUserArticleAsync(string articleId, string? userCNP = null);

        Task<bool> CreateArticleAsync(NewsArticle article, string? authorCNP = null);

        Task<bool> DeleteUserArticleAsync(string articleId, string? userCNP = null);

        Task<NewsArticle> GetNewsArticleByIdAsync(string articleId);

        Task<List<NewsArticle>> GetNewsArticlesAsync();

        Task<List<Stock>> GetRelatedStocksForArticleAsync(string articleId);

        Task<List<NewsArticle>> GetUserArticlesAsync(Status status = Status.All, string topic = "All", string? authorCNP = null);

        Task<bool> MarkArticleAsReadAsync(string articleId);

        Task<bool> RejectUserArticleAsync(string articleId, string? userCNP = null);

        Task<bool> SubmitUserArticleAsync(NewsArticle article, string? userCNP = null);

        /// <summary>
        /// Filters and sorts news articles based on category and search query.
        /// </summary>
        /// <param name="category">The category to filter by. Use "All" for no category filter.</param>
        /// <param name="searchQuery">The search query to filter articles by.</param>
        /// <returns>A filtered and sorted list of news articles.</returns>
        Task<List<NewsArticle>> GetFilteredArticlesAsync(string category = "All", string searchQuery = "");
    }
}