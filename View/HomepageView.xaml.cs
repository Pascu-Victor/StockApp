﻿using CreateStock;
using GemStore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StockApp.Models;
using StockApp.Service;
using StockApp.StockPage;
using StockApp.ViewModel;
using StockNewsPage.Views;
using StocksApp;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace StocksHomepage
{
    public sealed partial class MainPage : Page
    {
        // add the view model as a property
        public HomepageViewModel ViewModel { get; }

        public MainPage()
        {
            this.InitializeComponent();
            ViewModel = new HomepageViewModel();
            DataContext = ViewModel;
        }

        private void FavoriteButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is HomepageStock stock)
            {
                ViewModel.FavoriteCommand.Execute(stock);
            }
        }

        private void CreateProfileClick(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as HomepageViewModel;

            if (viewModel != null)
            {
                viewModel.CreateUserProfile();
            }
        }

        private void SearchBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel.SearchQuery = SearchBox.Text;
        }

        private void SortDropdownSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortDropdown.SelectedItem is ComboBoxItem selectedItem)
            {
                ViewModel.SelectedSortOption = selectedItem.Content.ToString();
            }
        }

        public void GoToNews(object sender, RoutedEventArgs e)
        {
            NavigationService.Initialize(this.Frame);
            NavigationService.Instance.Navigate(typeof(NewsListView));
        }

        public void GoToCreateStock(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CreateStockPage), null);
        }

        public void GoToHistory(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(TransactionLog.TransactionLogView), null);
        }

        public void GoToProfile(object sender, RoutedEventArgs e)
        {
            NavigationService.Initialize(this.Frame);
            NavigationService.Instance.Navigate(typeof(ProfilePage), ViewModel.GetUserCNP());
        }

        public void GoToStore(object sender, RoutedEventArgs e)
        {
            NavigationService.Initialize(this.Frame);
            NavigationService.Instance.Navigate(typeof(GemStoreWindow), null);
        }

        public void GoToStock(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is HomepageStock stock)
            {
                // this.Frame.Navigate(typeof(StockPage), null);
                NavigationService.Initialize(this.Frame);
                NavigationService.Instance.Navigate(typeof(StockPage), stock.Name);
            }
        }
    }
}
