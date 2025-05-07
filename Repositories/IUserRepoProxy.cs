namespace StockApp.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;

    public interface IUserRepoProxy
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByCnpAsync(string cnp);
        Task<User> GetUserByUsernameAsync(string username);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(string cnp);
        Task<bool> PenalizeUserAsync(string cnp, int amount);
        Task<bool> IncrementOffensesCountAsync(string cnp);
        Task<bool> UpdateUserCreditScoreAsync(string cnp, int creditScore);
        Task<bool> UpdateUserROIAsync(string cnp, decimal roi);
        Task<bool> UpdateUserRiskScoreAsync(string cnp, int riskScore);
    }
}
