namespace StockApp.Services
{
    using System.Collections.Generic;
    using StockApp.Models;

    public interface IHistoryService
    {
        List<CreditScoreHistory> GetAllHistory();
        CreditScoreHistory GetHistoryById(int id);
        void AddHistory(CreditScoreHistory history);
        void UpdateHistory(CreditScoreHistory history);
        void DeleteHistory(int id);
        List<CreditScoreHistory> GetHistoryForUser(string userCnp);
        List<CreditScoreHistory> GetHistoryWeekly(string userCnp);
        List<CreditScoreHistory> GetHistoryMonthly(string userCnp);
        List<CreditScoreHistory> GetHistoryYearly(string userCnp);
    }
}
