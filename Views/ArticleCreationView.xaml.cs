namespace StockApp.Views
{
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;

    public sealed partial class ArticleCreationView : Page
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleCreationView"/> class.
        /// </summary>
        public ArticleCreationView(ArticleCreationViewModel articleCreationViewModel)
        {
            this.ViewModel = articleCreationViewModel ?? throw new System.ArgumentNullException(nameof(articleCreationViewModel));
            this.DataContext = this.ViewModel;
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model for the article creation view.
        /// </summary>
        public ArticleCreationViewModel ViewModel { get; }
    }
}