using System.Collections.Generic;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Services
{
    public interface ITipsService
    {
        Task GiveTipToUserAsync(string userCNP);
        Task<List<Tip>> GetTipsForGivenUserAsync(string userCnp);
    }
}
