namespace StockApp.Views
{
    using System;
    using Microsoft.UI.Xaml.Controls;
    using Microsoft.UI.Xaml.Navigation;
    using StockApp.ViewModels;

    public sealed partial class AlertsView : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlertsView"/> class.
        /// </summary>
        public AlertsView(AlertViewModel viewModel)
        {
            this.ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.InitializeComponent();
            this.DataContext = this.ViewModel;
        }

        /// <summary>
        /// Gets or Sets the ViewModel for managing alerts.
        /// </summary>
        public AlertViewModel ViewModel { get; set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Retrieve the stock name passed during navigation
            if (e.Parameter is string stockName)
            {
                this.ViewModel.SelectedStockName = stockName;
            }
            else
            {
                throw new InvalidOperationException("Parameter is not of type string");
            }
        }

    }
}
