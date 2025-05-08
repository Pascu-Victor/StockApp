namespace StockApp.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;

    public interface IUserRepository
    {
        string CurrentUserCNP { get; set; }

        bool IsGuest { get; }

        Task CreateUserAsync(User user);

        Task<User> GetUserByCnpAsync(string userCNP);

        Task<User> GetUserByUsernameAsync(string username);

        Task UpdateUserAsync(User user);

        Task DeleteUserAsync(string userCNP);

        Task<List<User>> GetAllUsersAsync();

        Task PenalizeUserAsync(string cnp, int penaltyAmount);

        Task IncrementOffensesCountAsync(string cnp);

        Task UpdateUserCreditScoreAsync(string cnp, int creditScore);

        Task UpdateUserROIAsync(string cnp, decimal roi);

        Task UpdateUserRiskScoreAsync(string cnp, int riskScore);
    }
}