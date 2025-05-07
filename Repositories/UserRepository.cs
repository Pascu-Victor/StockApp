namespace StockApp.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using StockApp.Database;
    using StockApp.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public string CurrentUserCNP { get; set; }

        public UserRepository(AppDbContext context)
        {
            _context = context;
            CurrentUserCNP = "default-cnp"; // You can override from appsettings if needed
        }

        public async Task CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserByCnpAsync(string userCNP)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.CNP == userCNP)
                ?? throw new KeyNotFoundException($"User with CNP {userCNP} not found.");
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username)
                ?? throw new KeyNotFoundException($"User with username {username} not found.");
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(string userCNP)
        {
            var user = await GetUserByCnpAsync(userCNP);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task PenalizeUserAsync(string cnp, int penaltyAmount)
        {
            var user = await GetUserByCnpAsync(cnp);
            user.CreditScore -= penaltyAmount;
            await _context.SaveChangesAsync();
        }

        public async Task IncrementOffensesCountAsync(string cnp)
        {
            var user = await GetUserByCnpAsync(cnp);
            user.NumberOfOffenses++;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserCreditScoreAsync(string cnp, int creditScore)
        {
            var user = await GetUserByCnpAsync(cnp);
            user.CreditScore = creditScore;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserROIAsync(string cnp, decimal roi)
        {
            var user = await GetUserByCnpAsync(cnp);
            user.ROI = roi;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserRiskScoreAsync(string cnp, int riskScore)
        {
            var user = await GetUserByCnpAsync(cnp);
            user.RiskScore = riskScore;
            await _context.SaveChangesAsync();
        }
    }
}
