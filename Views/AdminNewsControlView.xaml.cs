namespace StockApp.Views
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.ViewModels;

    public sealed partial class AdminNewsControlView : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminNewsControlView"/> class.
        /// </summary>
        public AdminNewsControlView(AdminNewsViewModel adminNewsViewModel)
        {
            this.ViewModel = adminNewsViewModel ?? throw new ArgumentNullException(nameof(adminNewsViewModel));
            this.DataContext = this.ViewModel;
            this.InitializeComponent();
            this.Loaded += this.OnLoaded;
        }

        /// <summary>
        /// Gets the view model for the AdminNewsControlView.
        /// </summary>
        public AdminNewsViewModel ViewModel { get; }

        /// <summary>
        /// Handles the Loaded event of the AdminNewsControlView control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Initialize();
        }
    }
}
