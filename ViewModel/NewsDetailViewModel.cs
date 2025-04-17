using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using System;
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
    public class ModelView : ViewModelBase
    {
        private readonly NewsService _newsService;
        private readonly DispatcherQueue _dispatcherQueue;
        private string _currentArticleId;
        private bool _isPreviewMode;
        private string _previewId;

        private NewsArticle _article;
        public NewsArticle Article
        {
            get => _article;
            set => SetProperty(ref _article, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _hasRelatedStocks;
        public bool HasRelatedStocks
        {
            get => _hasRelatedStocks;
            set => SetProperty(ref _hasRelatedStocks, value);
        }

        // admin preview mode properties
        private bool _isAdminPreview;
        public bool IsAdminPreview
        {
            get => _isAdminPreview;
            set => SetProperty(ref _isAdminPreview, value);
        }

        private string _articleStatus;
        public string ArticleStatus
        {
            get => _articleStatus;
            set => SetProperty(ref _articleStatus, value);
        }

        private bool _canApprove;
        public bool CanApprove
        {
            get => _canApprove;
            set => SetProperty(ref _canApprove, value);
        }

        private bool _canReject;
        public bool CanReject
        {
            get => _canReject;
            set => SetProperty(ref _canReject, value);
        }

        // commands
        public ICommand BackCommand { get; }
        public ICommand ApproveCommand { get; }
        public ICommand RejectCommand { get; }
        public ICommand DeleteCommand { get; }

        // constructor
        public ModelView()
        {
            _newsService = new NewsService();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // init commands
            BackCommand = new StockNewsRelayCommand(() => NavigationService.Instance.GoBack());
            ApproveCommand = new StockNewsRelayCommand(async () => await ApproveArticleAsync());
            RejectCommand = new StockNewsRelayCommand(async () => await RejectArticleAsync());
            DeleteCommand = new StockNewsRelayCommand(async () => await DeleteArticleAsync());
        }

        public async void LoadArticle(string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
                throw new ArgumentNullException(nameof(articleId));

            IsLoading = true;

            try
            {
                // if this is a preview
                if (articleId.StartsWith("preview:"))
                {
                    _isPreviewMode = true;
                    _previewId = articleId.Substring(8); // Remove "preview:"
                    _currentArticleId = _previewId;

                    // admin preview
                    IsAdminPreview = true;

                    var userArticle = _newsService.GetUserArticleForPreview(_previewId) 
                        ?? throw new InvalidOperationException($"User article with ID {_previewId} not found");
            
                    ArticleStatus = userArticle.Status;
                    CanApprove = userArticle.Status != "Approved";
                    CanReject = userArticle.Status != "Rejected";

                    var article = await _newsService.GetNewsArticleByIdAsync(articleId);
                    if (article != null)
                    {
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            Article = article;
                            HasRelatedStocks = article.RelatedStocks != null && article.RelatedStocks.Any();
                            System.Diagnostics.Debug.WriteLine($"Related stocks count: {article.RelatedStocks?.Count ?? 0}");
                            IsLoading = false;
                        });
                    }
                    else
                    {
                        // Article not found
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            Article = new NewsArticle
                            {
                                Title = "Article Not Found",
                                Summary = "The requested preview article could not be found.",
                                Content = "The preview article you are looking for may no longer be available."
                            };
                            HasRelatedStocks = false;
                            System.Diagnostics.Debug.WriteLine("Preview article not found");
                            IsLoading = false;
                        });
                    }
                }
                else
                {
                    _isPreviewMode = false;
                    _currentArticleId = articleId;
                    IsAdminPreview = false;

                    var article = await _newsService.GetNewsArticleByIdAsync(articleId);
                    if (article != null)
                    {
                        _dispatcherQueue.TryEnqueue(async () =>
                        {
                            Article = article;
                            HasRelatedStocks = article.RelatedStocks != null && article.RelatedStocks.Any();

                            // Mark as read if not in preview mode
                            if (!_isPreviewMode)
                            {
                                await _newsService.MarkArticleAsReadAsync(articleId);
                            }
                            IsLoading = false;
                        });
                    }
                    else
                    {
                        // Article not found
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            Article = new NewsArticle
                            {
                                Title = "Article Not Found",
                                Summary = "The requested article could not be found.",
                                Content = "The article you are looking for may have been removed or is no longer available."
                            };
                            IsLoading = false;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading article: {ex.Message}");

                _dispatcherQueue.TryEnqueue(() =>
                {
                    Article = new NewsArticle
                    {
                        Title = "Error Loading Article",
                        Summary = "There was an error loading the article.",
                        Content = "Please try again later or contact support if the problem persists."
                    };
                    IsLoading = false;
                });
            }
        }

        private async Task ApproveArticleAsync()
        {
            if (!_isPreviewMode || string.IsNullOrEmpty(_previewId))
                return;

            IsLoading = true;

            try
            {
                var success = await _newsService.ApproveUserArticleAsync(_previewId);
                if (success)
                {
                    // update the status locally
                    ArticleStatus = "Approved";
                    CanApprove = false;
                    CanReject = true;

                    // success message
                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Article has been approved and will now appear in the news list.",
                        CloseButtonText = "OK",
                        XamlRoot = App.CurrentWindow.Content.XamlRoot
                    };

                    await dialog.ShowAsync();

                    // nav back
                    NavigationService.Instance.GoBack();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error approving article: {ex.Message}");

                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to approve article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RejectArticleAsync()
        {
            if (!_isPreviewMode || string.IsNullOrEmpty(_previewId))
                return;

            IsLoading = true;

            try
            {
                var success = await _newsService.RejectUserArticleAsync(_previewId);
                if (success)
                {
                    // update status locally
                    ArticleStatus = "Rejected";
                    CanApprove = true;
                    CanReject = false;

                    // success message
                    var dialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Article has been rejected.",
                        CloseButtonText = "OK",
                        XamlRoot = App.CurrentWindow.Content.XamlRoot
                    };

                    await dialog.ShowAsync();

                    // nav back
                    NavigationService.Instance.GoBack();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error rejecting article: {ex.Message}");

                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to reject article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteArticleAsync()
        {
            if (!_isPreviewMode || string.IsNullOrEmpty(_previewId))
                return;

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
                    IsLoading = true;

                    var success = await _newsService.DeleteUserArticleAsync(_previewId);
                    if (success)
                    {
                        // success message
                        var dialog = new ContentDialog
                        {
                            Title = "Success",
                            Content = "Article has been deleted.",
                            CloseButtonText = "OK",
                            XamlRoot = App.CurrentWindow.Content.XamlRoot
                        };

                        await dialog.ShowAsync();

                        // nav back
                        NavigationService.Instance.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting article: {ex.Message}");

                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Failed to delete article. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = App.CurrentWindow.Content.XamlRoot
                };

                await dialog.ShowAsync();
            }
            finally
            {
                IsLoading = false;
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

