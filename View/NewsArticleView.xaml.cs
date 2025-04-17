using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using StockApp.StockPage;
using StockNewsPage.ViewModels;
using System.Linq;
using StockApp.Service;

namespace StockNewsPage.Views
{
    public sealed partial class NewsArticleView : Page
    {
        public ModelView ViewModel { get; } = new ModelView();

        public NewsArticleView()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Article == null)
                return;

            // Using null-conditional operator to safely access RelatedStocks
            ViewModel.HasRelatedStocks = ViewModel.Article.RelatedStocks?.Any() ?? false;
        }

        private void RelatedStockClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is string stockName)
            {
                NavigationService.Instance.Navigate(typeof(StockPage), stockName);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string articleId)
            {
                ViewModel.LoadArticle(articleId);
            }
            else
            {
                throw new ArgumentException("Navigation parameter must be a string article ID", nameof(e));
            }
        }
    }
}