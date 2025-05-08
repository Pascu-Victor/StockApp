namespace StockApp.Repositories
{
    using System.Threading.Tasks;

    public interface IGemStoreRepository
    {
        Task<int> GetUserGemBalanceAsync(string cnp);

        Task UpdateUserGemBalanceAsync(string cnp, int newBalance);

    }
}
