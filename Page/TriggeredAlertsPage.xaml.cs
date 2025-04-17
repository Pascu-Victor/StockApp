using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using StockApp.Repository;
using StockApp.ViewModel;

namespace StockApp.Views
{
    public sealed partial class TriggeredAlertsPage : Microsoft.UI.Xaml.Controls.Page
    {
        private readonly AlertRepository _alertRepository;

        public TriggeredAlertsPage()
        {
            this.InitializeComponent();
            _alertRepository = new AlertRepository();
            LoadAlerts();
        }

        private void LoadAlerts()
        {
            AlertListView.ItemsSource = _alertRepository.GetTriggeredAlerts();
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            LoadAlerts();
        }
    }
}
