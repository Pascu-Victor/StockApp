using System.Collections.Generic;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Services
{
    public interface IHistoryService
    {
        List<CreditScoreHistory> GetHistoryForUser(string userCnp);
        Task<List<CreditScoreHistory>> GetHistoryForUserAsync(string userCnp);
        Task<CreditScoreHistory> AddHistoryEntryAsync(CreditScoreHistory history);
        Task<bool> DeleteHistoryEntryAsync(int id);
        
        // Time-based history methods
        List<CreditScoreHistory> GetHistoryWeekly(string userCnp);
        List<CreditScoreHistory> GetHistoryMonthly(string userCnp);
        List<CreditScoreHistory> GetHistoryYearly(string userCnp);
    }
}
