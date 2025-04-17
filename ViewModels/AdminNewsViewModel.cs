namespace StockApp.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Microsoft.UI.Dispatching;
    using Microsoft.UI.Xaml.Controls;
    using StockApp;
    using StockApp.Commands;
    using StockApp.Models;
    using StockApp.Services;
    using StockApp.Views;

    public class AdminNewsViewModel : ViewModelBase
    {
        private readonly INewsService _newsService;
        private readonly DispatcherQueue _dispatcherQueue;

        private ObservableCollection<UserArticle> userArticles = [];
        private bool isLoading;
        private ObservableCollection<string> statuses = [];
        private string selectedStatus;
        private ObservableCollection<string> topics = [];
        private string selectedTopic;
        private UserArticle? selectedArticle;
        private bool isEmptyState;

        // Constructor
        public AdminNewsViewModel()
        {
            this.newsService = new NewsService();
            this.dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // init commands
            this.RefreshCommand = new StockNewsRelayCommand(async () => await this.RefreshArticlesAsync());
            this.ApproveCommand = new RelayCommandGeneric<string>(async (id) => await this.ApproveArticleAsync(id));
            this.RejectCommand = new RelayCommandGeneric<string>(async (id) => await this.RejectArticleAsync(id));
            this.DeleteCommand = new RelayCommandGeneric<string>(async (id) => await this.DeleteArticleAsync(id));
            this.BackCommand = new StockNewsRelayCommand(() => NavigationService.Instance.GoBack());
            this.PreviewCommand = new RelayCommandGeneric<UserArticle>((article) => this.NavigateToPreview(article));

            // init statuses
            this.Statuses.Add("All");
            this.Statuses.Add("Pending");
            this.Statuses.Add("Approved");
            this.Statuses.Add("Rejected");

            // init topics
            this.Topics.Add("All");
            this.Topics.Add("Stock News");
            this.Topics.Add("Company News");
            this.Topics.Add("Functionality News");
            this.Topics.Add("Market Analysis");
            this.Topics.Add("Economic News");

            // set default values
            this.selectedStatus = "All";
            this.selectedTopic = "All";
        }

        public ObservableCollection<UserArticle> UserArticles
        {
            get => this.userArticles;
            set => this.SetProperty(ref this.userArticles, value);
        }

        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        public ObservableCollection<string> Statuses
        {
            get => this.statuses;
            set => this.SetProperty(ref this.statuses, value);
        }

        public string SelectedStatus
        {
            get => this.selectedStatus;
            set
            {
                if (this.SetProperty(ref this.selectedStatus, value))
                {
                    this.RefreshArticlesAsync();
                }
            }
        }

        public ObservableCollection<string> Topics
        {
            get => this.topics;
            set => this.SetProperty(ref this.topics, value);
        }

        public string SelectedTopic
        {
            get => this.selectedTopic;
            set
            {
                if (this.SetProperty(ref this.selectedTopic, value))
                {
                    this.RefreshArticlesAsync();
                }
            }
        }

        public UserArticle? SelectedArticle
        {
            get => this.selectedArticle;
            set
            {
                if (this.SetProperty(ref this.selectedArticle, value))
                {
                    ArgumentNullException.ThrowIfNull(value);

                    // nav to preview when selected article changes
                    this.NavigateToPreview(value);
                    this.selectedArticle = null; // Reset after navigation
                }
            }
        }

        public bool IsEmptyState
        {
            get => this.isEmptyState;
            set => this.SetProperty(ref this.isEmptyState, value);
        }

        // Commands
        public ICommand RefreshCommand { get; }

        public ICommand ApproveCommand { get; }

        public ICommand RejectCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand BackCommand { get; }

        public ICommand PreviewCommand { get; }

        public async void Initialize()
        {
            await this.RefreshArticlesAsync();
        }

        private async Task RefreshArticlesAsync()
        {
            this.IsLoading = true;
            this.IsEmptyState = false;

            try
            {
                string? status = this.SelectedStatus == "All" ? null : this.SelectedStatus;
                string? topic = this.SelectedTopic == "All" ? null : this.SelectedTopic;

                var articles = await this.newsService.GetUserArticlesAsync(status, topic);

                this.dispatcherQueue.TryEnqueue(() =>
                {
                    this.UserArticles.Clear();
                    foreach (var article in articles)
                    {
                        this.UserArticles.Add(article);
                    }

                    this.IsEmptyState = this.UserArticles.Count == 0;
                });
            }
            catch
            {
                this.IsEmptyState = true;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        private void NavigateToPreview(UserArticle article)
        {
            if (article == null)
            {
                return;
            }

            // convert UserArticle to NewsArticle for preview
            var previewArticle = new NewsArticle
            {
                ArticleId = article.ArticleId,
                Title = article.Title,
                Summary = article.Summary ?? string.Empty,
                Content = article.Content,
                Source = $"User: {article.Author}",
                PublishedDate = article.SubmissionDate.ToString("MMMM dd, yyyy"),
                IsRead = false,
                IsWatchlistRelated = false,
                Category = article.Topic,
                RelatedStocks = article.RelatedStocks ?? [],
            };

            // store preview article in the NewsService for retrieval
            this.newsService.StorePreviewArticle(previewArticle, article);

            // nav to the article view with a flag indicating it's a preview
            NavigationService.Instance.Navigate(typeof(NewsArticleView), $"preview:{article.ArticleId}");
        }

        private async Task ApproveArticleAsync(string articleId)
        {
            try
            {
                var success = await this.newsService.ApproveUserArticleAsync(articleId);
                if (success)
                {
                    await this.RefreshArticlesAsync();

                    // success message
                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Article has been approved and will now appear in the news list.",
                        CloseButtonText = "OK",
                        XamlRoot = App.CurrentWindow.Content.XamlRoot,
                    };

                    await dialog.ShowAsync();
                }
            }
            catch
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to approve article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                };

                await dialog.ShowAsync();
            }
        }

        private async Task RejectArticleAsync(string articleId)
        {
            try
            {
                var success = await this.newsService.RejectUserArticleAsync(articleId);
                if (success)
                {
                    await this.RefreshArticlesAsync();

                    // success message
                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Article has been rejected.",
                        CloseButtonText = "OK",
                        XamlRoot = App.CurrentWindow.Content.XamlRoot,
                    };

                    await dialog.ShowAsync();
                }
            }
            catch
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to reject article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
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
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                };

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var success = await this.newsService.DeleteUserArticleAsync(articleId);
                    if (success)
                    {
                        await this.RefreshArticlesAsync();

                        // success message
                        var dialog = new ContentDialog
                        {
                            Title = "Success",
                            Content = "Article has been deleted.",
                            CloseButtonText = "OK",
                            XamlRoot = App.CurrentWindow.Content.XamlRoot,
                        };

                        await dialog.ShowAsync();
                    }
                }
            }
            catch
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to delete article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                };

                await dialog.ShowAsync();
            }
        }

        // method to set property and raise PropertyChanged event
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            this.OnPropertyChanged(propertyName);
            
            return true;
        }
    }
}
