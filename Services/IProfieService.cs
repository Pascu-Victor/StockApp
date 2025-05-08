using System.Collections.Generic;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Services
{
    public interface IProfileService
    {
        Task<Profile> GetProfileByCnpAsync(string cnp);
        Task<Profile> GetCurrentProfileAsync();
        Task<string> GenerateUsernameAsync();
        Task UpdateProfileAsync(string cnp, string newUsername, string newImage, string newDescription, bool newHidden);
        Task UpdateIsAdminAsync(string cnp, bool isAdmin);
        Task<List<Stock>> GetUserStocksAsync(string cnp);

        // Legacy methods for backward compatibility
        string GetImage();
        string GetUsername();
        string GetDescription();
        bool IsHidden();
        bool IsAdmin();
        List<Stock> GetUserStocks();
        void UpdateUser(string newUsername, string newImage, string newDescription, bool newHidden);
        void UpdateIsAdmin(bool isAdmin);
        string GetLoggedInUserCnp();
    }
}