namespace StockApp.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;
    public interface IUserApiService
    {
        Task<User?> GetUserByCnpAsync(string cnp);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<List<User>> GetAllUsersAsync();
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(string cnp, User user);
        Task<bool> DeleteUserAsync(string cnp);
        Task<bool> PenalizeUserAsync(string cnp, int amount);
        Task<bool> IncrementOffensesAsync(string cnp);
        Task<bool> UpdateCreditScoreAsync(string cnp, int creditScore);
        Task<bool> UpdateROIAsync(string cnp, decimal roi);
        Task<bool> UpdateRiskScoreAsync(string cnp, int riskScore);
    }
}
