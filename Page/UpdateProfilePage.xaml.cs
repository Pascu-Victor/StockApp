using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Catel.MVVM;
using Microsoft.UI.Xaml.Navigation;
using StockApp.ViewModel;
using StockApp.Service;

namespace StocksApp
{
    public sealed partial class UpdateProfilePage : Page
    {
        private ModelView viewModelUpdate; 

        public UpdateProfilePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Retrieve the stock name passed during navigation
            if (e.Parameter is string authorCNP)
            {
                viewModelUpdate = new ModelView(authorCNP);
                this.DataContext = viewModelUpdate;
            }

        }

        private void GoToProfilePage(object sender, RoutedEventArgs e)
        {
            NavigationService.Instance.GoBack();
        }

        private async void GetAdminPassword(object sender, RoutedEventArgs e)
        {
            string userTryPass = PasswordTry.Text;
            bool isAdmin = viewModelUpdate.GetPassword() == userTryPass;
            viewModelUpdate.UpdateAdminMode(isAdmin);

            string message = isAdmin ? "You are now ADMIN!" : "Incorrect Password!";
            string title = isAdmin ? "Success" : "Error";
            ContentDialog dialog = CreateDialog(title, message);
            await dialog.ShowAsync();
        }

        private async void UpdateUserProfile(object sender, RoutedEventArgs e)
        {
            bool DescriptionEmpty = MyDescriptionCheckBox?.IsChecked == true;
            bool newHidden = MyCheckBox?.IsChecked == true;
            string newUsername = UsernameInput?.Text ?? string.Empty;
            string newImage = ImageInput?.Text ?? string.Empty;
            string newDescription = DescriptionInput?.Text ?? string.Empty;

            if (string.IsNullOrEmpty(newUsername) && string.IsNullOrEmpty(newImage) && string.IsNullOrEmpty(newDescription)
                && (MyCheckBox?.IsChecked == false && viewModelUpdate?.IsHidden() == false) && MyDescriptionCheckBox?.IsChecked == false)
            {
                await ShowErrorDialog("Please fill up at least one of the information fields");
                return;
            }

            if ((newUsername.Length < 8 || newUsername.Length > 24) && newUsername.Length != 0)
            {
                await ShowErrorDialog("Username must be 8-24 characters long.");
                return;
            }

            if(newDescription.Length > 100)
            {
                await ShowErrorDialog("The description should be max 100 characters long.");
                return;
            }

            if (string.IsNullOrEmpty(newUsername))
            {
                newUsername = viewModelUpdate?.GetUsername() ?? throw new InvalidOperationException("Username cannot be null");
            }

            if(DescriptionEmpty == false)
            {
                newDescription = viewModelUpdate?.GetDescription() ?? string.Empty;
            }

            if(string.IsNullOrEmpty(newImage))
            {
                newImage = viewModelUpdate?.GetImage() ?? string.Empty;
            }

            else if(DescriptionEmpty == true)
            {
                newDescription = string.Empty;
            }

            viewModelUpdate?.UpdateAll(newUsername, newImage, newDescription, newHidden);
            await ShowSuccessDialog("Profile updated successfully!");
        }

        private async Task ShowErrorDialog(string message)
        {
            ContentDialog dialog = CreateDialog("Error", message);
            await dialog.ShowAsync();
        }

        private async Task ShowSuccessDialog(string message)
        {
            ContentDialog dialog = CreateDialog("Success", message);
            await dialog.ShowAsync();
        }

        private ContentDialog CreateDialog(string title, string message)
        {
            return new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };
        }
    }
}
