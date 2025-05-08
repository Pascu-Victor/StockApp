using System.Threading.Tasks;

namespace StockApp.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public interface IGemStoreRepository
    {
        string GetCnp();

        Task<int> GetUserGemBalanceAsync(string cnp);

        Task UpdateUserGemBalanceAsync(string cnp, int newBalance);

        Task<bool> IsGuestAsync(string cnp);
    }
}
