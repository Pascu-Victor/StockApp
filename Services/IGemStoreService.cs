using System.Threading.Tasks;

namespace StockApp.Services
{
    public interface IGemStoreService
    {
        string GetCnp();
        bool IsGuest(string cnp);
        int GetUserGemBalance(string cnp);
        void UpdateUserGemBalance(string cnp, int newBalance);
        Task<int> GetCurrentUserGemBalanceAsync();
        Task UpdateCurrentUserGemBalanceAsync(int newBalance);
        Task<bool> IsCurrentUserGuestAsync();
        Task InitializeGemStoreForCurrentUserAsync(int initialBalance = 0);
    }
} 