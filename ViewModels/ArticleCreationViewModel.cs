﻿namespace StockApp.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Microsoft.UI.Dispatching;
    using Microsoft.UI.Xaml.Controls;
    using StockApp;
    using StockApp.Commands;
    using StockApp.Models;
    using StockApp.Repositories;
    using StockApp.Services;
    using StockApp.Views;

    /// <summary>
    /// ViewModel for creating, previewing, and submitting user‐authored news articles.
    /// </summary>
    public class ArticleCreationViewModel : ViewModelBase
    {
        private readonly INewsService newsService;
        private readonly IDispatcher dispatcherQueue;
        private readonly IAppState appState;
        private readonly IBaseStocksRepository stocksRepository;

        private string title;
        /// <summary>
        /// Gets or sets the article title.
        /// </summary>
        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value);
        }

        private string summary;
        /// <summary>
        /// Gets or sets the article summary.
        /// </summary>
        public string Summary
        {
            get => this.summary;
            set => this.SetProperty(ref this.summary, value);
        }

        private string content;
        /// <summary>
        /// Gets or sets the full article content.
        /// </summary>
        public string Content
        {
            get => this.content;
            set => this.SetProperty(ref this.content, value);
        }

        private ObservableCollection<string> topics = new();
        /// <summary>
        /// Gets or sets the list of available topics for the article.
        /// </summary>
        public ObservableCollection<string> Topics
        {
            get => this.topics;
            set => this.SetProperty(ref this.topics, value);
        }

        private string selectedTopic;
        /// <summary>
        /// Gets or sets the currently selected topic.
        /// </summary>
        public string SelectedTopic
        {
            get => this.selectedTopic;
            set => this.SetProperty(ref this.selectedTopic, value);
        }

        private string relatedStocksText;
        /// <summary>
        /// Gets or sets the comma‑separated string of related stock symbols.
        /// </summary>
        public string RelatedStocksText
        {
            get => this.relatedStocksText;
            set => this.SetProperty(ref this.relatedStocksText, value);
        }

        private bool isLoading;
        /// <summary>
        /// Gets or sets a value indicating whether an operation is in progress.
        /// </summary>
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetProperty(ref this.isLoading, value);
        }

        private bool hasError;
        /// <summary>
        /// Gets or sets a value indicating whether the view is in an error state.
        /// </summary>
        public bool HasError
        {
            get => this.hasError;
            set => this.SetProperty(ref this.hasError, value);
        }

        private string errorMessage;
        /// <summary>
        /// Gets or sets the current error message; also updates <see cref="HasError"/>.
        /// </summary>
        public string ErrorMessage
        {
            get => this.errorMessage;
            set
            {
                this.SetProperty(ref this.errorMessage, value);
                this.HasError = !string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        /// Command to navigate back to the previous view.
        /// </summary>
        public ICommand BackCommand { get; }

        /// <summary>
        /// Command to clear the article creation form.
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Command to preview the article before submission.
        /// </summary>
        public ICommand PreviewCommand { get; }

        /// <summary>
        /// Command to submit the article for review.
        /// </summary>
        public ICommand SubmitCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleCreationViewModel"/> class with dependencies.
        /// </summary>
        /// <param name="newsService">Service for news operations.</param>
        /// <param name="dispatcher">Dispatcher for UI thread invocation.</param>
        /// <param name="appState">Global application state.</param>
        /// <param name="stocksRepo">Repository for base stock data.</param>
        public ArticleCreationViewModel(
            INewsService newsService,
            IDispatcher dispatcher,
            IAppState appState,
            IBaseStocksRepository stocksRepo)
        {
            this.newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            this.dispatcherQueue = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            this.appState = appState ?? throw new ArgumentNullException(nameof(appState));
            this.stocksRepository = stocksRepo ?? throw new ArgumentNullException(nameof(stocksRepo));

            // Initialize commands
            this.BackCommand = new StockNewsRelayCommand(() => NavigationService.Instance.GoBack());
            this.ClearCommand = new StockNewsRelayCommand(() => this.ClearForm());
            this.PreviewCommand = new StockNewsRelayCommand(async () => await this.PreviewArticleAsync());
            this.SubmitCommand = new StockNewsRelayCommand(async () => await this.SubmitArticleAsync());

            // Initialize topic list
            this.Topics.Add("Stock News");
            this.Topics.Add("Company News");
            this.Topics.Add("Market Analysis");
            this.Topics.Add("Economic News");
            this.Topics.Add("Functionality News");

            // Set default selected topic
            this.selectedTopic = "Stock News";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleCreationViewModel"/> class using default implementations.
        /// </summary>
        public ArticleCreationViewModel()
          : this(
              new NewsService(),
              new DispatcherAdapter(),
              AppState.Instance,
              new BaseStocksRepository())
        { }

        /// <summary>
        /// Performs initial setup, including checking user authentication and clearing the form.
        /// </summary>
        public void Initialize()
        {
            // Ensure user is logged in before allowing article creation
            if (this.appState.CurrentUser == null)
            {
                this.ShowErrorDialog("You must be logged in to create an article.");
                NavigationService.Instance.GoBack();
                return;
            }

            this.ClearForm();
        }

        /// <summary>
        /// Clears all form fields and resets state.
        /// </summary>
        private void ClearForm()
        {
            this.Title = string.Empty;
            this.Summary = string.Empty;
            this.Content = string.Empty;
            this.SelectedTopic = "Stock News";
            this.RelatedStocksText = string.Empty;
            this.ErrorMessage = string.Empty;
        }

        private async Task PreviewArticleAsync()
        {
            if (!this.ValidateForm())
            {
                return;
            }

            try
            {
                this.IsLoading = true;

                // Create a unique ID for this preview
                var previewId = Guid.NewGuid().ToString();

                // Build the NewsArticle for display
                var article = new NewsArticle
                {
                    ArticleId = previewId,
                    Title = this.Title,
                    Summary = this.Summary ?? string.Empty,
                    Content = this.Content,
                    Source = $"User: {this.appState.CurrentUser?.CNP ?? "Anonymous"}",
                    PublishedDate = DateTime.Now.ToString("MMMM dd, yyyy"),
                    IsRead = false,
                    IsWatchlistRelated = false,
                    Category = this.SelectedTopic,
                    RelatedStocks = this.ParseRelatedStocks(),
                };

                // Build a temporary UserArticle for preview context
                var userArticle = new UserArticle
                {
                    ArticleId = previewId,
                    Title = this.Title,
                    Summary = this.Summary ?? string.Empty,
                    Content = this.Content,
                    Author = this.appState.CurrentUser ?? throw new InvalidOperationException("User not found"),
                    SubmissionDate = DateTime.Now,
                    Status = "Preview",
                    Topic = this.SelectedTopic,
                    RelatedStocks = this.ParseRelatedStocks(),
                };

                // Store preview in the service
                this.newsService.StorePreviewArticle(article, userArticle);

                // Navigate to the preview view
                NavigationService.Instance.Navigate(
                    typeof(NewsArticleView),
                    $"preview:{previewId}");
            }
            catch (Exception ex)
            {
                // Log and display an error if preview fails
                System.Diagnostics.Debug.WriteLine($"Error previewing article: {ex.Message}");
                this.ErrorMessage = "An error occurred while trying to preview the article.";
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        private async Task SubmitArticleAsync()
        {
            if (!this.ValidateForm())
            {
                return;
            }

            this.IsLoading = true;
            this.ErrorMessage = string.Empty;

            try
            {
                // Parse and validate the related stocks list
                List<string> relatedStocks = this.ParseRelatedStocks();

                bool continueSubmission = await this.ValidateStocksAsync(relatedStocks);
                if (!continueSubmission)
                {
                    this.IsLoading = false;
                    return;
                }

                // Build the UserArticle to submit
                var article = new UserArticle
                {
                    ArticleId = Guid.NewGuid().ToString(),
                    Title = this.Title,
                    Summary = this.Summary,
                    Content = this.Content,
                    Author = this.appState.CurrentUser ?? throw new InvalidOperationException("User not found"),
                    SubmissionDate = DateTime.Now,
                    Status = "Pending",
                    Topic = this.SelectedTopic,
                    RelatedStocks = relatedStocks,
                };

                bool success = await this.newsService.SubmitUserArticleAsync(article);

                if (success)
                {
                    // Show confirmation and clear form once done
                    this.dispatcherQueue.TryEnqueue(async () =>
                    {
                        var dialog = new ContentDialog
                        {
                            Title = "Success",
                            Content = "Your article has been submitted for review.",
                            CloseButtonText = "OK",
                            XamlRoot = App.CurrentWindow.Content.XamlRoot,
                        };

                        await dialog.ShowAsync();
                        this.ClearForm();
                        NavigationService.Instance.GoBack();
                    });
                }
                else
                {
                    this.ShowErrorDialog("Failed to submit article. Please try again later.");
                }
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"An error occurred: {ex.Message}");
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        private bool ValidateForm()
        {
            this.ErrorMessage = string.Empty;

            // Ensure required fields are populated
            if (string.IsNullOrWhiteSpace(this.Title))
            {
                this.ErrorMessage = "Title is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.Summary))
            {
                this.ErrorMessage = "Summary is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.Content))
            {
                this.ErrorMessage = "Content is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.SelectedTopic))
            {
                this.ErrorMessage = "Topic is required.";
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateStocksAsync(List<string> enteredStocks)
        {
            if (enteredStocks == null)
            {
                throw new ArgumentNullException(nameof(enteredStocks));
            }

            if (enteredStocks.Count == 0)
            {
                return true;
            }

            var allStocks = this.stocksRepository.GetAllStocks()
                ?? throw new InvalidOperationException("Stocks repository returned null");

            var invalidStocks = new List<string>();

            foreach (var stock in enteredStocks)
            {
                if (string.IsNullOrWhiteSpace(stock))
                {
                    throw new ArgumentException("Stock name cannot be null or whitespace.", nameof(enteredStocks));
                }

                bool stockExists = allStocks.Any(s =>
                    s.Name.Equals(stock, StringComparison.OrdinalIgnoreCase) ||
                    s.Symbol.Equals(stock, StringComparison.OrdinalIgnoreCase));

                if (!stockExists)
                {
                    invalidStocks.Add(stock);
                }
            }

            if (invalidStocks.Count > 0)
            {
                // Build a short list of available stocks for user reference
                var availableStocksList = allStocks
                    .OrderBy(s => s.Name)
                    .Take(20) // Limit to first 20 to keep dialog concise
                    .Select(s => $"• {s.Name} ({s.Symbol})")
                    .ToList();

                string invalidStocksMessage = string.Join(", ", invalidStocks);
                string availableStocksMessage = string.Join("\n", availableStocksList);
                if (allStocks.Count > 20)
                {
                    availableStocksMessage += $"\n(and {allStocks.Count - 20} more...)";
                }

                string message =
                    $"The following stocks are not found: {invalidStocksMessage}\n\n" +
                    "Would you like to continue anyway? We'll create these automatically.\n\n" +
                    $"Available stocks include:\n{availableStocksMessage}";

                var dialog = new ContentDialog
                {
                    Title = "Stock Validation",
                    Content = message,
                    PrimaryButtonText = "Continue Anyway",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.CurrentWindow.Content.XamlRoot,
                };

                var result = await dialog.ShowAsync();
                return result == ContentDialogResult.Primary;
            }

            return true;
        }

        private List<string> ParseRelatedStocks()
        {
            // Split the comma-separated input into a cleaned list
            if (string.IsNullOrWhiteSpace(this.RelatedStocksText))
            {
                return new List<string>();
            }

            return this.RelatedStocksText
                .Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }

        private NewsArticle CreateArticle()
        {
            return new NewsArticle
            {
                ArticleId = Guid.NewGuid().ToString(),
                Title = this.Title,
                Summary = this.Summary,
                Content = this.Content,
                Source = $"User: {this.appState.CurrentUser?.CNP ?? "Anonymous"}",
                PublishedDate = DateTime.Now.ToString("MMMM dd, yyyy"),
                IsRead = false,
                IsWatchlistRelated = false,
                Category = this.SelectedTopic,
                RelatedStocks = this.ParseRelatedStocks(),
            };
        }

        /// <summary>
        /// Displays an error message dialog with the specified text.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private async void ShowErrorDialog(string message)
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
                // If dialog fails, write to debug output
                System.Diagnostics.Debug.WriteLine($"Error showing error dialog: {ex.Message}");
            }
        }
    }
}
