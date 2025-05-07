using System.Collections.Generic;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Repositories
{
    public interface IHistoryRepository
    {
        List<CreditScoreHistory> GetHistoryForUser(string userCNP);
        Task<List<CreditScoreHistory>> GetHistoryForUserAsync(string userCNP);
        Task<CreditScoreHistory> AddHistoryEntryAsync(CreditScoreHistory history);
        Task<bool> DeleteHistoryEntryAsync(int id);
    }
}
