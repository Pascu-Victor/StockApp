namespace StockApp.Repositories
{
    using System;
    using System.Collections.Generic;
    using Src.Model;
    using System.Threading.Tasks;

    public interface IChatReportRepository
    {
        List<ChatReport> GetChatReports();

        void DeleteChatReport(int id);

        void UpdateScoreHistoryForUser(string userCNP, int newScore);

        int GetNumberOfGivenTipsForUser(string reportedUserCnp);

        Task UpdateActivityLog(string reportedUserCnp, int amount);
    }
}
