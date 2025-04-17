using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using StockNewsPage.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using StockApp;
using StockApp.Model;
using StockApp.Service;

namespace StockNewsPage.ViewModels
{
    public class AdminNewsViewModel : ViewModelBase
    {
        private readonly NewsService _newsService;
        private readonly DispatcherQueue _dispatcherQueue;

        private ObservableCollection<UserArticle> _userArticles = new();
        public ObservableCollection<UserArticle> UserArticles
        {
            get => _userArticles;
            set => SetProperty(ref _userArticles, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private ObservableCollection<string> _statuses = new();
        public ObservableCollection<string> Statuses
        {
            get => _statuses;
            set => SetProperty(ref _statuses, value);
        }

        private string _selectedStatus;
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    RefreshArticlesAsync();
                }
            }
        }

        private ObservableCollection<string> _topics = new();
        public ObservableCollection<string> Topics
        {
            get => _topics;
            set => SetProperty(ref _topics, value);
        }

        private string _selectedTopic;
        public string SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                if (SetProperty(ref _selectedTopic, value))
                {
                    RefreshArticlesAsync();
                }
            }
        }

        private UserArticle _selectedArticle;
        public UserArticle SelectedArticle
        {
            get => _selectedArticle;
            set
            {
                if (SetProperty(ref _selectedArticle, value) && value != null)
                {
                    // nav to preview when selected article changes
                    NavigateToPreview(value ?? throw new ArgumentNullException(nameof(value)));
                    SelectedArticle = null;
                }
            }
        }

        private bool _isEmptyState;
        public bool IsEmptyState
        {
            get => _isEmptyState;
            set => SetProperty(ref _isEmptyState, value);
        }

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand PreviewCommand { get; }

        // Constructor
        public AdminNewsViewModel()
        {
            _newsService = new NewsService();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // init commands
            RefreshCommand = new StockNewsRelayCommand(async () => await RefreshArticlesAsync());
            ApproveCommand = new RelayCommandGeneric<string>(async (id) => await ApproveArticleAsync(id));
            RejectCommand = new RelayCommandGeneric<string>(async (id) => await RejectArticleAsync(id));
            DeleteCommand = new RelayCommandGeneric<string>(async (id) => await DeleteArticleAsync(id));
            BackCommand = new StockNewsRelayCommand(() => NavigationService.Instance.GoBack());
            PreviewCommand = new RelayCommandGeneric<UserArticle>((article) => NavigateToPreview(article));

            // init statuses
            Statuses.Add("All");
            Statuses.Add("Pending");
            Statuses.Add("Approved");
            Statuses.Add("Rejected");

            // init topics
            Topics.Add("All");
            Topics.Add("Stock News");
            Topics.Add("Company News");
            Topics.Add("Functionality News");
            Topics.Add("Market Analysis");
            Topics.Add("Economic News");

            // set default values
            _selectedStatus = "All";
            _selectedTopic = "All";
        }

        public async void Initialize()
        {
            await RefreshArticlesAsync();
        }

        private async Task RefreshArticlesAsync()
        {
            IsLoading = true;
            IsEmptyState = false;

            try
            {
                var status = SelectedStatus == "All" ? null : SelectedStatus;
                var topic = SelectedTopic == "All" ? null : SelectedTopic;

                var articles = await _newsService.GetUserArticlesAsync(status, topic);

                _dispatcherQueue.TryEnqueue(() =>
                {
                    UserArticles.Clear();
                    foreach (var article in articles)
                    {
                        UserArticles.Add(article);
                    }

                    IsEmptyState = UserArticles.Count == 0;
                });
            }
            catch (Exception ex)
            {
                IsEmptyState = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void NavigateToPreview(UserArticle article)
        {
            if (article != null)
            {
                // convert UserArticle to NewsArticle for preview
                var previewArticle = new NewsArticle
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
                    RelatedStocks = article.RelatedStocks ?? new System.Collections.Generic.List<string>()
                };

                // store preview article in the NewsService for retrieval
                _newsService.StorePreviewArticle(previewArticle, article);

                // nav to the article view with a flag indicating it's a preview
                NavigationService.Instance.Navigate(typeof(NewsArticleView), $"preview:{article.ArticleId}");
            }
        }


        private async Task ApproveArticleAsync(string articleId)
        {
            try
            {
                var success = await _newsService.ApproveUserArticleAsync(articleId);
                if (success)
                {
                    await RefreshArticlesAsync();

                    // success message
                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Article has been approved and will now appear in the news list.",
                        CloseButtonText = "OK",
                        XamlRoot = App.CurrentWindow.Content.XamlRoot
                    };

                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to approve article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
        }

        private async Task RejectArticleAsync(string articleId)
        {
            try
            {
                var success = await _newsService.RejectUserArticleAsync(articleId);
                if (success)
                {
                    await RefreshArticlesAsync();

                    // success message
                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Article has been rejected.",
                        CloseButtonText = "OK",
                        XamlRoot = App.CurrentWindow.Content.XamlRoot
                    };

                    await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to reject article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
        }

        private async Task DeleteArticleAsync(string articleId)
        {
            try
            {
                // confirm deletion
                var confirmDialog = new ContentDialog
                {
                    Title = "Confirm Deletion",
                    Content = "Are you sure you want to delete this article? This action cannot be undone.",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot
                };

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var success = await _newsService.DeleteUserArticleAsync(articleId);
                    if (success)
                    {
                        await RefreshArticlesAsync();

                        // success message
                        var dialog = new ContentDialog
                        {
                            Title = "Success",
                            Content = "Article has been deleted.",
                            CloseButtonText = "OK",
                            XamlRoot = App.CurrentWindow.Content.XamlRoot
                        };

                        await dialog.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to delete article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
        }

        // method to set property and raise PropertyChanged event
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

