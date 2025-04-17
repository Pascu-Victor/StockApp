namespace Alerts
{
    using System;
    using System.Collections.ObjectModel;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Models;
    using StockApp.ViewModel;

    public sealed partial class AlertsView : Page
    {
        private readonly AlertViewModel alertViewModel = new ();

        public AlertsView()
        {
            this.InitializeComponent();
            this.LoadAlerts();
        }

        private void LoadAlerts() => this.AlertsListView.ItemsSource = new ObservableCollection<Alert>(this.alertViewModel.Alerts);

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            Alert newAlert = this.alertViewModel.CreateAlert("Tesla", "New Alert", 100, 0, true);
            this.AlertsListView.SelectedItem = newAlert;
        }

        private async void MinusButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.AlertsListView.SelectedItem is Alert selectedAlert)
            {
                this.alertViewModel.DeleteAlert(selectedAlert.AlertId);

                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "Alert deleted successfully",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Please select an alert to delete",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                };

                await dialog.ShowAsync();
            }
        }

        private async void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var alert in this.alertViewModel.Alerts)
                {
                    if (alert.LowerBound > alert.UpperBound)
                    {
                        throw new Exception("Lower bound cannot be greater than upper bound");
                    }

                    this.alertViewModel.UpdateAlert(alert);
                }

                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "All alerts saved successfully",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot,
                };
                await dialog.ShowAsync();
            }
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}