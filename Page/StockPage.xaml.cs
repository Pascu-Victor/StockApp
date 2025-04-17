using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using StocksApp;
using Windows.UI.Core;
using LiveChartsCore.Drawing;
using System.Windows.Input;
using StockNewsPage.ViewModels;
using StockApp.Service;
using StockApp.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StockApp.StockPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StockPage : Microsoft.UI.Xaml.Controls.Page
    {
        private StockPageViewModel _viewModel;

        ICommand command { get; }


        public StockPage()
        {
            this.InitializeComponent();
            command = new StockNewsRelayCommand(() => AuthorButtonClick());
        }

        public void ReturnButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Instance.GoBack();
        }


        public void AuthorButtonClick()
        {
            // this.Frame.Navigate(typeof(AuthorPage.AuthorPage));

            //var newFrame = new Frame();
            //// link the current frame (This.Frame) to the new one and make all go back work as expected
            //this.Frame.Content = newFrame;
            // newFrame.Navigate(typeof(ProfilePage), _viewModel.getStockAuthor());
            NavigationService.Instance.Navigate(typeof(ProfilePage), _viewModel.GetStockAuthor());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Retrieve the stock name passed during navigation
            if (e.Parameter is string stockName)
            {
                _viewModel = new StockPageViewModel(stockName, PriceLabel, IncreaseLabel, OwnedStocks, StockChart);
                this.DataContext = _viewModel;
            }
        }

        public void FavoriteButtonClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleFavorite();
        }

        public void AlertsButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Instance.Navigate(typeof(Alerts.AlertWindow));
        }

        public void BuyButtonClick(object sender, RoutedEventArgs e) {
            int quantity = (int)QuantityInput.Value;
            bool r = _viewModel.BuyStock(quantity);
            QuantityInput.Value = 1;
            if (!r)
            {
                //var dialog = new ContentDialog
                //{
                //    Title = "Not enough gems",
                //    Content = "You don't have enough gems to buy this stock.",
                //    CloseButtonText = "OK"
                //};
                //dialog.ShowAsync();
                Console.WriteLine("Not enough Gems!");
            }
        }

        public void SellButtonClick(object sender, RoutedEventArgs e)
        {
            int quantity = (int)QuantityInput.Value;
            bool r = _viewModel.SellStock(quantity);
            QuantityInput.Value = 1;
            if (!r)
            {
                //var dialog = new ContentDialog
                //{
                //    Title = "Not enough stocks",
                //    Content = "You don't have enough stocks to sell.",
                //    CloseButtonText = "OK"
                //};

                //dialog.ShowAsync();
                Console.WriteLine("Not enough Stocks!");
            }
        }
    }
}
