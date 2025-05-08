namespace StockApp.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;

    public interface IHistoryRepository
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
