using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockApp.Model;
using StockApp.Repository;

namespace StockApp.Service
{
    public class NewsService
    {
        private readonly AppState _appState;
        private static readonly Dictionary<string, NewsArticle> _previewArticles = new();
        private static readonly Dictionary<string, UserArticle> _previewUserArticles = new();
        private readonly List<NewsArticle> _cachedArticles = new();
        private static readonly List<UserArticle> _userArticles = new();
        private static bool _isInitialized = false;
        private NewsRepository _repository = new NewsRepository();
        private BaseStocksRepository _stocksRepository;

        public NewsService()
        {
            _appState = AppState.Instance;
            _stocksRepository = new BaseStocksRepository();

            if (!_isInitialized)
            {
                _userArticles.AddRange(_repository.GetAllUserArticles());
                _isInitialized = true;
            }
        }

        // Article Methods
        public async Task<List<NewsArticle>> GetNewsArticlesAsync()
        {
            await Task.Delay(200);

            try
            {
                return await Task.Run(() => _repository.GetAllNewsArticles());
            }
            catch
            {
                return _repository.GetAllNewsArticles();
            }
        }

        public async Task<NewsArticle> GetNewsArticleByIdAsync(string lookupId)
        {
            if (string.IsNullOrEmpty(lookupId))
                throw new ArgumentNullException(nameof(lookupId));

            // First check if this is a preview article using the correct lookup ID
            if (_previewArticles.TryGetValue(lookupId, out var previewArticle))
            {
                return previewArticle ?? throw new InvalidOperationException($"Preview article with ID {lookupId} is null");
            }

            await Task.Delay(200);

            try
            {
                var article = await Task.Run(() => _repository.GetNewsArticleById(lookupId));
                return article ?? throw new InvalidOperationException($"Article with ID {lookupId} not found");
            }
            catch
            {
                var mockArticles = _repository.GetAllNewsArticles();
                return mockArticles.FirstOrDefault(a => a.ArticleId == lookupId) 
                    ?? throw new InvalidOperationException($"Article with ID {lookupId} not found in mock data");
            }
        }

        public async Task<bool> MarkArticleAsReadAsync(string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
                throw new ArgumentNullException(nameof(articleId));

            await Task.Delay(100);

            try
            {
                await Task.Run(() => _repository.MarkArticleAsRead(articleId));
                return true;
            }
            catch
            {
                // mark as read in cached articles
                var article = _cachedArticles.FirstOrDefault(a => a.ArticleId == articleId);
                if (article != null)
                {
                    article.IsRead = true;
                }
                return true;
            }
        }

        public async Task<bool> CreateArticleAsync(NewsArticle article)
        {
            if (article == null)
                throw new ArgumentNullException(nameof(article));

            // ensure user is logged in
            if (_appState.CurrentUser == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to create an article");
            }

            await Task.Delay(300);

            try
            {
                await Task.Run(() => _repository.AddNewsArticle(article));
                return true;
            }
            catch
            {
                _cachedArticles.Add(article);
                return true;
            }
        }

        // User Article Methods
        public async Task<List<UserArticle>> GetUserArticlesAsync(string status = null, string topic = null)
        {
            // ensure the user is admin
            if (_appState.CurrentUser == null || !_appState.CurrentUser.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to access user articles");
            }

            await Task.Delay(300);
            var userArticles = new List<UserArticle>();
            try
            {
                userArticles = await Task.Run(() => _repository.GetAllUserArticles());

            }
            catch
            {
                userArticles = new List<UserArticle>(_userArticles);
            }

            // filters
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                userArticles = userArticles.Where(a => a.Status == status).ToList();
            }

            if (!string.IsNullOrEmpty(topic) && topic != "All")
            {
                userArticles = userArticles.Where(a => a.Topic == topic).ToList();
            }

            return userArticles;
        }

        public async Task<bool> ApproveUserArticleAsync(string articleId)
        {
            // ensure the user is admin
            if (_appState.CurrentUser == null || !_appState.CurrentUser.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to approve articles");
            }

            await Task.Delay(300);

            try
            {
                await Task.Run(() => _repository.ApproveUserArticle(articleId));
                _cachedArticles.Clear();
                return true;
            }
            catch
            {
                var article = _userArticles.FirstOrDefault(a => a.ArticleId == articleId);
                if (article != null)
                {
                    article.Status = "Approved";
                    _cachedArticles.Clear();
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> RejectUserArticleAsync(string articleId)
        {
            // ensure the user is admin
            if (_appState.CurrentUser == null || !_appState.CurrentUser.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to reject articles");
            }

            await Task.Delay(300);

            try
            {
                await Task.Run(() => _repository.RejectUserArticle(articleId));
                _cachedArticles.Clear();
                return true;
            }
            catch
            {
                var article = _userArticles.FirstOrDefault(a => a.ArticleId == articleId);
                if (article != null)
                {
                    article.Status = "Rejected";
                    _cachedArticles.Clear();
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> DeleteUserArticleAsync(string articleId)
        {
            // ensure the user is admin
            if (_appState.CurrentUser == null || !_appState.CurrentUser.IsModerator)
            {
                throw new UnauthorizedAccessException("User must be an admin to delete articles");
            }

            await Task.Delay(300);

            try
            {
                await Task.Run(() => _repository.DeleteUserArticle(articleId));
                await Task.Run(() => _repository.DeleteNewsArticle(articleId));
                _cachedArticles.Clear();
                return true;
            }
            catch
            {
                var article = _userArticles.FirstOrDefault(a => a.ArticleId == articleId);
                if (article != null)
                {
                    _userArticles.Remove(article);
                    _cachedArticles.Clear();
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> SubmitUserArticleAsync(UserArticle article)
        {
            // ensure user is logged in
            if (_appState.CurrentUser == null)
            {
                throw new UnauthorizedAccessException("User must be logged in to submit an article");
            }

            // set author and submission date
            article.Author = _appState.CurrentUser.Cnp;
            article.SubmissionDate = DateTime.Now;
            article.Status = "Pending";

            await Task.Delay(300);

            try
            {
                await Task.Run(() => _repository.AddUserArticle(article));
                _cachedArticles.Clear();
                return true;
            }
            catch
            {
                // rn, return success and add to local data
                _userArticles.Add(article);
                _cachedArticles.Clear();
                return true;
            }

        }

        // User Authentication Methods
        public async Task<User> GetCurrentUserAsync()
        {
            // checks if user is already in app state
            if (_appState.CurrentUser != null)
            {
                return _appState.CurrentUser;
            }

            await Task.Delay(200);
            throw new InvalidOperationException("No user is currently logged in");
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            await Task.Delay(300);

            if (username == "admin" && password == "admin")
            {
                string adminCnp = "6666666666666";

                try
                {
                    _repository.EnsureUserExists(
                        adminCnp,
                        "admin",
                        "Administrator Account",
                        true, // isAdmin
                        false, // isHidden
                        "img.jpg"
                    );
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error ensuring admin user exists: {ex.Message}");
                }

                return new User(
                    adminCnp,
                    "admin",
                    "Administrator Account",
                    true,
                    "img.jpg", false);
            }
            else if (username == "user" && password == "user")
            {
                return new User(
                    "1234567890123",
                    "Caramel",
                    "asdf",
                    false,
                    "imagine", false);
            }

            throw new UnauthorizedAccessException("Invalid username or password");
        }

        public void Logout()
        {
            // this supposed to be DIFFERENT BUT AINT NO WAY IT COULD BE CHANGED WITH THE CURRENT CODEBASE
            _appState.CurrentUser = null;
            // clear preview articles
            _previewArticles.Clear();
            _previewUserArticles.Clear();
        }

        // Preview Methods
        public void StorePreviewArticle(NewsArticle article, UserArticle userArticle)
        {
            // First, ensure both articles use the same ID format for consistent lookup
            string articleId = article.ArticleId;
            if (articleId.StartsWith("preview:"))
            {
                articleId = articleId.Substring(8);
                article.ArticleId = articleId;
            }

            // Properly copy related stocks from userArticle to the preview article
            if (userArticle.RelatedStocks != null && userArticle.RelatedStocks.Count > 0)
            {
                article.RelatedStocks = new List<string>(userArticle.RelatedStocks);
                System.Diagnostics.Debug.WriteLine($"StorePreviewArticle: Storing {article.RelatedStocks.Count} related stocks for article {articleId}");
            }
            else
            {
                article.RelatedStocks = new List<string>();
                System.Diagnostics.Debug.WriteLine($"StorePreviewArticle: No related stocks found in user article {articleId}");
            }

            // Store the article and user article in the preview caches
            _previewArticles[articleId] = article;
            _previewUserArticles[articleId] = userArticle;

            // Also update the repository with related stocks for this article
            try
            {
                if (article.RelatedStocks != null && article.RelatedStocks.Count > 0)
                {
                    _repository.AddRelatedStocksForArticle(articleId, article.RelatedStocks, null);
                    System.Diagnostics.Debug.WriteLine($"StorePreviewArticle: Added {article.RelatedStocks.Count} related stocks to repository for article {articleId}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StorePreviewArticle: Error adding related stocks to repository: {ex.Message}");
            }
        }

        public UserArticle GetUserArticleForPreview(string articleId)
        {
            if (_previewUserArticles.TryGetValue(articleId, out var previewArticle))
            {
                return previewArticle;
            }

            // if not in preview cache, check the regular user articles
            return _userArticles.FirstOrDefault(a => a.ArticleId == articleId);
        }

        public List<string> GetRelatedStocksForArticle(string articleId)
        {
            // Remove "preview:" prefix if present
            string actualId = articleId.StartsWith("preview:") ? articleId.Substring(8) : articleId;

            // Check preview dictionary first
            if (_previewUserArticles.TryGetValue(actualId, out var previewUserArticle) &&
                previewUserArticle.RelatedStocks != null &&
                previewUserArticle.RelatedStocks.Any())
            {
                System.Diagnostics.Debug.WriteLine($"GetRelatedStocksForArticle: Found {previewUserArticle.RelatedStocks.Count} stocks in preview");
                return previewUserArticle.RelatedStocks;
            }

            // Then check repository
            try
            {
                var stocks = _repository.GetRelatedStocksForArticle(actualId);
                System.Diagnostics.Debug.WriteLine($"GetRelatedStocksForArticle: Found {stocks.Count} stocks in repository");
                return stocks;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetRelatedStocksForArticle: Error: {ex.Message}");
                return new List<string>();
            }
        }

        public void UpdateCachedArticles(List<NewsArticle> articles)
        {
            _cachedArticles.Clear();
            if (articles != null)
            {
                _cachedArticles.AddRange(articles);
            }
        }

        public List<NewsArticle> GetCachedArticles()
        {
            return _cachedArticles.Count > 0 ? _cachedArticles : _repository.GetAllNewsArticles();
        }
    }
}

