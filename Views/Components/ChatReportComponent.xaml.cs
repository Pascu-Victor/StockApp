namespace StockApp.Views.Components
{
    using System;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Src.Helpers;
    using Src.Model;
    using StockApp.Services;

    public sealed partial class ChatReportComponent : Page
    {
        private readonly IChatReportService chatReportService;

        public event EventHandler ReportSolved;

        public string ReportedUserCNP { get; set; }

        public string ReportedMessage { get; set; }

        public int ReportId { get; set; }

        public ChatReportComponent(IChatReportService chatReportService)
        {
            this.InitializeComponent();
            this.chatReportService = chatReportService;
        }

        private void PunishReportedUser(object sender, RoutedEventArgs e)
        {
            ChatReport chatReport = new ChatReport
            {
                Id = this.ReportId,
                ReportedUserCnp = this.ReportedUserCNP,
                ReportedMessage = this.ReportedMessage
            };

            this.chatReportService.PunishUser(chatReport);
            this.ReportSolved?.Invoke(this, EventArgs.Empty);
        }

        private void DoNotPunishReportedUser(object sender, RoutedEventArgs e)
        {
            ChatReport chatReport = new ChatReport
            {
                Id = this.ReportId,
                ReportedUserCnp = this.ReportedUserCNP,
                ReportedMessage = this.ReportedMessage
            };
            this.chatReportService.DoNotPunishUser(chatReport);
            this.ReportSolved?.Invoke(this, EventArgs.Empty);
        }

        public async void SetReportData(int id, string reportedUserCnp, string reportedMessage)
        {
            this.ReportId = id;
            this.ReportedUserCNP = reportedUserCnp;
            this.ReportedMessage = reportedMessage;

            bool apiSuggestion = await ProfanityChecker.IsMessageOffensive(reportedMessage);

            this.IdTextBlock.Text = $"Report ID: {id}";
            this.ReportedUserCNPTextBlock.Text = $"Reported user's CNP: {reportedUserCnp}";
            this.ReportedMessageTextBlock.Text = $"Message: {reportedMessage}";
            this.ApiSuggestionTextBlock.Text = apiSuggestion ? "The software marked this message as offensive" : "The software marked this message as inoffensive";
        }
    }
}
