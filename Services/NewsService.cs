namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using StockApp.Database;
    using StockApp.Exceptions;
    using StockApp.Models;
    using StockApp.Models.Articles;
    using StockApp.Repositories;

    /// <summary>
    /// Provides business logic for managing news and user-submitted articles.
    /// </summary>
    public class NewsService : INewsService
    {
        private readonly NewsArticlesApiService newsArticlesApi;
        private readonly UserArticlesApiService userArticlesApi;

        private readonly IUserRepository userRepository;
        private readonly INewsRepository newsRepository;

        private static readonly Dictionary<int, NewsArticle> PreviewArticles = [];
        private static readonly Dictionary<int, UserArticle> PreviewUserArticles = [];

        private static List<UserArticle> userArticles = [];
        private static bool isInitialized = false;

        private readonly List<NewsArticle> cachedArticles = [];
        private readonly IBaseStocksRepository stocksRepository;
        private readonly AppDbContext dbContext;
        private readonly ILogger<NewsService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsService"/> class
        /// with default repository implementations.
        /// </summary>
        public NewsService(IUserRepository userRepository, INewsRepository newsRepository, ILogger<NewsService> logger)
        {
            this.userRepository = userRepository;
            this.newsRepository = newsRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsService"/> class
        /// using the specified repositories.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="stocksRepository">The stocks repository.</param>
        public NewsService(NewsArticlesApiService newsArticlesApi, UserArticlesApiService userArticlesApi, IBaseStocksRepository stocksRepository = null)
        {
            this.newsArticlesApi = newsArticlesApi;
            this.userArticlesApi = userArticlesApi;
            this.stocksRepository = stocksRepository;

            if (this.stocksRepository == null && App.Host != null)
            {
                try
                {
                    // Try to get from service provider if available
                    this.stocksRepository = App.Host.Services.GetService<IBaseStocksRepository>();
                }
                catch
                {
                    // Fallback handling
                    Debug.WriteLine("Could not get IBaseStocksRepository from service provider");
                }
            }

            if (!isInitialized)
            {
                // Load initial user-submitted articles into memory
                _ = this.LoadUserArticles();
                isInitialized = true;
            }
        }

        private async Task LoadUserArticles()
        {
            var initialUserArticles = await this.userArticlesApi.GetAllUserArticlesAsync();
            userArticles.AddRange(initialUserArticles);
        }

        /// <summary>
        /// Retrieves and caches all news articles.
        /// </summary>
        /// <returns>A list of <see cref="NewsArticle"/> instances.</returns>
        /// <exception cref="NewsPersistenceException">Thrown if retrieval fails.</exception>
        public async Task<List<NewsArticle>> GetNewsArticles()
        {
            try
            {
                // Fetch articles in a background thread
                var articles = await this.newsArticlesApi.GetAllNewsArticlesAsync();

                // Refresh cache
                this.cachedArticles.Clear();
                this.cachedArticles.AddRange(articles);

                // Inline: For each article, populate its related stocks
                foreach (var article in articles)
                {
                    // FIX: NEEDS CALL TO THE STOCK API
                    //article.RelatedStocks = NewsRepository.GetRelatedStocksForArticle(article.ArticleId);
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
        public async Task<NewsArticle> GetNewsArticleById(int articleId)
        {
            // Return from preview cache if available
            if (PreviewArticles.TryGetValue(articleId, out var previewArticle))
            {
                return previewArticle;
            }

            try
            {
                var article = await this.newsArticlesApi.GetNewsArticleByIdAsync(articleId);
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
        public async Task<bool> MarkArticleAsRead(int articleId)
        {
            try
            {
                await this.newsArticlesApi.MarkNewsArticleAsReadAsync(articleId);
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
        public async Task<bool> CreateArticle(NewsArticle article)
        {
            if (userRepository.CurrentUserCNP == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to create an article");
            }

            try
            {
                await this.newsArticlesApi.CreateNewsArticleAsync(article);
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
        public async Task<List<UserArticle>> GetUserArticles(Status status = Status.Pending, string? topic = null)
        {
            if (userRepository.CurrentUserCNP == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to access user articles");
            }

            User user = userRepository.GetUserByCnpAsync(userRepository.CurrentUserCNP).Result;
            if (user.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to access user articles");
            }

            try
            {
                // Reload from repository
                userArticles = await this.userArticlesApi.GetAllUserArticlesAsync();

                // Inline: apply status filter
                if (status != Status.All)
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
        public async Task<bool> ApproveUserArticle(int articleId)
        {
            if (userRepository.CurrentUserCNP == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to access user articles");
            }

            User user = userRepository.GetUserByCnpAsync(userRepository.CurrentUserCNP).Result;
            if (user.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to access user articles");
            }

            try
            {
                await this.userArticlesApi.ApproveUserArticleAsync(articleId);
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
        public async Task<bool> RejectUserArticle(int articleId)
        {
            if (userRepository.CurrentUserCNP == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to access user articles");
            }

            User user = userRepository.GetUserByCnpAsync(userRepository.CurrentUserCNP).Result;
            if (user.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to access user articles");
            }

            try
            {
                await this.userArticlesApi.RejectUserArticleAsync(articleId);
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
        public async Task<bool> DeleteUserArticle(int articleId)
        {
            if (userRepository.CurrentUserCNP == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to access user articles");
            }

            User user = userRepository.GetUserByCnpAsync(userRepository.CurrentUserCNP).Result;
            if (user.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to access user articles");
            }

            try
            {
                // Remove user article and its published counterpart
                await this.newsArticlesApi.DeleteNewsArticleAsync(articleId);
                await this.userArticlesApi.DeleteUserArticleAsync(articleId);
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
        public async Task<bool> SubmitUserArticle(UserArticle article)
        {
            if (userRepository.CurrentUserCNP == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to access user articles");
            }

            User user = userRepository.GetUserByCnpAsync(userRepository.CurrentUserCNP).Result;
            if (user.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to access user articles");
            }

            // Inline: set author and metadata
            article.AuthorId = user.Id;
            article.PublishedOn = DateTime.Now;
            article.Status = Status.Pending;

            try
            {
                await this.userArticlesApi.AddUserArticleAsync(article);
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
        public User GetCurrentUser()
        {
            if (userRepository.CurrentUserCNP == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to access user articles");
            }

            User user = userRepository.GetUserByCnpAsync(userRepository.CurrentUserCNP).Result;
            if (user.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to access user articles");
            }

            return user;
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
            // WTF IS THIS DOING HERE????
            // Not even toughing this, i give up.

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
                throw new Exception("UNIMPLEMENTED FIX THIS");
                string adminCnp = "6666666666666";
                try
                {
                    this.newsRepository.EnsureUserExists(
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

                //return new User(
                //    adminCnp,
                //    "admin",
                //    "Administrator Account",
                //    true,
                //    "img.jpg",
                //    false,
                //    0);
            }
            else if (password == "user")
            {
                User user = userRepository.GetUserByUsernameAsync(username).Result;
                if (user == null)
                {
                    throw new UnauthorizedAccessException("Invalid username or password");
                }

                userRepository.CurrentUserCNP = user.CNP;
                return user;
            }

            throw new UnauthorizedAccessException("Invalid username or password");
        }

        /// <summary>
        /// Logs out the current user and clears preview caches.
        /// </summary>
        public void Logout()
        {
            PreviewArticles.Clear();
            PreviewUserArticles.Clear();
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
        public async Task StorePreviewArticle(NewsArticle article, UserArticle userArticle)
        {
            // Inline: copy related stocks list if provided
            article.RelatedStocks = userArticle.RelatedStocks != null
                ? [.. userArticle.RelatedStocks]
                : [];

            PreviewArticles[article.Id] = article;
            PreviewUserArticles[article.Id] = userArticle;

            if (article.RelatedStocks?.Count > 0)
            {
                try
                {
                    await this.newsArticlesApi.AddRelatedStocksAsync(article.Id, [.. article.RelatedStocks.Select(stock => stock.Id)]);
                }
                catch (NewsPersistenceException ex)
                {
                    throw new NewsPersistenceException(
                        $"Failed to persist related stocks for preview article '{article.Id}'.", ex);
                }
            }
        }

        /// <summary>
        /// Retrieves a user article for preview by its ID.
        /// </summary>
        /// <param name="articleId">The ID of the preview article.</param>
        /// <returns>The <see cref="UserArticle"/> if found; otherwise null.</returns>
        public UserArticle GetUserArticleForPreview(int articleId)
        {
            if (PreviewUserArticles.TryGetValue(articleId, out var previewArticle))
            {
                return previewArticle;
            }

            // Inline: fallback to main list
            return userArticles.FirstOrDefault(a => a.Id == articleId);
        }

        /// <summary>
        /// Gets related stock symbols for the specified article.
        /// </summary>
        /// <param name="articleId">The ID of the article.</param>
        /// <returns>A list of related stock symbols.</returns>
        /// <exception cref="NewsPersistenceException">If retrieval fails.</exception>
        public List<string> GetRelatedStocksForArticle(int articleId)
        {
            return [];
            //FIXME: this needs to be reimplemented
            // Return preview stocks if available
            if (PreviewUserArticles.TryGetValue(articleId, out var previewUserArticle) &&
                previewUserArticle.RelatedStocks != null &&
                previewUserArticle.RelatedStocks.Any())
            {
                return [.. previewUserArticle.RelatedStocks.Select(stock => stock.Name)];
            }

            try
            {
                // NEEDS CALL TO THE STOCK API, NOT THE ARTICLE SERVICE OR REPO FFS!!!!!!
                //var stocks = NewsRepository.GetRelatedStocksForArticle(articleId);
                //System.Diagnostics.Debug.WriteLine($"GetRelatedStocksForArticle: Found {stocks.Count} stocks");
                //return stocks;
            }
            catch (NewsPersistenceException ex)
            {
                throw new NewsPersistenceException(
                    $"Failed to retrieve related stocks for article '{articleId}'.", ex);
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
        public async Task<List<NewsArticle>> GetCachedArticles()
        {
            return this.cachedArticles.Count > 0
                ? this.cachedArticles
                : await this.newsArticlesApi.GetAllNewsArticlesAsync();
        }
    }
}
