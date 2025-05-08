using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Services
{
    public interface IGemStoreApiService
    {
        Task<GemStore> GetGemStoreByCnpAsync(string cnp);
        Task<int> GetGemBalanceAsync(string cnp);
        Task<GemStore> UpdateGemBalanceAsync(string cnp, int newBalance);
        Task<GemStore> CreateGemStoreAsync(string cnp, int initialBalance = 0);
    }
} 