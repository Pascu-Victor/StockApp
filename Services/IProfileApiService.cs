using System.Collections.Generic;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Services
{
    public interface IProfileApiService
    {
        Task<Profile> GetProfileByCnpAsync(string cnp);
        Task<Profile> GetCurrentProfileAsync();
        Task<string> GenerateUsernameAsync();
        Task UpdateProfileAsync(string cnp, string newUsername, string newImage, string newDescription, bool newHidden);
        Task UpdateIsAdminAsync(string cnp, bool isAdmin);
        Task<List<Stock>> GetUserStocksAsync(string cnp);
    }
} 