namespace StockApp.Pages
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Models;
    using StockApp.ViewModels;

    public sealed partial class GemStoreWindow : Page
    {
        private readonly StoreViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="GemStoreWindow"/> class.
        /// </summary>
        public GemStoreWindow(StoreViewModel storeViewModel)
        {
            _viewModel = storeViewModel ?? throw new ArgumentNullException(nameof(storeViewModel));
            this.DataContext = _viewModel;
            this.InitializeComponent();
        }

        private async void OnBuyClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsGuest)
            {
                this.ShowErrorDialog("Guests are not allowed to buy gems.");
                return;
            }

            if (sender is Button button && button.CommandParameter is GemDeal selectedDeal)
            {
                ComboBox bankAccountDropdown = new ComboBox
                {
                    ItemsSource = StoreViewModel.GetUserBankAccounts(),
                    SelectedIndex = 0,
                };

                StackPanel dialogContent = new StackPanel();
                dialogContent.Children.Add(new TextBlock { Text = $"You are about to buy {selectedDeal.GemAmount} Gems for {selectedDeal.Price}€.\n\nSelect a Bank Account:" });
                dialogContent.Children.Add(bankAccountDropdown);

                ContentDialog confirmDialog = new ContentDialog
                {
                    Title = "Confirm Purchase",
                    Content = dialogContent,
                    PrimaryButtonText = "Buy",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.rootGrid.XamlRoot,
                };

                ContentDialogResult result = await confirmDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    if (bankAccountDropdown.SelectedItem is not string selectedAccount)
                    {
                        this.ShowErrorDialog("No bank account selected.");
                        return;
                    }

                    string purchaseResult = await _viewModel.BuyGemsAsync(selectedDeal, selectedAccount);
                    this.ShowSuccessDialog(purchaseResult);
                }
            }
            else
            {
                this.ShowErrorDialog("Please select a deal before buying.");
            }
        }

        private async void ShowErrorDialog(string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.rootGrid.XamlRoot,
            };
            await errorDialog.ShowAsync();
        }

        private async void ShowSuccessDialog(string message)
        {
            ContentDialog successDialog = new ContentDialog
            {
                Title = "Success",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.rootGrid.XamlRoot,
            };
            await successDialog.ShowAsync();
        }

        private async void OnSellClicked(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsGuest)
            {
                this.ShowErrorDialog("Guests are not allowed to sell gems.");
                return;
            }

            if (!int.TryParse(this.sellInput.Text, out int gemsToSell) || gemsToSell <= 0)
            {
                this.ShowErrorDialog("Enter a valid number of Gems.");
                return;
            }

            if (gemsToSell > _viewModel.UserGems)
            {
                this.ShowErrorDialog("Not enough Gems to sell.");
                return;
            }

            ComboBox bankAccountDropdown = new ComboBox
            {
                ItemsSource = StoreViewModel.GetUserBankAccounts(),
                SelectedIndex = 0,
            };

            StackPanel dialogContent = new StackPanel();
            dialogContent.Children.Add(new TextBlock { Text = $"You are about to sell {gemsToSell} Gems for {gemsToSell / 100.0}€.\n\nSelect a Bank Account from below:\n" });
            dialogContent.Children.Add(bankAccountDropdown);

            ContentDialog sellDialog = new ContentDialog
            {
                Title = "Confirm Sale",
                Content = dialogContent,
                PrimaryButtonText = "Sell",
                CloseButtonText = "Cancel",
                XamlRoot = this.rootGrid.XamlRoot,
            };

            ContentDialogResult result = await sellDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (bankAccountDropdown.SelectedItem is not string selectedAccount)
                {
                    this.ShowErrorDialog("No bank account selected.");
                    return;
                }

                string sellResult = await _viewModel.SellGemsAsync(gemsToSell, selectedAccount);
                this.ShowSuccessDialog(sellResult);
            }
        }
    }
}