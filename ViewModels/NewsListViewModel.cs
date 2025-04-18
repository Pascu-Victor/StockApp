namespace StockApp.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Microsoft.UI.Dispatching;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Commands;
    using StockApp.Models;
    using StockApp.Services;
    using StockApp.Views;

    public class NewsListViewModel : ViewModelBase
    {
        private readonly INewsService _newsService;
        private readonly IDispatcher _dispatcherQueue;
        private readonly IAppState _appState;

        // properties
        private ObservableCollection<NewsArticle> _articles = new();
        public ObservableCollection<NewsArticle> Articles
        {
            get => _articles;
            set => SetProperty(ref _articles, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        private bool _isEmptyState;
        public bool IsEmptyState
        {
            get => _isEmptyState;
            set => SetProperty(ref _isEmptyState, value);
        }

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    FilterArticles();
                }
            }
        }

        private ObservableCollection<string> _categories = new();
        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    FilterArticles();
                }
            }
        }

        private NewsArticle _selectedArticle;
        public NewsArticle SelectedArticle
        {
            get => _selectedArticle;
            set
            {
                if (SetProperty(ref _selectedArticle, value) && value != null)
                {
                    // Store the article ID before clearing the selection
                    var articleId = value.ArticleId;
                    // Clear selection first to prevent UI issues
                    _selectedArticle = null;
                    OnPropertyChanged(nameof(SelectedArticle));
                    // Then navigate
                    NavigateToArticleDetail(articleId);
                }
            }
        }

        // user and auth properties
        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (SetProperty(ref _currentUser, value))
                {
                    OnPropertyChanged(nameof(IsAdmin));
                    OnPropertyChanged(nameof(IsLoggedIn));
                }
            }
        }

        public bool IsAdmin => CurrentUser?.IsModerator ?? false;
        public bool IsLoggedIn => CurrentUser != null;

        // commands
        public ICommand RefreshCommand { get; }
        public ICommand CreateArticleCommand { get; }
        public ICommand AdminPanelCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ClearSearchCommand { get; }

        // constructor
        public NewsListViewModel(
        INewsService newsService,
        IDispatcher dispatcherQueue,
        IAppState appState)
        {
            _newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));

            RefreshCommand = new StockNewsRelayCommand(async () => await RefreshArticlesAsync());
            CreateArticleCommand = new StockNewsRelayCommand(() => NavigateToCreateArticle());
            AdminPanelCommand = new StockNewsRelayCommand(() => NavigateToAdminPanel());
            LoginCommand = new StockNewsRelayCommand(async () => await ShowLoginDialogAsync());
            LogoutCommand = new StockNewsRelayCommand(() => LogoutUser());
            ClearSearchCommand = new StockNewsRelayCommand(() => SearchQuery = string.Empty);

            Categories.Add("All");
            Categories.Add("Stock News");
            Categories.Add("Company News");
            Categories.Add("Market Analysis");
            Categories.Add("Economic News");
            Categories.Add("Functionality News");
            _selectedCategory = "All";
        }

        public NewsListViewModel()
          : this(new NewsService(),
                 new DispatcherAdapter(),
                 AppState.Instance)
        {
        }

        // methods
        public async void Initialize()
        {
            IsLoading = true;
            IsEmptyState = false;

            try
            {
                // get current user
                await GetCurrentUserAsync();

                // load articles
                await RefreshArticlesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing NewsListViewModel: {ex.Message}");
                IsEmptyState = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GetCurrentUserAsync()
        {
            try
            {
                var user = await _newsService.GetCurrentUserAsync();
                _dispatcherQueue.TryEnqueue(() =>
                {
                    CurrentUser = user;
                    _appState.CurrentUser = (User)user;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting current user: {ex.Message}");
                _dispatcherQueue.TryEnqueue(() =>
                {
                    CurrentUser = null;
                    _appState.CurrentUser = null;
                });
            }
        }

        private async Task RefreshArticlesAsync()
        {
            if (IsRefreshing)
                return;

            IsRefreshing = true;
            IsEmptyState = false;

            try
            {
                var articles = await _newsService.GetNewsArticlesAsync();

                _dispatcherQueue.TryEnqueue(() =>
                {
                    // store the full list of articles for filtering
                    _newsService.UpdateCachedArticles(articles);

                    // apply filters to the new data
                    FilterArticles();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing articles: {ex.Message}");
                _dispatcherQueue.TryEnqueue(() =>
                {
                    IsEmptyState = true;
                });
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void FilterArticles()
        {
            if (Articles == null)
            {
                IsEmptyState = true;
                return;
            }

            // get all articles from the original source
            var allArticles = _newsService.GetCachedArticles();
            if (allArticles == null || !allArticles.Any())
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    Articles.Clear();
                    IsEmptyState = true;
                });
                return;
            }

            var filteredArticles = allArticles.ToList();

            // filter by category
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All")
            {
                filteredArticles = filteredArticles.Where(a => a.Category == SelectedCategory).ToList();
            }

            // filter by search query
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filteredArticles = filteredArticles.Where(a =>
                    a.Title.ToLower().Contains(query) ||
                    a.Summary.ToLower().Contains(query) ||
                    a.Content.ToLower().Contains(query) ||
                    a.RelatedStocks != null && a.RelatedStocks.Any(s => s.ToLower().Contains(query))
                ).ToList();
            }

            // sort articles - watchlist items first, then by date (newest first)
            filteredArticles = filteredArticles
                .OrderByDescending(a => a.IsWatchlistRelated)
                .ThenByDescending(a => DateTime.TryParse(a.PublishedDate, out var date) ? date : DateTime.MinValue)
                .ToList();


            _dispatcherQueue.TryEnqueue(() =>
            {
                Articles.Clear();
                foreach (var article in filteredArticles)
                {
                    Articles.Add(article);
                }

                IsEmptyState = !Articles.Any();
            });
        }

        private void NavigateToArticleDetail(string articleId)
        {
            if (string.IsNullOrWhiteSpace(articleId))
            {
                System.Diagnostics.Debug.WriteLine("NavigateToArticleDetail: ArticleId is null or empty");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"NavigateToArticleDetail: Navigating to article {articleId}");
            NavigationService.Instance.NavigateToArticleDetail(articleId);
        }

        private void NavigateToCreateArticle()
        {
            NavigationService.Instance.Navigate(typeof(ArticleCreationView));
        }

        private void NavigateToAdminPanel()
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
                    PrimaryButtonText = "Login"
                };

                var panel = new StackPanel { Spacing = 10 };

                var usernameBox = new TextBox
                {
                    PlaceholderText = "Username",
                    Header = "Username"
                };

                var passwordBox = new PasswordBox
                {
                    PlaceholderText = "Password",
                    Header = "Password"
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

                    await LoginUserAsync(username, password);
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
                IsLoading = true;

                var user = await _newsService.LoginAsync(username, password);

                if (user != null)
                {
                    CurrentUser = user;
                    _appState.CurrentUser = (User)user;

                    // success message
                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = $"Welcome, {user.Username}!",
                        CloseButtonText = "OK",
                        XamlRoot = App.CurrentWindow.Content.XamlRoot
                    };

                    await dialog.ShowAsync();

                    // refresh articles to show user-specific content
                    await RefreshArticlesAsync();
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
                IsLoading = false;
            }
        }

        private void LogoutUser()
        {
            try
            {
                _newsService.Logout();
                CurrentUser = null;
                _appState.CurrentUser = null;

                // refresh articles to show non-user-specific content
                RefreshArticlesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging out: {ex.Message}");
            }
        }

        private async Task ShowErrorDialogAsync(string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot
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

