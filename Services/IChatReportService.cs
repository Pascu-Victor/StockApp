namespace StockApp.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Src.Model;

    public interface IChatReportService
    {
        Task DoNotPunishUser(ChatReport chatReportToBeSolved);

        Task PunishUser(ChatReport chatReportToBeSolved);

        Task<bool> IsMessageOffensive(string messageToBeChecked);

        Task UpdateHistoryForUser(string userCNP, int newScore);

        Task<List<ChatReport>> GetChatReports();

        Task DeleteChatReport(int id);
    }
}
