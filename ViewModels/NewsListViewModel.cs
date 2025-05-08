namespace StockApp.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Commands;
    using StockApp.Models;
    using StockApp.Models.Articles;
    using StockApp.Services;
    using StockApp.Views;

    public class NewsListViewModel : ViewModelBase
    {
        private readonly INewsService newsService;
        private static readonly IUserService UserService = App.Host.Services.GetService(typeof(IUserService)) as IUserService ?? throw new InvalidOperationException("UserService not found");

        // properties
        private ObservableCollection<NewsArticle> articles = [];

        private bool isLoading;

        private bool isRefreshing;

        private bool isEmptyState;

        private string searchQuery = string.Empty;

        private ObservableCollection<string> categories = [];

        private string selectedCategory;

        private NewsArticle? selectedArticle;

        // user and auth properties
        private User currentUser;

        /// <summary>
        /// Gets or sets the collection of news articles.
        /// </summary>
        public ObservableCollection<NewsArticle> Articles
        {
            get => this.articles;
            set => this.SetProperty(ref this.articles, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether data is currently loading.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a refresh operation is in progress.
        /// </summary>
        public bool IsRefreshing
        {
            get => this.isRefreshing;
            set => this.SetProperty(ref this.isRefreshing, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the UI should show the empty state.
        /// </summary>
        public bool IsEmptyState
        {
            get => this.isEmptyState;
            set => this.SetProperty(ref this.isEmptyState, value);
        }

        /// <summary>
        /// Gets or sets the query text used to filter articles.
        /// </summary>
        public string SearchQuery
        {
            get => this.searchQuery;
            set
            {
                if (this.SetProperty(ref this.searchQuery, value))
                {
                    this.FilterArticles();
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of available article categories.
        /// </summary>
        public ObservableCollection<string> Categories
        {
            get => this.categories;
            set => this.SetProperty(ref this.categories, value);
        }

        /// <summary>
        /// Gets or sets the currently selected category for filtering.
        /// </summary>
        public string SelectedCategory
        {
            get => this.selectedCategory;
            set
            {
                if (this.SetProperty(ref this.selectedCategory, value))
                {
                    this.FilterArticles();
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently selected news article.
        /// Selecting an article will navigate to its detail view.
        /// </summary>
        public NewsArticle? SelectedArticle
        {
            get => this.selectedArticle;
            set
            {
                if (this.SetProperty(ref this.selectedArticle, value) && value != null)
                {
                    // Store the article ID before clearing the selection
                    var articleId = value.Id;

                    // Clear selection first to prevent UI issues
                    this.selectedArticle = null;
                    this.OnPropertyChanged(nameof(this.SelectedArticle));

                    // Then navigate
                    NavigateToArticleDetail(articleId);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current authenticated user.
        /// </summary>
        public User CurrentUser
        {
            get => this.currentUser;
            set
            {
                if (this.SetProperty(ref this.currentUser, value))
                {
                    this.OnPropertyChanged(nameof(this.IsAdmin));
                    this.OnPropertyChanged(nameof(this.IsLoggedIn));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current user has moderator privileges.
        /// </summary>
        public bool IsAdmin => this.CurrentUser?.IsModerator ?? false;

        /// <summary>
        /// Gets a value indicating whether there is a user currently logged in.
        /// </summary>
        public bool IsLoggedIn => this.CurrentUser != null;

        // commands

        /// <summary>
        /// Gets the command to refresh the list of articles.
        /// </summary>
        public ICommand RefreshCommand { get; }

        /// <summary>
        /// Gets the command to navigate to the article creation view.
        /// </summary>
        public ICommand CreateArticleCommand { get; }

        /// <summary>
        /// Gets the command to navigate to the admin panel.
        /// </summary>
        public ICommand AdminPanelCommand { get; }

        /// <summary>
        /// Gets the command to show the login dialog.
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// Gets the command to log out the current user.
        /// </summary>
        public ICommand LogoutCommand { get; }

        /// <summary>
        /// Gets the command to clear the current search query.
        /// </summary>
        public ICommand ClearSearchCommand { get; }

        // constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NewsListViewModel"/> class with specified services.
        /// </summary>
        public NewsListViewModel(INewsService newsService)
        {
            this.newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));

            this.RefreshCommand = new StockNewsRelayCommand(() => this.RefreshArticles());
            this.CreateArticleCommand = new StockNewsRelayCommand(() => NavigateToCreateArticle());
            this.AdminPanelCommand = new StockNewsRelayCommand(() => NavigateToAdminPanel());
            this.LoginCommand = new StockNewsRelayCommand(async () => await this.ShowLoginDialogAsync());
            this.LogoutCommand = new StockNewsRelayCommand(() => this.LogoutUser());
            this.ClearSearchCommand = new StockNewsRelayCommand(() => this.SearchQuery = string.Empty);

            this.Categories.Add("All");
            this.Categories.Add("Stock News");
            this.Categories.Add("Company News");
            this.Categories.Add("Market Analysis");
            this.Categories.Add("Economic News");
            this.Categories.Add("Functionality News");
            this.selectedCategory = "All";

        }

        public async Task RefreshArticles()
        {
            if (this.IsRefreshing)
            {
                return;
            }

            this.IsRefreshing = true;
            this.IsEmptyState = false;

            try
            {
                var articles = await this.newsService.GetNewsArticles();

                // store the full list of articles for filtering
                this.newsService.UpdateCachedArticles(articles);

                // apply filters to the new data
                this.FilterArticles();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing articles: {ex.Message}");
                throw;

            }
            finally
            {
                this.IsRefreshing = false;
            }
        }

        private async Task FilterArticles()
        {
            if (this.Articles == null)
            {
                this.IsEmptyState = true;
                return;
            }

            // get all articles from the original source
            var allArticles = await this.newsService.GetCachedArticles();
            if (allArticles == null || !allArticles.Any())
            {
                this.Articles.Clear();
                this.IsEmptyState = true;
                return;
            }

            var filteredArticles = allArticles.ToList();

            // filter by category
            if (!string.IsNullOrEmpty(this.SelectedCategory) && this.SelectedCategory != "All")
            {
                filteredArticles = [.. filteredArticles.Where(a => a.Category == this.SelectedCategory)];
            }
            // Updated filtering logic for RelatedStocks to fix CS1929 error
            if (!string.IsNullOrEmpty(this.SearchQuery))
            {
                var query = this.SearchQuery.ToLower();
                filteredArticles = [.. filteredArticles.Where(a =>
                        a.Title.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                        a.Summary.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                        a.Content.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                        (a.RelatedStocks != null && a.RelatedStocks.Any(s =>
                            s.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                            s.Symbol.Contains(query, StringComparison.CurrentCultureIgnoreCase))))];
            }

            // Inline: Sort watchlist items first, then by date (newest first)
            filteredArticles = [..filteredArticles
            .OrderByDescending(a => a.IsWatchlistRelated)
                .ThenByDescending(a => a.PublishedOn)];


            this.Articles.Clear();
            foreach (var article in filteredArticles)
            {
                this.Articles.Add(article);
            }

            this.IsEmptyState = !this.Articles.Any();
        }

        private static void NavigateToArticleDetail(int articleId)
        {
            System.Diagnostics.Debug.WriteLine($"NavigateToArticleDetail: Navigating to article {articleId}");
            //FIXME: make this not use the "navigation service" just poop out some modal thinggie
            //this.Navigate(typeof(StockApp.Views.NewsArticleView), new ArticleNavigationParameter
            //{
            //    ArticleId = articleId,
            //    IsPreview = false,
            //});
        }

        private static void NavigateToCreateArticle()
        {
            NavigationService.Instance.Navigate(typeof(ArticleCreationView));
        }

        private static void NavigateToAdminPanel()
        {
            NavigationService.Instance.Navigate(typeof(AdminNewsControlView));
        }

        private async Task ShowLoginDialogAsync()
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Login",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Login",
                };

                var panel = new StackPanel { Spacing = 10 };

                // Inline: Build login dialog UI elements
                var usernameBox = new TextBox
                {
                    PlaceholderText = "Username",
                    Header = "Username",
                };

                var passwordBox = new PasswordBox
                {
                    PlaceholderText = "Password",
                    Header = "Password",
                };

                panel.Children.Add(usernameBox);
                panel.Children.Add(passwordBox);

                dialog.Content = panel;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var username = usernameBox.Text;
                    var password = passwordBox.Password;

                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        await ShowErrorDialogAsync("Please enter both username and password.");
                        return;
                    }

                    await this.LoginUserAsync(username, password);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing login dialog: {ex.Message}");
                await ShowErrorDialogAsync("An error occurred while trying to show the login dialog.");
            }
        }

        private async Task LoginUserAsync(string username, string password)
        {
            try
            {
                this.IsLoading = true;

                var user = await this.newsService.LoginAsync(username, password);

                if (user != null)
                {
                    this.CurrentUser = user;
                    //this.appState.CurrentUser = (User)user;

                    // success message
                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = $"Welcome, {user.Username}!",
                        CloseButtonText = "OK",
                        XamlRoot = App.CurrentWindow.Content.XamlRoot,
                    };

                    await dialog.ShowAsync();

                    // refresh articles to show user-specific content
                    this.RefreshArticles();
                }
                else
                {
                    await ShowErrorDialogAsync("Invalid username or password.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging in: {ex.Message}");
                await ShowErrorDialogAsync("An error occurred while trying to log in.");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        private void LogoutUser()
        {
            try
            {
                this.newsService.Logout();
                this.CurrentUser = null;

                // refresh articles to show non-user-specific content
                this.RefreshArticles();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging out: {ex.Message}");
            }
        }

        private static async Task ShowErrorDialogAsync(string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing error dialog: {ex.Message}");
            }
        }
    }
}
