namespace StockApp.Pages
{
    using System;
    using System.Windows.Input;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using StockApp.Commands;
    using StockApp.Models;
    using StockApp.Services;
    using StockApp.ViewModels;
    using StockApp.Repositories;
    using StockApp.Database;

    public sealed partial class ProfilePage : Page
    {
        public ProfilePageViewModel ViewModel { get; }

        private ICommand UpdateProfileButton { get; }

        /// <summary>
        /// Constructor for the ProfilePage class.
        /// </summary>
        public ProfilePage()
        {
            this.InitializeComponent();
            var dbContext = new AppDbContext();
            var profileRepository = new ProfileRepository(dbContext, "current_user_cnp"); // Replace with actual CNP
            var profileService = new ProfileService(profileRepository, "current_user_cnp"); // Replace with actual CNP
            ViewModel = new ProfilePageViewModel(profileService);
            this.UpdateProfileButton = new StockNewsRelayCommand(() => this.GoToUpdatePage());
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.ShowUserInformation();
            this.StocksListView.ItemsSource = this.ViewModel.GetUserStocks();

            if (this.ViewModel.IsHidden())
            {
                this.HideProfile();
            }

            this.UserStocksShowUsername();
        }

        private void ShowUserInformation()
        {
            this.UsernameTextBlock.Text = this.ViewModel.GetUsername();
            this.ProfileDescription.Text = this.ViewModel.GetDescription();
            this.ProfileImage.Source = this.ViewModel.ImageSource;
        }

        private void GoToUpdatePage()
        {
            NavigationService.Instance.Navigate(typeof(UpdateProfilePage), this.ViewModel.GetLoggedInUserCnp());
        }

        private void GetSelectedStock(object sender, RoutedEventArgs e)
        {
            if (this.StocksListView.SelectedItem is string selectedStock)
            {
                this.StockName.Text = selectedStock;
            }
            else
            {
                this.StockName.Text = "No stock selected";
            }
        }

        private void HideProfile()
        {
            this.StocksListView.Visibility = Visibility.Collapsed;
            this.ProfileDescription.Visibility = Visibility.Collapsed;
            this.ProfileImage.Visibility = Visibility.Collapsed;
            this.EnterStockButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handles the click event for the "Go Back" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService.Instance.GoBack();
        }

        /// <summary>
        /// Sets the text of the UsernameMyStocks TextBlock to the user's username.
        /// </summary>
        public void UserStocksShowUsername()
        {
            this.UsernameMyStocks.Text = this.ViewModel.GetUsername() + "'s STOCKS: ";
        }

        /// <summary>
        /// Handles the click event for the "Go To Stock" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void GoToStockButton(object sender, RoutedEventArgs e)
        {
            if (this.StocksListView.SelectedItem is Stock selectedStock)
            {
                NavigationService.Initialize(new FrameAdapter(this.Frame));
                NavigationService.Instance.Navigate(typeof(StockPage), selectedStock);
            }
            else
            {
                throw new InvalidOperationException("No stock selected");
            }
        }
    }
}
