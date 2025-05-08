namespace StockApp.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;

    public interface IUserService
    {
        User GetUserByCnp(string cnp);

        Task<User> GetCurrentUserAsync();

        string GetCurrentUserCNP();

        bool IsGuest();

        Task<List<User>> GetUsers();
    }
}
