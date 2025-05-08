namespace StockApp.Services
{
    using System.Threading.Tasks;
    using StockApp.Models;

    public interface IStoreService
    {
        string GetCnp();

        bool IsGuest(string cnp);

        int GetUserGemBalance(string cnp);

        void UpdateUserGemBalance(string cnp, int newBalance);

        Task<string> BuyGemsAsync(GemDeal deal, string selectedAccountId);

        Task<string> SellGemsAsync(int gemAmount, string selectedAccountId);
    }
}
