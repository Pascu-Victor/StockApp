namespace StockApp.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Src.Model;

    public interface IChatReportRepository
    {
        //List<ChatReport> GetChatReports();

        //void DeleteChatReport(int id);

        //void UpdateScoreHistoryForUser(string userCNP, int newScore);

        //int GetNumberOfGivenTipsForUser(string reportedUserCnp);

        //void UpdateActivityLog(string reportedUserCnp, int amount);

        Task<List<ChatReport>> GetAllChatReportsAsync();

        Task<ChatReport?> GetChatReportByIdAsync(int id);

        Task AddChatReportAsync(ChatReport report);

        Task DeleteChatReportAsync(int id);

        Task<int> GetNumberOfGivenTipsForUserAsync(string userCnp);

        Task UpdateActivityLogAsync(string userCnp, int amount);

        Task UpdateScoreHistoryForUserAsync(string userCnp, int newScore);
    }
}
