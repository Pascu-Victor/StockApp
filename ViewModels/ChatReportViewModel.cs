namespace StockApp.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Src.Model;
    using StockApp.Services;

    public class ChatReportsViewModel
    {
        private readonly IChatReportService chatReportService;

        public ObservableCollection<ChatReport> ChatReports { get; set; }

        public ChatReportsViewModel(IChatReportService chatReportService)
        {
            this.chatReportService = chatReportService;
            this.ChatReports = new ObservableCollection<ChatReport>();
        }

        public async Task LoadChatReportsAsync()
        {
            try
            {
                this.ChatReports.Clear();

                var reports = await this.chatReportService.GetChatReports();

                foreach (var report in reports)
                {
                    this.ChatReports.Add(report);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading reports: {ex.Message}");
            }
        }
    }
}
