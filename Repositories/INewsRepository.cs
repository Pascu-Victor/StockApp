namespace StockApp.Repositories
{
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;
    using StockApp.Models.Articles;

    /// <summary>
    /// Defines the contract for a repository that manages news articles and user articles.
    /// </summary>
    public interface INewsRepository
    {
        /// <summary>
        /// Adds a new news article to the repository.
        /// </summary>
        /// <param name="newsArticle">The news article to add.</param>
        void AddNewsArticle(NewsArticle newsArticle);

        /// <summary>
        /// Associates related stocks with a news article.
        /// </summary>
        /// <param name="articleId">The ID of the article.</param>
        /// <param name="stockNames">The list of stock names to associate.</param>
        /// <param name="connection">An optional SQL connection.</param>
        /// <param name="transaction">An optional SQL transaction.</param>
        void AddRelatedStocksForArticle(string articleId, List<string> stockNames, SqlConnection? connection = null, SqlTransaction? transaction = null);

        /// <summary>
        /// Adds a user-submitted article to the repository.
        /// </summary>
        /// <param name="userArticle">The user article to add.</param>
        void AddUserArticle(UserArticle userArticle);

        /// <summary>
        /// Approves a user-submitted article.
        /// </summary>
        /// <param name="articleId">The ID of the article to approve.</param>
        void ApproveUserArticle(string articleId);

        /// <summary>
        /// Deletes a news article from the repository.
        /// </summary>
        /// <param name="articleId">The ID of the article to delete.</param>
        void DeleteNewsArticle(string articleId);

        /// <summary>
        /// Deletes a user-submitted article from the repository.
        /// </summary>
        /// <param name="articleId">The ID of the article to delete.</param>
        void DeleteUserArticle(string articleId);

        /// <summary>
        /// Ensures that a user exists in the repository, creating them if necessary.
        /// </summary>
        /// <param name="cnp">The user's unique identifier.</param>
        /// <param name="name">The user's name.</param>
        /// <param name="description">A description of the user.</param>
        /// <param name="isAdmin">Whether the user is an administrator.</param>
        /// <param name="isHidden">Whether the user is hidden.</param>
        /// <param name="profilePicture">The user's profile picture.</param>
        /// <param name="gemBalance">The user's initial gem balance. Defaults to 1000.</param>
        void EnsureUserExists(string cnp, string name, string description, bool isAdmin, bool isHidden, string profilePicture, int gemBalance = 1000);

        /// <summary>
        /// Retrieves all news articles from the repository.
        /// </summary>
        /// <returns>A list of all news articles.</returns>
        List<NewsArticle> GetAllNewsArticles();

        /// <summary>
        /// Retrieves all user-submitted articles from the repository.
        /// </summary>
        /// <returns>A list of all user articles.</returns>
        List<UserArticle> GetAllUserArticles();

        /// <summary>
        /// Retrieves a news article by its ID.
        /// </summary>
        /// <param name="articleId">The ID of the article to retrieve.</param>
        /// <returns>The news article with the specified ID.</returns>
        NewsArticle GetNewsArticleById(string articleId);

        /// <summary>
        /// Retrieves news articles by their category.
        /// </summary>
        /// <param name="category">The category to filter by.</param>
        /// <returns>A list of news articles in the specified category.</returns>
        List<NewsArticle> GetNewsArticlesByCategory(string category);

        /// <summary>
        /// Retrieves news articles related to a specific stock.
        /// </summary>
        /// <param name="stockName">The name of the stock to filter by.</param>
        /// <returns>A list of news articles related to the specified stock.</returns>
        List<NewsArticle> GetNewsArticlesByStock(string stockName);

        /// <summary>
        /// Retrieves a user-submitted article by its ID.
        /// </summary>
        /// <param name="articleId">The ID of the article to retrieve.</param>
        /// <returns>The user article with the specified ID.</returns>
        UserArticle GetUserArticleById(string articleId);

        /// <summary>
        /// Retrieves user-submitted articles by their status.
        /// </summary>
        /// <param name="status">The status to filter by.</param>
        /// <returns>A list of user articles with the specified status.</returns>
        List<UserArticle> GetUserArticlesByStatus(string status);

        /// <summary>
        /// Retrieves user-submitted articles by their topic.
        /// </summary>
        /// <param name="topic">The topic to filter by.</param>
        /// <returns>A list of user articles with the specified topic.</returns>
        List<UserArticle> GetUserArticlesByTopic(string topic);

        /// <summary>
        /// Loads all news articles into the repository.
        /// </summary>
        void LoadNewsArticles();

        /// <summary>
        /// Loads all user-submitted articles into the repository.
        /// </summary>
        void LoadUserArticles();

        /// <summary>
        /// Marks a news article as read.
        /// </summary>
        /// <param name="articleId">The ID of the article to mark as read.</param>
        void MarkArticleAsRead(string articleId);

        /// <summary>
        /// Rejects a user-submitted article.
        /// </summary>
        /// <param name="articleId">The ID of the article to reject.</param>
        void RejectUserArticle(string articleId);

        /// <summary>
        /// Updates an existing news article in the repository.
        /// </summary>
        /// <param name="newsArticle">The news article to update.</param>
        void UpdateNewsArticle(NewsArticle newsArticle);

        /// <summary>
        /// Updates an existing user-submitted article in the repository.
        /// </summary>
        /// <param name="userArticle">The user article to update.</param>
        void UpdateUserArticle(UserArticle userArticle);
    }
}
