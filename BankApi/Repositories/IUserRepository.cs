namespace BankApi.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;

    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByCnpAsync(string cnp);
        Task<User> GetUserByUsernameAsync(string username);
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(string cnp);
        Task PenalizeUserAsync(string cnp, int amount);
        Task IncrementOffensesCountAsync(string cnp);
        Task UpdateUserCreditScoreAsync(string cnp, int creditScore);
        Task UpdateUserROIAsync(string cnp, decimal roi);
        Task UpdateUserRiskScoreAsync(string cnp, int riskScore);
    }
}
