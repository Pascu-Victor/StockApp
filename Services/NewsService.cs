﻿namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using StockApp.Exceptions;
    using StockApp.Models;
    using StockApp.Repositories;

    /// <summary>
    /// Provides business logic for managing news and user-submitted articles.
    /// </summary>
    public class NewsService : INewsService
    {
        private readonly AppState appState;
        private static readonly Dictionary<string, NewsArticle> previewArticles = [];
        private static readonly Dictionary<string, UserArticle> previewUserArticles = [];
        private readonly List<NewsArticle> cachedArticles = [];
        private static List<UserArticle> userArticles = [];
        private static bool isInitialized = false;
        private readonly INewsRepository repository;
        private readonly IBaseStocksRepository stocksRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsService"/> class
        /// with default repository implementations.
        /// </summary>
        public NewsService() : this(new NewsRepository(), new BaseStocksRepository())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsService"/> class
        /// using the specified repositories.
        /// </summary>
        /// <param name="repository">The news repository.</param>
        /// <param name="stocksRepository">The base stocks repository.</param>
        public NewsService(INewsRepository repository, IBaseStocksRepository stocksRepository)
        {
            this.repository = repository;
            this.stocksRepository = stocksRepository; // FIXME: stocksRepository is never used in this class.
            this.appState = AppState.Instance;

            if (!isInitialized)
            {
                // Load initial user-submitted articles into memory
                var initialUserArticles = this.repository.GetAllUserArticles() ?? [];
                userArticles.AddRange(initialUserArticles);
                isInitialized = true;
            }
        }

        /// <summary>
        /// Retrieves and caches all news articles.
        /// </summary>
        /// <returns>A list of <see cref="NewsArticle"/> instances.</returns>
        /// <exception cref="NewsPersistenceException">Thrown if retrieval fails.</exception>
        public async Task<List<NewsArticle>> GetNewsArticlesAsync()
        {
            try
            {
                // Fetch articles in a background thread
                var articles = await Task.Run(() => this.repository.GetAllNewsArticles());

                // Refresh cache
                this.cachedArticles.Clear();
                this.cachedArticles.AddRange(articles);

                // Inline: For each article, populate its related stocks
                foreach (var article in articles)
                {
                    article.RelatedStocks = NewsRepository.GetRelatedStocksForArticle(article.ArticleId);
                }

                return articles;
            }
            catch (NewsPersistenceException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to retrieve news articles: {ex.Message}");
                throw new NewsPersistenceException("Failed to retrieve news articles.", ex);
            }
        }

        /// <summary>
        /// Retrieves a news article by ID, checking preview cache first.
        /// </summary>
        /// <param name="articleId">The ID of the article to fetch.</param>
        /// <returns>The requested <see cref="NewsArticle"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="articleId"/> is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">If no article matches the ID.</exception>
        /// <exception cref="NewsPersistenceException">If retrieval fails.</exception>
        public async Task<NewsArticle> GetNewsArticleByIdAsync(string articleId)
        {
            if (string.IsNullOrWhiteSpace(articleId))
            {
                throw new ArgumentNullException(nameof(articleId));
            }

            // Return from preview cache if available
            if (previewArticles.TryGetValue(articleId, out var previewArticle))
            {
                return previewArticle;
            }

            await Task.Delay(200); // TODO: Replace artificial delay with real async call

            try
            {
                var article = await Task.Run(() => this.repository.GetNewsArticleById(articleId));
                return article ?? throw new KeyNotFoundException($"Article with ID {articleId} not found");
            }
            catch (NewsPersistenceException ex)
            {
                throw new NewsPersistenceException($"Failed to load article with ID {articleId}.", ex);
            }
        }

        /// <summary>
        /// Marks the specified article as read.
        /// </summary>
        /// <param name="articleId">The ID of the article to mark.</param>
        /// <returns>True if successful.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="articleId"/> is null or empty.</exception>
        /// <exception cref="NewsPersistenceException">If marking fails.</exception>
        public async Task<bool> MarkArticleAsReadAsync(string articleId)
        {
            if (string.IsNullOrWhiteSpace(articleId))
            {
                throw new ArgumentNullException(nameof(articleId));
            }

            await Task.Delay(100); // TODO: Remove hardcoded delay

            try
            {
                await Task.Run(() => this.repository.MarkArticleAsRead(articleId));
                return true;
            }
            catch (NewsPersistenceException ex)
            {
                throw new NewsPersistenceException($"Failed to mark article {articleId} as read.", ex);
            }
        }

        /// <summary>
        /// Creates a new news article.
        /// </summary>
        /// <param name="article">The article to create.</param>
        /// <returns>True if creation succeeded.</returns>
        /// <exception cref="UnauthorizedAccessException">If no user is logged in.</exception>
        /// <exception cref="NewsPersistenceException">If creation fails.</exception>
        public async Task<bool> CreateArticleAsync(NewsArticle article)
        {
            if (this.appState.CurrentUser == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to create an article");
            }

            await Task.Delay(300); // TODO: Remove artificial delay

            try
            {
                await Task.Run(() => this.repository.AddNewsArticle(article));
                return true;
            }
            catch (NewsPersistenceException ex)
            {
                throw new NewsPersistenceException("Failed to create news article.", ex);
            }
        }

        /// <summary>
        /// Retrieves user-submitted articles, optionally filtering by status and topic.
        /// </summary>
        /// <param name="status">The status to filter by (or null for all).</param>
        /// <param name="topic">The topic to filter by (or null for all).</param>
        /// <returns>A list of <see cref="UserArticle"/> instances.</returns>
        /// <exception cref="UnauthorizedAccessException">If current user is not an admin.</exception>
        /// <exception cref="NewsPersistenceException">If loading fails.</exception>
        public async Task<List<UserArticle>> GetUserArticlesAsync(string status = null, string topic = null)
        {
            if (this.appState.CurrentUser == null || !this.appState.CurrentUser.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to access user articles");
            }

            await Task.Delay(300); // TODO: Remove artificial delay

            try
            {
                // Reload from repository
                userArticles = await Task.Run(() => this.repository.GetAllUserArticles());

                // Inline: apply status filter
                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    userArticles = [.. userArticles.Where(a => a.Status == status)];
                }

                // Inline: apply topic filter
                if (!string.IsNullOrEmpty(topic) && topic != "All")
                {
                    userArticles = [.. userArticles.Where(a => a.Topic == topic)];
                }

                return userArticles;
            }
            catch (NewsPersistenceException ex)
            {
                throw new NewsPersistenceException("Failed to load user articles.", ex);
            }
        }

        /// <summary>
        /// Approves a pending user article.
        /// </summary>
        /// <param name="articleId">The ID of the article to approve.</param>
        /// <returns>True if approval succeeded.</returns>
        /// <exception cref="UnauthorizedAccessException">If current user is not an admin.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="articleId"/> is null or empty.</exception>
        /// <exception cref="NewsPersistenceException">If approval fails.</exception>
        public async Task<bool> ApproveUserArticleAsync(string articleId)
        {
            if (this.appState.CurrentUser == null || !this.appState.CurrentUser.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to approve articles");
            }

            if (string.IsNullOrWhiteSpace(articleId))
            {
                throw new ArgumentNullException(nameof(articleId));
            }

            await Task.Delay(300); // TODO: Remove artificial delay

            try
            {
                await Task.Run(() => this.repository.ApproveUserArticle(articleId));
                this.cachedArticles.Clear(); // Invalidate cache after approval
                return true;
            }
            catch (NewsPersistenceException ex)
            {
                throw new NewsPersistenceException($"Failed to approve article {articleId}.", ex);
            }
        }

        /// <summary>
        /// Rejects a pending user article.
        /// </summary>
        /// <param name="articleId">The ID of the article to reject.</param>
        /// <returns>True if rejection succeeded.</returns>
        /// <exception cref="UnauthorizedAccessException">If current user is not an admin.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="articleId"/> is null or empty.</exception>
        /// <exception cref="NewsPersistenceException">If rejection fails.</exception>
        public async Task<bool> RejectUserArticleAsync(string articleId)
        {
            if (this.appState.CurrentUser == null || !this.appState.CurrentUser.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to reject articles");
            }

            if (string.IsNullOrWhiteSpace(articleId))
            {
                throw new ArgumentNullException(nameof(articleId));
            }

            await Task.Delay(300); // TODO: Remove artificial delay

            try
            {
                await Task.Run(() => this.repository.RejectUserArticle(articleId));
                this.cachedArticles.Clear(); // Invalidate cache after rejection
                return true;
            }
            catch (NewsPersistenceException ex)
            {
                throw new NewsPersistenceException($"Failed to reject article {articleId}.", ex);
            }
        }

        /// <summary>
        /// Deletes both a user-submitted article and its corresponding news article.
        /// </summary>
        /// <param name="articleId">The ID of the article to delete.</param>
        /// <returns>True if deletion succeeded.</returns>
        /// <exception cref="UnauthorizedAccessException">If current user is not an admin.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="articleId"/> is null or empty.</exception>
        /// <exception cref="NewsPersistenceException">If deletion fails.</exception>
        public async Task<bool> DeleteUserArticleAsync(string articleId)
        {
            if (this.appState.CurrentUser == null || !this.appState.CurrentUser.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to delete articles");
            }

            if (string.IsNullOrWhiteSpace(articleId))
            {
                throw new ArgumentNullException(nameof(articleId));
            }

            await Task.Delay(300); // TODO: Remove artificial delay

            try
            {
                // Remove user article and its published counterpart
                await Task.Run(() => this.repository.DeleteUserArticle(articleId));
                await Task.Run(() => this.repository.DeleteNewsArticle(articleId));
                this.cachedArticles.Clear(); // Invalidate cache after deletion
                return true;
            }
            catch (NewsPersistenceException ex)
            {
                throw new NewsPersistenceException($"Failed to delete article {articleId}.", ex);
            }
        }

        /// <summary>
        /// Submits a user article for review.
        /// </summary>
        /// <param name="article">The user article to submit.</param>
        /// <returns>True if submission succeeded.</returns>
        /// <exception cref="UnauthorizedAccessException">If no user is logged in.</exception>
        /// <exception cref="NewsPersistenceException">If submission fails.</exception>
        public async Task<bool> SubmitUserArticleAsync(UserArticle article)
        {
            if (this.appState.CurrentUser == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to submit an article");
            }

            // Inline: set author and metadata
            article.Author = this.appState.CurrentUser;
            article.SubmissionDate = DateTime.Now;
            article.Status = "Pending";

            await Task.Delay(300); // TODO: Remove artificial delay

            try
            {
                await Task.Run(() => this.repository.AddUserArticle(article));
                this.cachedArticles.Clear(); // Invalidate cache after submission
                return true;
            }
            catch (NewsPersistenceException ex)
            {
                throw new NewsPersistenceException("Failed to submit user article.", ex);
            }
        }

        /// <summary>
        /// Gets the currently logged-in user from application state.
        /// </summary>
        /// <returns>The current <see cref="User"/>.</returns>
        /// <exception cref="InvalidOperationException">If no user is logged in.</exception>
        public async Task<User> GetCurrentUserAsync()
        {
            if (this.appState.CurrentUser != null)
            {
                return this.appState.CurrentUser;
            }

            await Task.Delay(200); // TODO: Remove artificial delay
            throw new InvalidOperationException("No user is currently logged in");
        }

        /// <summary>
        /// Authenticates a user by username and password.
        /// </summary>
        /// <param name="username">The username to authenticate.</param>
        /// <param name="password">The password to verify.</param>
        /// <returns>The authenticated <see cref="User"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="username"/> or <paramref name="password"/> is null or empty.</exception>
        /// <exception cref="UnauthorizedAccessException">If credentials are invalid.</exception>
        public async Task<User> LoginAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            await Task.Delay(300); // TODO: Replace with real authentication call

            if (username == "admin" && password == "admin")
            {
                string adminCnp = "6666666666666";
                try
                {
                    this.repository.EnsureUserExists(
                        adminCnp,
                        "admin",
                        "Administrator Account",
                        true,  // isAdmin
                        false, // isHidden
                        "img.jpg");
                }
                catch (NewsPersistenceException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error ensuring admin user exists: {ex.Message}");
                }

                return new User(
                    adminCnp,
                    "admin",
                    "Administrator Account",
                    true,
                    "img.jpg",
                    false,
                    0);
            }
            else if (username == "user" && password == "user")
            {
                return new User(
                    "1234567890123",
                    "Caramel",
                    "asdf",
                    false,
                    "imagine",
                    false,
                    1000);
            }

            throw new UnauthorizedAccessException("Invalid username or password");
        }

        /// <summary>
        /// Logs out the current user and clears preview caches.
        /// </summary>
        public void Logout()
        {
            // TODO: Refine logout logic to persist session state if needed
            this.appState.CurrentUser = null;
            previewArticles.Clear();
            previewUserArticles.Clear();
        }

        /// <summary>
        /// Stores a preview of a news article and its corresponding user article.
        /// </summary>
        /// <param name="article">The news article preview.</param>
        /// <param name="userArticle">The user article preview.</param>
        /// <remarks>
        /// Strips any "preview:" prefix from <c>ArticleId</c> before storage.
        /// </remarks>
        /// <exception cref="NewsPersistenceException">If persisting related stocks fails.</exception>
        public void StorePreviewArticle(NewsArticle article, UserArticle userArticle)
        {
            // Inline: normalize ID by removing "preview:" prefix if present
            string articleId = article.ArticleId.StartsWith("preview:") ? article.ArticleId[8..] : article.ArticleId;
            article.ArticleId = articleId;

            // Inline: copy related stocks list if provided
            article.RelatedStocks = userArticle.RelatedStocks != null
                ? [.. userArticle.RelatedStocks]
                : [];

            previewArticles[articleId] = article;
            previewUserArticles[articleId] = userArticle;

            if (article.RelatedStocks?.Count > 0)
            {
                try
                {
                    this.repository.AddRelatedStocksForArticle(articleId, article.RelatedStocks, null, null);
                }
                catch (NewsPersistenceException ex)
                {
                    throw new NewsPersistenceException(
                        $"Failed to persist related stocks for preview article '{articleId}'.", ex);
                }
            }
        }

        /// <summary>
        /// Retrieves a user article for preview by its ID.
        /// </summary>
        /// <param name="articleId">The ID of the preview article.</param>
        /// <returns>The <see cref="UserArticle"/> if found; otherwise null.</returns>
        public UserArticle GetUserArticleForPreview(string articleId)
        {
            if (previewUserArticles.TryGetValue(articleId, out var previewArticle))
            {
                return previewArticle;
            }

            // Inline: fallback to main list
            return userArticles.FirstOrDefault(a => a.ArticleId == articleId);
        }

        /// <summary>
        /// Gets related stock symbols for the specified article.
        /// </summary>
        /// <param name="articleId">The ID of the article.</param>
        /// <returns>A list of related stock symbols.</returns>
        /// <exception cref="NewsPersistenceException">If retrieval fails.</exception>
        public List<string> GetRelatedStocksForArticle(string articleId)
        {
            // Inline: normalize ID
            string actualId = articleId.StartsWith("preview:") ? articleId[8..] : articleId;

            // Return preview stocks if available
            if (previewUserArticles.TryGetValue(actualId, out var previewUserArticle) &&
                previewUserArticle.RelatedStocks != null &&
                previewUserArticle.RelatedStocks.Any())
            {
                return previewUserArticle.RelatedStocks;
            }

            try
            {
                var stocks = NewsRepository.GetRelatedStocksForArticle(actualId);
                System.Diagnostics.Debug.WriteLine($"GetRelatedStocksForArticle: Found {stocks.Count} stocks");
                return stocks;
            }
            catch (NewsPersistenceException ex)
            {
                throw new NewsPersistenceException(
                    $"Failed to retrieve related stocks for article '{actualId}'.", ex);
            }
        }

        /// <summary>
        /// Updates the internal cache of news articles.
        /// </summary>
        /// <param name="articles">The new list of articles to cache.</param>
        public void UpdateCachedArticles(List<NewsArticle> articles)
        {
            this.cachedArticles.Clear();
            if (articles != null)
            {
                this.cachedArticles.AddRange(articles);
            }
        }

        /// <summary>
        /// Gets the current cache of news articles, or fetches from repository if empty.
        /// </summary>
        /// <returns>A list of <see cref="NewsArticle"/>.</returns>
        public List<NewsArticle> GetCachedArticles()
        {
            return this.cachedArticles.Count > 0
                ? this.cachedArticles
                : this.repository.GetAllNewsArticles();
        }
    }
}
