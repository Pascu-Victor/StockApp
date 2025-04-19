namespace StockApp.Pages
{
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Repositories;

    public sealed partial class TriggeredAlertsPage : Page
    {
        private readonly AlertRepository _alertRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggeredAlertsPage"/> class.
        /// </summary>
        public TriggeredAlertsPage()
        {
            this.InitializeComponent();
            _alertRepository = new AlertRepository();
            LoadAlerts();
        }

        private void LoadAlerts()
        {
            //AlertListView.ItemsSource = _alertRepository.GetTriggeredAlerts();
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            LoadAlerts();
        }
    }
}
