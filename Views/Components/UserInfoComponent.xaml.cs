namespace StockApp.Views.Components
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Src.Data;
    using StockApp.Models;
    using StockApp.Repositories;
    using StockApp.Services;
    using StockApp.Views.Pages;

    public sealed partial class UserInfoComponent : Page
    {
        private readonly IActivityService activityService;
        private readonly IHistoryService historyService;
        public User User;

        public UserInfoComponent(IActivityService activityService, IHistoryService historyService)
        {
            this.InitializeComponent();
            this.activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
            this.historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        public void SetUserData(User userData)
        {
            this.User = userData;
            this.NameTextBlock.Text = $"{this.User.FirstName}  {this.User.LastName}";
            this.CNPTextBlock.Text = $"{this.User.CNP}";
            this.ScoreTextBlock.Text = $"Score: {this.User.CreditScore}";
        }

        private async void OnAnalysisClick(object sender, RoutedEventArgs e)
        {
            if (this.User != null)
            {
                var analysisWindow = new AnalysisWindow(this.User, this.activityService, this.historyService);
                analysisWindow.Activate();
            }
        }

        private async void OnTipHistoryClick(object seder, RoutedEventArgs e)
        {
            if (this.User != null)
            {
                DatabaseConnection dbConnection = new DatabaseConnection();
                TipHistoryWindow tipHistoryWindow = new TipHistoryWindow(this.User, new MessagesRepository(dbConnection), new TipsRepository(dbConnection));
                tipHistoryWindow.Activate();
            }
        }
    }
}
