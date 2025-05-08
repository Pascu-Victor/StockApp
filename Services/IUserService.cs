namespace StockApp.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;

    public interface IUserService
    {
        User GetUserByCnp(string cnp);

        Task<List<User>> GetUsers();
    }
}
