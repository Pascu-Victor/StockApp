namespace StockApp.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Microsoft.UI.Xaml.Controls;
    using StockApp;
    using StockApp.Commands;
    using StockApp.Models;
    using StockApp.Models.Articles;
    using StockApp.Services;
    using StockApp.Views;

    /// <summary>
    /// ViewModel for the admin news moderation screen, handling retrieval,
    /// filtering, and actions on user-submitted articles.
    /// </summary>
    public class AdminNewsViewModel : ViewModelBase
    {
        private readonly INewsService newsService;
        private readonly IDispatcher dispatcherQueue;

        private ObservableCollection<UserArticle> userArticles = [];
        private bool isLoading;
        private ObservableCollection<string> statuses = [];
        private Status selectedStatus;
        private ObservableCollection<string> topics = [];
        private string selectedTopic;
        private UserArticle? selectedArticle;
        private bool isEmptyState;

        /// <summary>
        /// Initializes a new instance of <see cref="AdminNewsViewModel"/> with the specified service and dispatcher.
        /// </summary>
        /// <param name="service">Service for retrieving and modifying news articles.</param>
        /// <param name="dispatcherQueue">Dispatcher used for UI thread operations.</param>
        public AdminNewsViewModel(
            INewsService service,
            IDispatcher dispatcherQueue)
        {
            this.newsService = service ?? throw new ArgumentNullException(nameof(service));
            this.dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
            this.InitializeCommandsAndFilters();
        }

        /// <summary>
        /// Sets up commands and default filter values.
        /// </summary>
        private void InitializeCommandsAndFilters()
        {
            // Initialize command bindings
            this.RefreshCommand = new StockNewsRelayCommand(async () => await this.RefreshArticlesAsync());
            this.ApproveCommand = new RelayCommandGeneric<int>(async (id) => await this.ApproveArticleAsync(id));
            this.RejectCommand = new RelayCommandGeneric<int>(async (id) => await this.RejectArticleAsync(id));
            this.DeleteCommand = new RelayCommandGeneric<int>(async (id) => await this.DeleteArticleAsync(id));
            this.BackCommand = new StockNewsRelayCommand(() => NavigationService.Instance.GoBack());
            this.PreviewCommand = new RelayCommandGeneric<UserArticle>((article) => this.NavigateToPreview(article));

            // Populate status filter options
            this.Statuses.Add("All");
            this.Statuses.Add("Pending");
            this.Statuses.Add("Approved");
            this.Statuses.Add("Rejected");

            // Populate topic filter options
            this.Topics.Add("All");
            this.Topics.Add("Stock News");
            this.Topics.Add("Company News");
            this.Topics.Add("Functionality News");
            this.Topics.Add("Market Analysis");
            this.Topics.Add("Economic News");

            // Set default selections
            this.selectedStatus = Status.All;
            this.selectedTopic = "All";
        }

        /// <summary>
        /// Gets or sets the collection of user articles to display.
        /// </summary>
        public ObservableCollection<UserArticle> UserArticles
        {
            get => this.userArticles;
            set => this.SetProperty(ref this.userArticles, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether articles are currently being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        /// <summary>
        /// Gets or sets the available statuses for filtering.
        /// </summary>
        public ObservableCollection<string> Statuses
        {
            get => this.statuses;
            set => this.SetProperty(ref this.statuses, value);
        }

        /// <summary>
        /// Gets or sets the currently selected status filter.
        /// Changing this triggers an article refresh.
        /// </summary>
        public Status SelectedStatus
        {
            get => this.selectedStatus;
            set
            {
                if (this.SetProperty(ref this.selectedStatus, value))
                {
                    // Refresh articles when the filter changes
                    this.RefreshArticlesAsync();
                }
            }
        }

        /// <summary>
        /// Gets or sets the available topics for filtering.
        /// </summary>
        public ObservableCollection<string> Topics
        {
            get => this.topics;
            set => this.SetProperty(ref this.topics, value);
        }

        /// <summary>
        /// Gets or sets the currently selected topic filter.
        /// Changing this triggers an article refresh.
        /// </summary>
        public string SelectedTopic
        {
            get => this.selectedTopic;
            set
            {
                if (this.SetProperty(ref this.selectedTopic, value))
                {
                    // Refresh articles when the filter changes
                    this.RefreshArticlesAsync();
                }
            }
        }

        /// <summary>
        /// Gets or sets the article selected by the user for preview.
        /// Selecting an article navigates to the preview view.
        /// </summary>
        public UserArticle? SelectedArticle
        {
            get => this.selectedArticle;
            set
            {
                if (this.SetProperty(ref this.selectedArticle, value))
                {
                    ArgumentNullException.ThrowIfNull(value);

                    // Navigate to preview when selected article changes
                    this.NavigateToPreview(value);
                    this.selectedArticle = null; // Reset after navigation
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the empty state (no articles) is shown.
        /// </summary>
        public bool IsEmptyState
        {
            get => this.isEmptyState;
            set => this.SetProperty(ref this.isEmptyState, value);
        }

        /// <summary>
        /// Gets the command to refresh the article list.
        /// </summary>
        public ICommand RefreshCommand { get; private set; }

        /// <summary>
        /// Gets the command to approve an article.
        /// </summary>
        public ICommand ApproveCommand { get; private set; }

        /// <summary>
        /// Gets the command to reject an article.
        /// </summary>
        public ICommand RejectCommand { get; private set; }

        /// <summary>
        /// Gets the command to delete an article.
        /// </summary>
        public ICommand DeleteCommand { get; private set; }

        /// <summary>
        /// Gets the command to navigate back.
        /// </summary>
        public ICommand BackCommand { get; private set; }

        /// <summary>
        /// Gets the command to preview an article.
        /// </summary>
        public ICommand PreviewCommand { get; private set; }

        /// <summary>
        /// Initializes the ViewModel by loading articles.
        /// </summary>
        public async void Initialize()
        {
            await this.RefreshArticlesAsync();
        }

        /// <summary>
        /// Asynchronously retrieves and applies filters to the list of user articles.
        /// </summary>
        private async Task RefreshArticlesAsync()
        {
            this.IsLoading = true;
            this.IsEmptyState = false;

            try
            {
                // Determine filter values (null means no filter for "All")
                //FIXME: do not convert this to string
                string? topic = this.SelectedTopic == "All" ? null : this.SelectedTopic;

                var articles = await this.newsService.GetUserArticles(this.selectedStatus, topic);

                // Update the collection on the UI thread
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
                // If fetching fails, show the empty state
                this.IsEmptyState = true;
            }
            finally
            {
                // Always hide the loading indicator
                this.IsLoading = false;
            }
        }

        /// <summary>
        /// Converts a <see cref="UserArticle"/> into a <see cref="NewsArticle"/> preview
        /// and navigates to the preview view.
        /// </summary>
        /// <param name="article">The user article to preview.</param>
        private void NavigateToPreview(UserArticle article)
        {
            if (article == null)
            {
                return;
            }

            // Prepare a preview NewsArticle from the user-submitted article
            var previewArticle = new NewsArticle(
                title: article.Title,
                summary: article.Summary,
                content: article.Content,
                source: article.AuthorId,
                publishedOn: article.PublishedOn
                );

            // Store the preview for later retrieval
            this.newsService.StorePreviewArticle(previewArticle, article);

            // Navigate to the article view in preview mode
            NavigationService.Instance.Navigate(
                typeof(NewsArticleView), new ArticleNavigationParameter
                {
                    ArticleId = article.Id,
                    IsPreview = true,
                });
        }

        /// <summary>
        /// Approves a user-submitted article and shows a confirmation dialog.
        /// </summary>
        /// <param name="articleId">The identifier of the article to approve.</param>
        private async Task ApproveArticleAsync(int articleId)
        {
            try
            {
                var success = await this.newsService.ApproveUserArticle(articleId);
                if (success)
                {
                    await this.RefreshArticlesAsync();

                    // Show success dialog
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
                // Show error dialog on failure
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

        /// <summary>
        /// Rejects a user-submitted article and shows a confirmation dialog.
        /// </summary>
        /// <param name="articleId">The identifier of the article to reject.</param>
        private async Task RejectArticleAsync(int articleId)
        {
            try
            {
                var success = await this.newsService.RejectUserArticle(articleId);
                if (success)
                {
                    await this.RefreshArticlesAsync();

                    // Show success dialog
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
                // Show error dialog on failure
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

        /// <summary>
        /// Deletes a user-submitted article after confirmation and shows a result dialog.
        /// </summary>
        /// <param name="articleId">The identifier of the article to delete.</param>
        private async Task DeleteArticleAsync(int articleId)
        {
            try
            {
                // Confirm deletion with the user
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
                    var success = await this.newsService.DeleteUserArticle(articleId);
                    if (success)
                    {
                        await this.RefreshArticlesAsync();

                        // Show success dialog
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
                // Show error dialog on failure
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

        /// <summary>
        /// Sets the backing field for a property and raises <see cref="OnPropertyChanged"/> if the value changed.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to the backing field.</param>
        /// <param name="value">New value to assign.</param>
        /// <param name="propertyName">Name of the property (automatically supplied).</param>
        /// <returns>True if the value changed; otherwise false.</returns>
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
