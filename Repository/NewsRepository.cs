using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using StockApp.Database;
using StockApp.Model;

namespace StockApp.Repository
{
    public class NewsRepository
    {
        private DatabaseHelper _databaseHelper = DatabaseHelper.Instance;
        private List<NewsArticle> newsArticles = new List<NewsArticle>();
        private List<UserArticle> userArticles = new List<UserArticle>();
        private static readonly object _lockObject = new object();
        private static bool _isInitialized = false;

        public NewsRepository()
        {
            Initialize();
        }

        private void Initialize()
        {
            lock (_lockObject)
            {
                try
                {
                    // Check if data already exists
                    bool hasData = CheckIfDataExists();

                    // Load existing data
                    LoadNewsArticles();
                    LoadUserArticles();

                    // Only add mock data if no data exists
                    if (!hasData)
                    {
                        hardCodedNewsArticles();
                    }

                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error initializing NewsRepository: {ex.Message}");
                    throw;
                }
            }
        }

        private bool CheckIfDataExists()
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                using (var command = new SqlCommand("SELECT COUNT(*) FROM NEWS_ARTICLE", connection))
                {
                    command.CommandTimeout = 30;
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public void EnsureUserExists(string cnp, string name, string description, bool isAdmin, bool isHidden, string profilePicture, int gemBalance = 1000)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                string checkQuery = "IF NOT EXISTS (SELECT 1 FROM [USER] WHERE CNP = @CNP) " +
                                    "INSERT INTO [USER] (CNP, NAME, DESCRIPTION, IS_HIDDEN, IS_ADMIN, PROFILE_PICTURE, GEM_BALANCE) " +
                                    "VALUES (@CNP, @Name, @Description, @IsHidden, @IsAdmin, @ProfilePicture, @GemBalance)";

                using (var command = new SqlCommand(checkQuery, connection))
                {
                    command.Parameters.AddWithValue("@CNP", cnp);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@IsHidden", isHidden ? 1 : 0);
                    command.Parameters.AddWithValue("@IsAdmin", isAdmin ? 1 : 0);
                    command.Parameters.AddWithValue("@ProfilePicture", profilePicture);
                    command.Parameters.AddWithValue("@GemBalance", gemBalance);
                    command.ExecuteNonQuery();
                }
            }
        }

        #region News Articles

        public void LoadNewsArticles()
        {
            newsArticles.Clear();
            using (var connection = _databaseHelper.GetConnection())
            {
                using (var command = new SqlCommand("SELECT * FROM NEWS_ARTICLE", connection))
                {
                    command.CommandTimeout = 30;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var article = new NewsArticle
                            {
                                ArticleId = reader.GetString(0),
                                Title = reader.GetString(1),
                                Summary = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Content = reader.GetString(3),
                                Source = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                PublishedDate = reader.GetString(5),
                                IsRead = reader.GetBoolean(6),
                                IsWatchlistRelated = reader.GetBoolean(7),
                                Category = reader.GetString(8),
                                RelatedStocks = GetRelatedStocksForArticle(reader.GetString(0))
                            };
                            newsArticles.Add(article);
                        }
                    }
                }
            }
        }

        public List<string> GetRelatedStocksForArticle(string articleId)
        {
            var relatedStocks = new List<string>();

            try
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    using (var command = new SqlCommand("SELECT STOCK_NAME FROM RELATED_STOCKS WHERE ARTICLE_ID = @ArticleId", connection))
                    {
                        command.CommandTimeout = 30;
                        command.Parameters.AddWithValue("@ArticleId", articleId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                relatedStocks.Add(reader.GetString(0));
                            }
                        }
                    }

                    // no stocks in database, check our mock data
                    if (relatedStocks.Count == 0)
                    {
                        var mockArticle = newsArticles.FirstOrDefault(a => a.ArticleId == articleId);
                        if (mockArticle != null && mockArticle.RelatedStocks != null && mockArticle.RelatedStocks.Count > 0)
                        {
                            relatedStocks.AddRange(mockArticle.RelatedStocks);
                            System.Diagnostics.Debug.WriteLine($"Found {relatedStocks.Count} related stocks in mock data for article {articleId}");

                            try
                            {
                                AddRelatedStocksForArticle(articleId, relatedStocks, connection);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error adding related stocks to database: {ex.Message}");
                            }
                        }

                        // user articles too
                        var userArticle = userArticles.FirstOrDefault(a => a.ArticleId == articleId);
                        if (userArticle != null && userArticle.RelatedStocks != null && userArticle.RelatedStocks.Count > 0)
                        {
                            relatedStocks.AddRange(userArticle.RelatedStocks.Where(s => !relatedStocks.Contains(s)));
                            System.Diagnostics.Debug.WriteLine($"Found {userArticle.RelatedStocks.Count} related stocks in user article for {articleId}");

                            try
                            {
                                AddRelatedStocksForArticle(articleId, relatedStocks, connection);
                                System.Diagnostics.Debug.WriteLine($"Added related stocks to database for user article {articleId}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error adding related stocks to database: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting related stocks: {ex.Message}");

                // fallback to in-memory data
                var mockArticle = newsArticles.FirstOrDefault(a => a.ArticleId == articleId);
                if (mockArticle != null && mockArticle.RelatedStocks != null)
                {
                    relatedStocks.AddRange(mockArticle.RelatedStocks);
                    System.Diagnostics.Debug.WriteLine($"Fallback: Found {relatedStocks.Count} related stocks in mock data");
                }

                var userArticle = userArticles.FirstOrDefault(a => a.ArticleId == articleId);
                if (userArticle != null && userArticle.RelatedStocks != null)
                {
                    relatedStocks.AddRange(userArticle.RelatedStocks.Where(s => !relatedStocks.Contains(s)));
                    System.Diagnostics.Debug.WriteLine($"Fallback: Found {userArticle.RelatedStocks.Count} related stocks in user article");
                }
            }

            return relatedStocks;
        }

        public void AddRelatedStocksForArticle(string articleId, List<string> stockNames, SqlConnection connection = null, SqlTransaction transaction = null)
        {
            if (stockNames == null || stockNames.Count == 0)
                return;

            bool ownConnection = false;
            try
            {
                // no connection was provided, create our own (idk how this works but it does, no need to change it yes)
                if (connection == null)
                {
                    connection = _databaseHelper.GetConnection();
                    ownConnection = true;
                    // new transaction if one wasn't provided
                    if (transaction == null)
                    {
                        transaction = connection.BeginTransaction();
                    }
                }

                foreach (var inputStockName in stockNames)
                {
                    // check if the stock exists in the STOCK table by name
                    string actualStockName = inputStockName;
                    bool stockExists = false;

                    using (var stockCheckCommand = new SqlCommand("SELECT STOCK_NAME FROM STOCK WHERE STOCK_NAME = @StockName", connection, transaction))
                    {
                        stockCheckCommand.CommandTimeout = 30;
                        stockCheckCommand.Parameters.AddWithValue("@StockName", inputStockName);
                        var result = stockCheckCommand.ExecuteScalar();
                        stockExists = result != null;
                    }

                    // not found by name, try to find by symbol
                    if (!stockExists)
                    {
                        using (var symbolCheckCommand = new SqlCommand("SELECT STOCK_NAME FROM STOCK WHERE STOCK_SYMBOL = @StockSymbol", connection, transaction))
                        {
                            symbolCheckCommand.CommandTimeout = 30;
                            symbolCheckCommand.Parameters.AddWithValue("@StockSymbol", inputStockName);
                            var result = symbolCheckCommand.ExecuteScalar();

                            if (result != null)
                            {
                                stockExists = true;
                                actualStockName = result.ToString(); // Use the actual stock name from the database
                            }
                        }
                    }

                    // stock doesn't exist at all, create it with default values
                    if (!stockExists)
                    {
                        string stockSymbol = inputStockName.Length <= 5 ? inputStockName.ToUpper() : inputStockName.Substring(0, 5).ToUpper();
                        string authorCnp = "1234567890123"; // Default author CNP

                        using (var createStockCommand = new SqlCommand("INSERT INTO STOCK (STOCK_NAME, STOCK_SYMBOL, AUTHOR_CNP) VALUES (@StockName, @StockSymbol, @AuthorCNP)", connection, transaction))
                        {
                            createStockCommand.CommandTimeout = 30;
                            createStockCommand.Parameters.AddWithValue("@StockName", inputStockName);
                            createStockCommand.Parameters.AddWithValue("@StockSymbol", stockSymbol);
                            createStockCommand.Parameters.AddWithValue("@AuthorCNP", authorCnp);
                            createStockCommand.ExecuteNonQuery();
                        }

                        // add an initial price in STOCK_VALUE
                        using (var valueCommand = new SqlCommand("INSERT INTO STOCK_VALUE (STOCK_NAME, PRICE) VALUES (@StockName, @Price)", connection, transaction))
                        {
                            valueCommand.CommandTimeout = 30;
                            valueCommand.Parameters.AddWithValue("@StockName", inputStockName);
                            valueCommand.Parameters.AddWithValue("@Price", 100); // Default initial price
                            valueCommand.ExecuteNonQuery();
                        }

                        actualStockName = inputStockName;
                    }

                    // check if the related stock entry already exists
                    bool relatedStockExists = false;
                    using (var checkCommand = new SqlCommand("SELECT COUNT(*) FROM RELATED_STOCKS WHERE STOCK_NAME = @StockName AND ARTICLE_ID = @ArticleId", connection, transaction))
                    {
                        checkCommand.CommandTimeout = 30;
                        checkCommand.Parameters.AddWithValue("@StockName", actualStockName);
                        checkCommand.Parameters.AddWithValue("@ArticleId", articleId);
                        relatedStockExists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                    }

                    // the related stock entry doesn't exist, create it
                    if (!relatedStockExists)
                    {
                        using (var command = new SqlCommand("INSERT INTO RELATED_STOCKS (STOCK_NAME, ARTICLE_ID) VALUES (@StockName, @ArticleId)", connection, transaction))
                        {
                            command.CommandTimeout = 30;
                            command.Parameters.AddWithValue("@StockName", actualStockName);
                            command.Parameters.AddWithValue("@ArticleId", articleId);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                // commit if we created the transaction
                if (ownConnection && transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                // rollback if we created the transaction
                if (ownConnection && transaction != null)
                {
                    try { transaction.Rollback(); } catch { /* Ignore rollback errors */ }
                }
                System.Diagnostics.Debug.WriteLine($"Error adding related stocks: {ex.Message}");
                throw;
            }
            finally
            {
                // dispose if we created the connection
                if (ownConnection && connection != null)
                {
                    connection.Close();
                }
            }
        }

        public void AddNewsArticle(NewsArticle newsArticle)
        {
            lock (_lockObject)
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    bool exists = false;
                    using (var checkCommand = new SqlCommand("SELECT COUNT(*) FROM NEWS_ARTICLE WHERE ARTICLE_ID = @ArticleId", connection))
                    {
                        checkCommand.CommandTimeout = 30;
                        checkCommand.Parameters.AddWithValue("@ArticleId", newsArticle.ArticleId);
                        exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                    }

                    if (exists)
                    {
                        UpdateNewsArticle(newsArticle);
                        return;
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string query = "INSERT INTO NEWS_ARTICLE (ARTICLE_ID, TITLE, SUMMARY, CONTENT, SOURCE, PUBLISH_DATE, IS_READ, IS_WATCHLIST_RELATED, CATEGORY) VALUES (@ArticleId, @Title, @Summary, @Content, @Source, @PublishedDate, @IsRead, @IsWatchlistRelated, @Category)";
                            using (var command = new SqlCommand(query, connection, transaction))
                            {
                                command.CommandTimeout = 30;
                                command.Parameters.AddWithValue("@ArticleId", newsArticle.ArticleId);
                                command.Parameters.AddWithValue("@Title", newsArticle.Title);
                                command.Parameters.AddWithValue("@Summary", newsArticle.Summary ?? "");
                                command.Parameters.AddWithValue("@Content", newsArticle.Content);
                                command.Parameters.AddWithValue("@Source", newsArticle.Source ?? "");
                                command.Parameters.AddWithValue("@PublishedDate", newsArticle.PublishedDate);
                                command.Parameters.AddWithValue("@IsRead", newsArticle.IsRead);
                                command.Parameters.AddWithValue("@IsWatchlistRelated", newsArticle.IsWatchlistRelated);
                                command.Parameters.AddWithValue("@Category", newsArticle.Category ?? "");
                                command.ExecuteNonQuery();
                            }

                            if (newsArticle.RelatedStocks != null && newsArticle.RelatedStocks.Count > 0)
                            {
                                AddRelatedStocksForArticle(newsArticle.ArticleId, newsArticle.RelatedStocks, connection, transaction);
                            }

                            transaction.Commit();

                            // to in-memory collection if not already there
                            if (!newsArticles.Any(a => a.ArticleId == newsArticle.ArticleId))
                            {
                                newsArticles.Add(newsArticle);
                            }
                        }
                        catch (Exception ex)
                        {
                            try { transaction.Rollback(); } catch { /* Ignore rollback errors */ }
                            System.Diagnostics.Debug.WriteLine($"Error adding news article: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
        }

        public void UpdateNewsArticle(NewsArticle newsArticle)
        {
            lock (_lockObject)
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string query = "UPDATE NEWS_ARTICLE SET TITLE = @Title, SUMMARY = @Summary, CONTENT = @Content, SOURCE = @Source, PUBLISH_DATE = @PublishedDate, IS_READ = @IsRead, IS_WATCHLIST_RELATED = @IsWatchlistRelated, CATEGORY = @Category WHERE ARTICLE_ID = @ArticleId";
                            using (var command = new SqlCommand(query, connection, transaction))
                            {
                                command.CommandTimeout = 30;
                                command.Parameters.AddWithValue("@ArticleId", newsArticle.ArticleId);
                                command.Parameters.AddWithValue("@Title", newsArticle.Title);
                                command.Parameters.AddWithValue("@Summary", newsArticle.Summary ?? "");
                                command.Parameters.AddWithValue("@Content", newsArticle.Content);
                                command.Parameters.AddWithValue("@Source", newsArticle.Source ?? "");
                                command.Parameters.AddWithValue("@PublishedDate", newsArticle.PublishedDate);
                                command.Parameters.AddWithValue("@IsRead", newsArticle.IsRead);
                                command.Parameters.AddWithValue("@IsWatchlistRelated", newsArticle.IsWatchlistRelated);
                                command.Parameters.AddWithValue("@Category", newsArticle.Category ?? "");
                                command.ExecuteNonQuery();
                            }

                            using (var command = new SqlCommand("DELETE FROM RELATED_STOCKS WHERE ARTICLE_ID = @ArticleId", connection, transaction))
                            {
                                command.CommandTimeout = 30;
                                command.Parameters.AddWithValue("@ArticleId", newsArticle.ArticleId);
                                command.ExecuteNonQuery();
                            }

                            if (newsArticle.RelatedStocks != null && newsArticle.RelatedStocks.Count > 0)
                            {
                                AddRelatedStocksForArticle(newsArticle.ArticleId, newsArticle.RelatedStocks, connection, transaction);
                            }

                            transaction.Commit();

                            var existingArticle = newsArticles.FirstOrDefault(a => a.ArticleId == newsArticle.ArticleId);
                            if (existingArticle != null)
                            {
                                int index = newsArticles.IndexOf(existingArticle);
                                newsArticles[index] = newsArticle;
                            }
                            else
                            {
                                newsArticles.Add(newsArticle);
                            }
                        }
                        catch (Exception ex)
                        {
                            try { transaction.Rollback(); } catch { /* Ignore rollback errors */ }
                            System.Diagnostics.Debug.WriteLine($"Error updating news article: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
        }

        public void DeleteNewsArticle(string articleId)
        {
            lock (_lockObject)
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SqlCommand("DELETE FROM RELATED_STOCKS WHERE ARTICLE_ID = @ArticleId", connection, transaction))
                            {
                                command.CommandTimeout = 30;
                                command.Parameters.AddWithValue("@ArticleId", articleId);
                                command.ExecuteNonQuery();
                            }

                            using (var command = new SqlCommand("DELETE FROM NEWS_ARTICLE WHERE ARTICLE_ID = @ArticleId", connection, transaction))
                            {
                                command.CommandTimeout = 30;
                                command.Parameters.AddWithValue("@ArticleId", articleId);
                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            var articleToRemove = newsArticles.FirstOrDefault(a => a.ArticleId == articleId);
                            if (articleToRemove != null)
                            {
                                newsArticles.Remove(articleToRemove);
                            }
                        }
                        catch (Exception ex)
                        {
                            try { transaction.Rollback(); } catch { /* Ignore rollback errors */ }
                            System.Diagnostics.Debug.WriteLine($"Error deleting news article: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
        }

        public NewsArticle GetNewsArticleById(string articleId)
        {
            var article = newsArticles.FirstOrDefault(a => a.ArticleId == articleId);

            if (article != null && (article.RelatedStocks == null || !article.RelatedStocks.Any()))
            {
                // related stocks are loaded
                article.RelatedStocks = GetRelatedStocksForArticle(articleId);
                System.Diagnostics.Debug.WriteLine($"GetNewsArticleById: Loaded {article.RelatedStocks?.Count ?? 0} related stocks for article {articleId}");
            }

            return article;
        }

        public List<NewsArticle> GetAllNewsArticles()
        {
            return new List<NewsArticle>(newsArticles);
        }

        public List<NewsArticle> GetNewsArticlesByStock(string stockName)
        {
            return newsArticles.Where(a => a.RelatedStocks.Contains(stockName)).ToList();
        }

        public List<NewsArticle> GetNewsArticlesByCategory(string category)
        {
            return newsArticles.Where(a => a.Category == category).ToList();
        }

        public void MarkArticleAsRead(string articleId)
        {
            var article = GetNewsArticleById(articleId);
            if (article != null)
            {
                article.IsRead = true;
                UpdateNewsArticle(article);
            }
        }

        #endregion

        #region User Articles

        public void LoadUserArticles()
        {
            userArticles.Clear();
            using (var connection = _databaseHelper.GetConnection())
            {
                using (var command = new SqlCommand("SELECT * FROM USER_ARTICLE", connection))
                {
                    command.CommandTimeout = 30;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var article = new UserArticle
                            {
                                ArticleId = reader.GetString(0),
                                Title = reader.GetString(1),
                                Summary = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Content = reader.GetString(3),

                                Author = reader.GetString(4),
                                SubmissionDate = DateTime.Parse(reader.GetString(5)),
                                Status = reader.GetString(6),
                                Topic = reader.GetString(7),
                                RelatedStocks = GetRelatedStocksForArticle(reader.GetString(0))
                            };
                            userArticles.Add(article);
                        }
                    }
                }
            }
        }

        public void AddUserArticle(UserArticle userArticle)
        {
            lock (_lockObject)
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    bool exists = false;
                    using (var checkCommand = new SqlCommand("SELECT COUNT(*) FROM USER_ARTICLE WHERE ARTICLE_ID = @ArticleId", connection))
                    {
                        checkCommand.CommandTimeout = 30;
                        checkCommand.Parameters.AddWithValue("@ArticleId", userArticle.ArticleId);
                        exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                    }

                    if (exists)
                    {
                        UpdateUserArticle(userArticle);
                        return;
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // First, add the user article
                            using (var command = new SqlCommand("INSERT INTO USER_ARTICLE (ARTICLE_ID, TITLE, SUMMARY, CONTENT, AUTHOR_CNP, SUBMISSION_DATE, STATUS, TOPIC) VALUES (@ArticleId, @Title, @Summary, @Content, @AuthorCNP, @SubmissionDate, @Status, @Topic)", connection, transaction))
                            {
                                command.CommandTimeout = 30;
                                command.Parameters.AddWithValue("@ArticleId", userArticle.ArticleId);
                                command.Parameters.AddWithValue("@Title", userArticle.Title);
                                command.Parameters.AddWithValue("@Summary", userArticle.Summary ?? "");
                                command.Parameters.AddWithValue("@Content", userArticle.Content);
                                command.Parameters.AddWithValue("@AuthorCNP", userArticle.Author);
                                command.Parameters.AddWithValue("@SubmissionDate", userArticle.SubmissionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                command.Parameters.AddWithValue("@Status", userArticle.Status);
                                command.Parameters.AddWithValue("@Topic", userArticle.Topic);
                                command.ExecuteNonQuery();
                            }

                            // IMPORTANT: Always add an entry to the NEWS_ARTICLE table regardless of status
                            // This ensures we can add related stocks that reference this article
                            using (var command = new SqlCommand("INSERT INTO NEWS_ARTICLE (ARTICLE_ID, TITLE, SUMMARY, CONTENT, SOURCE, PUBLISH_DATE, IS_READ, IS_WATCHLIST_RELATED, CATEGORY) VALUES (@ArticleId, @Title, @Summary, @Content, @Source, @PublishedDate, @IsRead, @IsWatchlistRelated, @Category)", connection, transaction))
                            {
                                command.CommandTimeout = 30;
                                command.Parameters.AddWithValue("@ArticleId", userArticle.ArticleId);
                                command.Parameters.AddWithValue("@Title", userArticle.Title);
                                command.Parameters.AddWithValue("@Summary", userArticle.Summary ?? "");
                                command.Parameters.AddWithValue("@Content", userArticle.Content);
                                command.Parameters.AddWithValue("@Source", $"User: {userArticle.Author}");
                                command.Parameters.AddWithValue("@PublishedDate", userArticle.SubmissionDate.ToString("MMMM dd, yyyy"));
                                command.Parameters.AddWithValue("@IsRead", false);
                                command.Parameters.AddWithValue("@IsWatchlistRelated", false);
                                command.Parameters.AddWithValue("@Category", userArticle.Topic ?? "");
                                command.ExecuteNonQuery();
                            }

                            // Now add related stocks if there are any
                            if (userArticle.RelatedStocks != null && userArticle.RelatedStocks.Count > 0)
                            {
                                AddRelatedStocksForArticle(userArticle.ArticleId, userArticle.RelatedStocks, connection, transaction);
                            }

                            transaction.Commit();

                            // Add to in-memory collection if not already there
                            if (!userArticles.Any(a => a.ArticleId == userArticle.ArticleId))
                            {
                                userArticles.Add(userArticle);
                            }
                        }
                        catch (Exception ex)
                        {
                            try { transaction.Rollback(); } catch { /* Ignore rollback errors */ }
                            System.Diagnostics.Debug.WriteLine($"Error adding user article: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
        }

        public void UpdateUserArticle(UserArticle userArticle)
        {
            lock (_lockObject)
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string query = "UPDATE USER_ARTICLE SET TITLE = @Title, SUMMARY = @Summary, CONTENT = @Content, AUTHOR_CNP = @AuthorCNP, SUBMISSION_DATE = @SubmissionDate, STATUS = @Status, TOPIC = @Topic WHERE ARTICLE_ID = @ArticleId";
                            using (var command = new SqlCommand(query, connection, transaction))
                            {
                                command.CommandTimeout = 30;
                                command.Parameters.AddWithValue("@ArticleId", userArticle.ArticleId);
                                command.Parameters.AddWithValue("@Title", userArticle.Title);
                                command.Parameters.AddWithValue("@Summary", userArticle.Summary ?? "");
                                command.Parameters.AddWithValue("@Content", userArticle.Content);
                                command.Parameters.AddWithValue("@AuthorCNP", userArticle.Author);
                                command.Parameters.AddWithValue("@SubmissionDate", userArticle.SubmissionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                command.Parameters.AddWithValue("@Status", userArticle.Status);
                                command.Parameters.AddWithValue("@Topic", userArticle.Topic);
                                command.ExecuteNonQuery();
                            }

                            using (var command = new SqlCommand("DELETE FROM RELATED_STOCKS WHERE ARTICLE_ID = @ArticleId", connection, transaction))
                            {
                                command.CommandTimeout = 30;
                                command.Parameters.AddWithValue("@ArticleId", userArticle.ArticleId);
                                command.ExecuteNonQuery();
                            }

                            if (userArticle.RelatedStocks != null && userArticle.RelatedStocks.Count > 0)
                            {
                                AddRelatedStocksForArticle(userArticle.ArticleId, userArticle.RelatedStocks, connection, transaction);
                            }

                            transaction.Commit();

                            var existingArticle = userArticles.FirstOrDefault(a => a.ArticleId == userArticle.ArticleId);
                            if (existingArticle != null)
                            {
                                int index = userArticles.IndexOf(existingArticle);
                                userArticles[index] = userArticle;
                            }
                            else
                            {
                                userArticles.Add(userArticle);
                            }
                        }
                        catch (Exception ex)
                        {
                            try { transaction.Rollback(); } catch { /* Ignore rollback errors */ }
                            System.Diagnostics.Debug.WriteLine($"Error updating user article: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
        }

        public void DeleteUserArticle(string articleId)
        {
            lock (_lockObject)
            {
                using (var connection = _databaseHelper.GetConnection())
                {
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var command = new SqlCommand("DELETE FROM RELATED_STOCKS WHERE ARTICLE_ID = @ArticleId", connection, transaction))
                            {
                                command.CommandTimeout = 30;
                                command.Parameters.AddWithValue("@ArticleId", articleId);
                                command.ExecuteNonQuery();
                            }

                            using (var command = new SqlCommand("DELETE FROM USER_ARTICLE WHERE ARTICLE_ID = @ArticleId", connection, transaction))
                            {
                                command.CommandTimeout = 30;
                                command.Parameters.AddWithValue("@ArticleId", articleId);
                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            var articleToRemove = userArticles.FirstOrDefault(a => a.ArticleId == articleId);
                            if (articleToRemove != null)
                            {
                                userArticles.Remove(articleToRemove);
                            }
                        }
                        catch (Exception ex)
                        {
                            try { transaction.Rollback(); } catch { /* Ignore rollback errors */ }
                            System.Diagnostics.Debug.WriteLine($"Error deleting user article: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
        }

        public UserArticle GetUserArticleById(string articleId)
        {
            return userArticles.FirstOrDefault(a => a.ArticleId == articleId);
        }

        public List<UserArticle> GetAllUserArticles()
        {
            return new List<UserArticle>(userArticles);
        }

        public List<UserArticle> GetUserArticlesByStatus(string status)
        {
            return userArticles.Where(a => a.Status == status).ToList();
        }

        public List<UserArticle> GetUserArticlesByTopic(string topic)
        {
            return userArticles.Where(a => a.Topic == topic).ToList();
        }

        public List<UserArticle> GetUserArticlesByStatusAndTopic(string status, string topic)
        {
            return userArticles.Where(a => a.Status == status && a.Topic == topic).ToList();
        }

        public void ApproveUserArticle(string articleId)
        {
            var article = GetUserArticleById(articleId);
            if (article != null)
            {
                article.Status = "Approved";
                UpdateUserArticle(article);

                // Create a news article from the approved user article
                var newsArticle = new NewsArticle
                {
                    ArticleId = article.ArticleId,
                    Title = article.Title,
                    Summary = article.Summary ?? "",
                    Content = article.Content,
                    Source = $"User: {article.Author}",
                    PublishedDate = article.SubmissionDate.ToString("MMMM dd, yyyy"),
                    IsRead = false,
                    IsWatchlistRelated = false,
                    Category = article.Topic,
                    RelatedStocks = article.RelatedStocks
                };

                // Check if the news article already exists
                var existingNewsArticle = GetNewsArticleById(article.ArticleId);
                if (existingNewsArticle == null)
                {
                    AddNewsArticle(newsArticle);
                }
                else
                {
                    UpdateNewsArticle(newsArticle);
                }
            }
        }

        public void RejectUserArticle(string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
                throw new ArgumentNullException(nameof(articleId));

            var article = GetUserArticleById(articleId) 
                ?? throw new InvalidOperationException($"User article with ID {articleId} not found");
            
            article.Status = "Rejected";
            UpdateUserArticle(article);

            // Remove from news articles if it exists
            var existingNewsArticle = GetNewsArticleById(article.ArticleId);
            if (existingNewsArticle != null)
            {
                DeleteNewsArticle(article.ArticleId ?? throw new InvalidOperationException("Article ID cannot be null"));
            }
        }

        #endregion

        #region Mock Data

        private List<UserArticle> GetMockUserArticles()
        {
            return new List<UserArticle>
    {
        new UserArticle
        {
            ArticleId = "ua1",
            Title = "Analysis of Recent Market Trends",
            Summary = "A detailed analysis of recent market trends and their implications for investors.",
            Content = "The market has shown significant volatility in recent weeks...",
            Author = "1234567890123",
            SubmissionDate = DateTime.Now.AddDays(-5),
            Status = "Pending",
            Topic = "Market Analysis",
            RelatedStocks = new List<string> { "Cesla" }
        },
        new UserArticle
        {
            ArticleId = "ua2",
            Title = "The Future of Electric Vehicles",
            Summary = "An exploration of the electric vehicle industry and its growth prospects.",
            Content = "Electric vehicles (EVs) are rapidly transforming the automotive industry...",
            Author = "1234567890124",
            SubmissionDate = DateTime.Now.AddDays(-3),
            Status = "Approved",
            Topic = "Company News",
            RelatedStocks = new List<string> { "Tesla" }
        },
        new UserArticle
        {
            ArticleId = "ua3",
            Title = "Cryptocurrency Market Update",
            Summary = "A review of recent developments in the cryptocurrency market.",
            Content = "The cryptocurrency market continues to evolve rapidly...",
            Author = "1234567890125",
            SubmissionDate = DateTime.Now.AddDays(-2),
            Status = "Rejected",
            Topic = "Market Analysis",
            RelatedStocks = new List<string> { "Besla" }
        },
        new UserArticle
        {
            ArticleId = "ua4",
            Title = "The Impact of Artificial Intelligence on Financial Services",
            Summary = "An analysis of how AI is transforming the financial services industry.",
            Content = "Artificial intelligence (AI) is revolutionizing the financial services industry...",
            Author = "1234567890123",
            SubmissionDate = DateTime.Now.AddDays(-1),
            Status = "Pending",
            Topic = "Functionality News",
            RelatedStocks = new List<string> { "Besla", "Tesla" }
        }
    };
        }

        private List<NewsArticle> GetMockArticles()
        {
            var approvedUserArticles = userArticles
                .Where(ua => ua.Status == "Approved")
                .Select(ua => new NewsArticle
                {
                    ArticleId = ua.ArticleId,
                    Title = ua.Title,
                    Summary = ua.Summary,
                    Content = ua.Content,
                    Source = $"User: {ua.Author}",
                    PublishedDate = ua.SubmissionDate.ToString("MMMM dd, yyyy"),
                    IsRead = false,
                    IsWatchlistRelated = false,
                    Category = ua.Topic,
                    RelatedStocks = ua.RelatedStocks
                })
                .ToList();

            var mockArticles = new List<NewsArticle>
            {
                new NewsArticle
                {
                    ArticleId = "1",
                    Title = "Market Reaches All-Time High",
                    Summary = "The stock market reached an all-time high today as tech stocks surged.",
                    Content = "The stock market reached an all-time high today as tech stocks surged. Investors are optimistic about the future of technology companies as they continue to innovate and grow. The S&P 500 index rose 1.2% to close at a record high, while the Nasdaq Composite index gained 1.5%. Leading the gains were shares of major tech companies such as Apple, Microsoft, and Amazon, which all rose more than 2%. Analysts attribute the rally to strong earnings reports and positive economic data. \"The market is showing resilience despite concerns about inflation and interest rates,\" said John Smith, chief market strategist at XYZ Investment Firm. \"Tech companies are demonstrating their ability to adapt and thrive in the current economic environment.\" However, some experts caution that the market may be overvalued and due for a correction. \"We're seeing signs of froth in certain sectors,\" warned Jane Doe, portfolio manager at ABC Asset Management. \"Investors should be selective and focus on companies with strong fundamentals.\" The rally comes amid a backdrop of improving economic conditions, with unemployment falling and consumer spending rising. The Federal Reserve has signaled that it will maintain its accommodative monetary policy for the foreseeable future, providing further support for the market. Looking ahead, investors will be closely watching upcoming earnings reports and economic data for signs of continued growth or potential headwinds.",
                    Source = "Financial Times",
                    PublishedDate = "April 15, 2023",
                    IsRead = false,
                    IsWatchlistRelated = true,
                    Category = "Market Analysis",
                    RelatedStocks = new List<string> { "Besla" }
                },
                new NewsArticle
                {
                    ArticleId = "2",
                    Title = "Tech Company Announces New Product Line",
                    Summary = "A major tech company has announced a new product line that is expected to revolutionize the industry.",
                    Content = "A major tech company has announced a new product line that is expected to revolutionize the industry. The company unveiled its latest innovations at a highly anticipated event yesterday, showcasing cutting-edge technology that promises to transform how consumers interact with their devices. The new products include a range of smart home devices, wearable technology, and advanced computing solutions. Industry analysts are bullish on the company's prospects following the announcement. \"This represents a significant leap forward in terms of both hardware and software integration,\" said tech analyst Sarah Johnson. \"The company has once again demonstrated its ability to innovate and stay ahead of the competition.\" The stock price of the company surged following the announcement, rising 5% in after-hours trading. Investors are particularly excited about the potential for new revenue streams from the expanded product ecosystem. Pre-orders for the new devices will begin next week, with shipping expected to start in early June. The company has also announced partnerships with several major retailers to ensure wide availability of the products at launch. Competitors are expected to respond with their own product announcements in the coming months, potentially setting up a fierce battle for market share in the second half of the year. \"This is just the beginning of what promises to be an exciting period of innovation in the tech sector,\" said industry consultant Michael Brown. \"Consumers will ultimately benefit from the increased competition and rapid pace of technological advancement.\"",
                    Source = "Tech Insider",
                    PublishedDate = "April 14, 2023",
                    IsRead = true,
                    IsWatchlistRelated = false,
                    Category = "Company News",
                    RelatedStocks = new List<string> { "Besla", "Tesla" }
                },
                new NewsArticle
                {
                    ArticleId = "3",
                    Title = "Economic Growth Exceeds Expectations",
                    Summary = "The economy grew faster than expected in the first quarter, according to new data.",
                    Content = "The economy grew faster than expected in the first quarter, according to new data released by the Commerce Department today. Gross domestic product (GDP) increased at an annual rate of 3.2%, surpassing economists' forecasts of 2.5% growth. The stronger-than-expected growth was driven by robust consumer spending, business investment, and exports. Consumer spending, which accounts for more than two-thirds of economic activity, rose 3.5% as households increased purchases of durable goods and services. Business investment grew 5.2%, reflecting increased spending on equipment and intellectual property products. Exports surged 7.8%, outpacing imports which grew at a more modest 4.5%. The positive economic data has implications for monetary policy, as the Federal Reserve may need to reassess its interest rate projections. \"The strong GDP report suggests that the economy is on solid footing, which could lead the Fed to maintain higher interest rates for longer than previously anticipated,\" said economist Robert Johnson. Inflation, as measured by the personal consumption expenditures (PCE) price index, increased at a 2.7% rate in the first quarter, slightly above the Fed's 2% target. Core PCE, which excludes food and energy prices, rose 2.5%. The labor market remains tight, with the unemployment rate holding steady at 3.6%. Wage growth has moderated but remains above pre-pandemic levels. Looking ahead, economists expect growth to slow in the second quarter as the effects of higher interest rates continue to work their way through the economy. However, the risk of a recession in the near term has diminished given the strength of recent economic data.",
                    Source = "Economic Journal",
                    PublishedDate = "April 13, 2023",
                    IsRead = false,
                    IsWatchlistRelated = false,
                    Category = "Economic News",
                    RelatedStocks = new List<string> { "Besla", "Cesla" }
                },
                new NewsArticle
                {
                    ArticleId = "4",
                    Title = "New Trading Feature Added to Platform",
                    Summary = "Our platform has added a new trading feature that allows for more efficient order execution.",
                    Content = "Our platform has added a new trading feature that allows for more efficient order execution. The new feature, called Smart Order Routing (SOR), automatically directs orders to the exchange or market center offering the best price at the time of the order. This ensures that traders get the best possible execution for their trades. SOR works by scanning multiple exchanges and market centers in real-time to find the best available price for a given security. It then routes the order to that venue for execution. If the order cannot be fully executed at a single venue, SOR will split the order and route it to multiple venues to achieve the best overall execution. \"This new feature represents a significant enhancement to our trading platform,\" said the company's Chief Technology Officer. \"It demonstrates our commitment to providing our users with the most advanced trading tools available.\" In addition to improving execution quality, SOR can also help reduce trading costs by minimizing market impact and slippage. It is particularly beneficial for large orders that might otherwise move the market if executed at a single venue. The feature is now available to all users of the platform at no additional cost. To access SOR, users simply need to select the \"Smart Routing\" option when placing an order. The company plans to release additional trading enhancements in the coming months, including advanced order types and improved risk management tools. These updates are part of a broader initiative to modernize the platform and provide users with a more comprehensive trading experience. \"We're constantly looking for ways to improve our platform and give our users an edge in the market,\" said the company's CEO. \"Smart Order Routing is just the beginning of what we have planned for this year.\"",
                    Source = "Platform News",
                    PublishedDate = "April 12, 2023",
                    IsRead = false,
                    IsWatchlistRelated = false,
                    Category = "Functionality News",
                    RelatedStocks = new List<string>()
                },
                new NewsArticle
                {
                    ArticleId = "5",
                    Title = "Pharmaceutical Company Receives FDA Approval",
                    Summary = "A major pharmaceutical company has received FDA approval for its new drug.",
                    Content = "A major pharmaceutical company has received FDA approval for its new drug targeting a rare genetic disorder. The approval comes after extensive clinical trials demonstrated the drug's safety and efficacy in treating the condition, which affects approximately 1 in 50,000 people worldwide. The drug, which will be marketed under the name Genetix, works by targeting a specific protein involved in the disease pathway. In clinical trials, patients who received the drug showed significant improvement in symptoms compared to those who received a placebo. \"This approval represents a major milestone for patients suffering from this debilitating disorder,\" said the company's Chief Medical Officer. \"Until now, treatment options have been limited to managing symptoms rather than addressing the underlying cause of the disease.\" The company expects to launch the drug in the U.S. market within the next three months, with international launches to follow pending regulatory approvals in other countries. Analysts estimate that the drug could generate annual sales of $1-2 billion at peak. The stock price of the pharmaceutical company rose 8% following the announcement, reflecting investor optimism about the drug's commercial potential. Industry experts note that the approval also validates the company's research and development strategy, which has focused on rare diseases with high unmet medical needs. \"This approval strengthens the company's position in the rare disease space and demonstrates its ability to successfully navigate the regulatory approval process,\" said healthcare analyst David Wilson. The company has already begun work on expanding the drug's indications to related disorders and is conducting early-stage research on next-generation treatments based on similar mechanisms of action.",
                    Source = "Health News",
                    PublishedDate = "April 11, 2023",
                    IsRead = true,
                    IsWatchlistRelated = true,
                    Category = "Company News",
                    RelatedStocks = new List<string> { "Tesla" }
                }
            };

            // Add approved user articles to the list
            mockArticles.AddRange(approvedUserArticles);

            return mockArticles;
        }

        public void hardCodedNewsArticles()
        {
            lock (_lockObject)
            {
                try
                {
                    using (var connection = _databaseHelper.GetConnection())
                    {
                        List<UserArticle> mockUserArticles = GetMockUserArticles();
                        List<NewsArticle> mockNewsArticles = GetMockArticles();

                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                // gt all existing stocks
                                List<string> existingStocks = new List<string>();
                                using (var command = new SqlCommand("SELECT STOCK_NAME FROM STOCK", connection, transaction))
                                {
                                    command.CommandTimeout = 30;
                                    using (var reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            existingStocks.Add(reader["STOCK_NAME"].ToString());
                                        }
                                    }
                                }

                                // unique stock names from articles
                                HashSet<string> allStocksInArticles = new HashSet<string>();
                                foreach (var article in mockNewsArticles)
                                {
                                    if (article.RelatedStocks != null)
                                    {
                                        foreach (var stock in article.RelatedStocks)
                                        {
                                            allStocksInArticles.Add(stock);
                                        }
                                    }
                                }

                                foreach (var article in mockUserArticles)
                                {
                                    if (article.RelatedStocks != null)
                                    {
                                        foreach (var stock in article.RelatedStocks)
                                        {
                                            allStocksInArticles.Add(stock);
                                        }
                                    }
                                }


                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                try { transaction.Rollback(); } catch { /* Ignore rollback errors */ }
                                System.Diagnostics.Debug.WriteLine($"Error ensuring stocks exist: {ex.Message}");
                                throw;
                            }
                        }

                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                foreach (var userArticle in mockUserArticles)
                                {
                                    bool exists = false;
                                    using (var checkCommand = new SqlCommand("SELECT COUNT(*) FROM USER_ARTICLE WHERE ARTICLE_ID = @ArticleId", connection, transaction))
                                    {
                                        checkCommand.CommandTimeout = 30;
                                        checkCommand.Parameters.AddWithValue("@ArticleId", userArticle.ArticleId);
                                        exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                                    }

                                    if (!exists)
                                    {
                                        string userArticleQuery = "INSERT INTO USER_ARTICLE (ARTICLE_ID, TITLE, SUMMARY, CONTENT, AUTHOR_CNP, SUBMISSION_DATE, STATUS, TOPIC) VALUES (@ArticleId, @Title, @Summary, @Content, @AuthorCNP, @SubmissionDate, @Status, @Topic)";
                                        using (var command = new SqlCommand(userArticleQuery, connection, transaction))
                                        {
                                            command.CommandTimeout = 30;
                                            command.Parameters.AddWithValue("@ArticleId", userArticle.ArticleId);
                                            command.Parameters.AddWithValue("@Title", userArticle.Title);
                                            command.Parameters.AddWithValue("@Summary", userArticle.Summary ?? "");
                                            command.Parameters.AddWithValue("@Content", userArticle.Content);
                                            command.Parameters.AddWithValue("@AuthorCNP", userArticle.Author);
                                            command.Parameters.AddWithValue("@SubmissionDate", userArticle.SubmissionDate.ToString("yyyy-MM-dd HH:mm:ss"));
                                            command.Parameters.AddWithValue("@Status", userArticle.Status);
                                            command.Parameters.AddWithValue("@Topic", userArticle.Topic);
                                            command.ExecuteNonQuery();
                                        }
                                    }
                                }

                                foreach (var newsArticle in mockNewsArticles)
                                {
                                    bool exists = false;
                                    using (var checkCommand = new SqlCommand("SELECT COUNT(*) FROM NEWS_ARTICLE WHERE ARTICLE_ID = @ArticleId", connection, transaction))
                                    {
                                        checkCommand.CommandTimeout = 30;
                                        checkCommand.Parameters.AddWithValue("@ArticleId", newsArticle.ArticleId);
                                        exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                                    }

                                    if (!exists)
                                    {
                                        string newsArticleQuery = "INSERT INTO NEWS_ARTICLE (ARTICLE_ID, TITLE, SUMMARY, CONTENT, SOURCE, PUBLISH_DATE, IS_READ, IS_WATCHLIST_RELATED, CATEGORY) VALUES (@ArticleId, @Title, @Summary, @Content, @Source, @PublishedDate, @IsRead, @IsWatchlistRelated, @Category)";
                                        using (var command = new SqlCommand(newsArticleQuery, connection, transaction))
                                        {
                                            command.CommandTimeout = 30;
                                            command.Parameters.AddWithValue("@ArticleId", newsArticle.ArticleId);
                                            command.Parameters.AddWithValue("@Title", newsArticle.Title);
                                            command.Parameters.AddWithValue("@Summary", newsArticle.Summary ?? "");
                                            command.Parameters.AddWithValue("@Content", newsArticle.Content);
                                            command.Parameters.AddWithValue("@Source", newsArticle.Source ?? "");
                                            command.Parameters.AddWithValue("@PublishedDate", newsArticle.PublishedDate);
                                            command.Parameters.AddWithValue("@IsRead", newsArticle.IsRead);
                                            command.Parameters.AddWithValue("@IsWatchlistRelated", newsArticle.IsWatchlistRelated);
                                            command.Parameters.AddWithValue("@Category", newsArticle.Category ?? "");
                                            command.ExecuteNonQuery();
                                        }
                                    }
                                }

                                foreach (var userArticle in mockUserArticles.Where(ua => ua.Status == "Approved"))
                                {
                                    bool exists = false;
                                    using (var checkCommand = new SqlCommand("SELECT COUNT(*) FROM NEWS_ARTICLE WHERE ARTICLE_ID = @ArticleId", connection, transaction))
                                    {
                                        checkCommand.CommandTimeout = 30;
                                        checkCommand.Parameters.AddWithValue("@ArticleId", userArticle.ArticleId);
                                        exists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                                    }

                                    if (!exists)
                                    {
                                        string newsArticleQuery = "INSERT INTO NEWS_ARTICLE (ARTICLE_ID, TITLE, SUMMARY, CONTENT, SOURCE, PUBLISH_DATE, IS_READ, IS_WATCHLIST_RELATED, CATEGORY) VALUES (@ArticleId, @Title, @Summary, @Content, @Source, @PublishedDate, @IsRead, @IsWatchlistRelated, @Category)";
                                        using (var command = new SqlCommand(newsArticleQuery, connection, transaction))
                                        {
                                            command.CommandTimeout = 30;
                                            command.Parameters.AddWithValue("@ArticleId", userArticle.ArticleId);
                                            command.Parameters.AddWithValue("@Title", userArticle.Title);
                                            command.Parameters.AddWithValue("@Summary", userArticle.Summary ?? "");
                                            command.Parameters.AddWithValue("@Content", userArticle.Content);
                                            command.Parameters.AddWithValue("@Source", $"User: {userArticle.Author}");
                                            command.Parameters.AddWithValue("@PublishedDate", userArticle.SubmissionDate.ToString("MMMM dd, yyyy"));
                                            command.Parameters.AddWithValue("@IsRead", false);
                                            command.Parameters.AddWithValue("@IsWatchlistRelated", false);
                                            command.Parameters.AddWithValue("@Category", userArticle.Topic);
                                            command.ExecuteNonQuery();
                                        }
                                    }
                                }

                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                try { transaction.Rollback(); } catch { /* Ignore rollback errors */ }
                                System.Diagnostics.Debug.WriteLine($"Error adding articles: {ex.Message}");
                                throw;
                            }
                        }

                        using (var transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                foreach (var newsArticle in mockNewsArticles)
                                {
                                    if (newsArticle.RelatedStocks != null && newsArticle.RelatedStocks.Count > 0)
                                    {
                                        foreach (var stockName in newsArticle.RelatedStocks)
                                        {
                                            bool stockExists = false;
                                            using (var checkCommand = new SqlCommand("SELECT COUNT(*) FROM RELATED_STOCKS WHERE STOCK_NAME = @StockName AND ARTICLE_ID = @ArticleId", connection, transaction))
                                            {
                                                checkCommand.CommandTimeout = 30;
                                                checkCommand.Parameters.AddWithValue("@StockName", stockName);
                                                checkCommand.Parameters.AddWithValue("@ArticleId", newsArticle.ArticleId);
                                                stockExists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                                            }

                                            if (!stockExists)
                                            {
                                                using (var command = new SqlCommand("INSERT INTO RELATED_STOCKS (STOCK_NAME, ARTICLE_ID) VALUES (@StockName, @ArticleId)", connection, transaction))
                                                {
                                                    command.CommandTimeout = 30;
                                                    command.Parameters.AddWithValue("@StockName", stockName);
                                                    command.Parameters.AddWithValue("@ArticleId", newsArticle.ArticleId);
                                                    command.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }

                                foreach (var userArticle in mockUserArticles.Where(ua => ua.Status == "Approved"))
                                {
                                    if (userArticle.RelatedStocks != null && userArticle.RelatedStocks.Count > 0)
                                    {
                                        foreach (var stockName in userArticle.RelatedStocks)
                                        {
                                            bool stockExists = false;
                                            using (var checkCommand = new SqlCommand("SELECT COUNT(*) FROM RELATED_STOCKS WHERE STOCK_NAME = @StockName AND ARTICLE_ID = @ArticleId", connection, transaction))
                                            {
                                                checkCommand.CommandTimeout = 30;
                                                checkCommand.Parameters.AddWithValue("@StockName", stockName);
                                                checkCommand.Parameters.AddWithValue("@ArticleId", userArticle.ArticleId);
                                                stockExists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                                            }

                                            if (!stockExists)
                                            {
                                                using (var command = new SqlCommand("INSERT INTO RELATED_STOCKS (STOCK_NAME, ARTICLE_ID) VALUES (@StockName, @ArticleId)", connection, transaction))
                                                {
                                                    command.CommandTimeout = 30;
                                                    command.Parameters.AddWithValue("@StockName", stockName);
                                                    command.Parameters.AddWithValue("@ArticleId", userArticle.ArticleId);
                                                    command.ExecuteNonQuery();
                                                }
                                            }
                                        }
                                    }
                                }

                                transaction.Commit();
                            }
                            catch (Exception ex)
                            {
                                try { transaction.Rollback(); } catch { /* Ignore rollback errors */ }
                                System.Diagnostics.Debug.WriteLine($"Error adding related stocks: {ex.Message}");
                                throw;
                            }
                        }

                        // Reload data after adding
                        LoadNewsArticles();
                        LoadUserArticles();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in hardCodedNewsArticles: {ex.Message}");
                    throw;
                }
            }
        }


        #endregion
    }
}