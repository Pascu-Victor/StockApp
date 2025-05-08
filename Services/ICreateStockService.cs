namespace StockApp.Services
{
    using System.Threading.Tasks;

    public interface ICreateStockService
    {
        bool CheckIfUserIsGuest();

        Task<string> AddStock(string stockName, string stockSymbol, string authorCNP);
    }
}
