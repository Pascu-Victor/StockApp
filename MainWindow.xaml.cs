namespace StockApp
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Navigation;
    using StockApp.Database;
    using StockApp.Services;
    using StockApp.Views;

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            DatabaseHelper.InitializeDatabase();

            //rootFrame.Navigate(typeof(CreateStockPage), null);
            // rootFrame.Navigate(typeof(ProfilePage), null);
            // rootFrame.Navigate(typeof(MainPage), null);

            //string stockName = "Tesla";
            //rootFrame.Navigate(typeof(StockPage.StockPage), stockName);  

            // rootFrame.Navigate(typeof(CreateStockPage.MainPage), null);

            rootFrame.Navigate(typeof(HomepageView), null);
            NavigationService.Initialize(rootFrame);

            // string stockName = "Tesla";

            // NavigationService.Instance.Initialize(rootFrame);
            // NavigationService.Instance.Navigate(typeof(StockPage.StockPage), stockName);
            //rootFrame.Navigate(typeof(StockPage.StockPage), stockName);  

            // rootFrame.Navigate(typeof(CreateStockPage.MainPage), null);

            //rootFrame.Navigate(typeof(StocksHomepage.MainPage), null);

            // rootFrame.Navigate(typeof(Test.TestPage), null);


            // <news>
            // NavigationService.Instance.Initialize(rootFrame);
            // NavigationService.Instance.Navigate(typeof(NewsListView));

            // GEM STORE:
            //rootFrame.Navigate(typeof(GemStore.GemStoreWindow), null);

            // TRANSACTION LOG:
            //rootFrame.Navigate(typeof(TransactionLog.TransactionLogView), null);

            // Alerts
            //rootFrame.Navigate(typeof(Alerts.AlertWindow), null);
        }
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page: " + e.SourcePageType.FullName);
        }
    }
}