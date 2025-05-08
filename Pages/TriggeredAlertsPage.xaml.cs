namespace StockApp.Pages
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Repositories;

    public sealed partial class TriggeredAlertsPage : Page
    {
        private readonly IAlertRepository alertRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggeredAlertsPage"/> class.
        /// </summary>
        public TriggeredAlertsPage(IAlertRepository alertRepository)
        {
            this.InitializeComponent();
            this.alertRepository = alertRepository ?? throw new ArgumentNullException(nameof(alertRepository));
            LoadAlerts();
        }

        private static void LoadAlerts()
        {
            //AlertListView.ItemsSource = _alertRepository.GetTriggeredAlerts();
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            LoadAlerts();
        }
    }
}
