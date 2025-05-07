using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly HistoryApiService _apiService;

        public HistoryService(HistoryApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        public List<CreditScoreHistory> GetHistoryForUser(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                // Since we're using async methods in the API service, we need to run this synchronously
                return Task.Run(() => _apiService.GetHistoryForUserAsync(userCnp)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving credit score history", ex);
            }
        }

        public async Task<List<CreditScoreHistory>> GetHistoryForUserAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return await _apiService.GetHistoryForUserAsync(userCnp);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving credit score history", ex);
            }
        }

        public async Task<CreditScoreHistory> AddHistoryEntryAsync(CreditScoreHistory history)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            try
            {
                return await _apiService.AddHistoryEntryAsync(history);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding credit score history entry", ex);
            }
        }

        public async Task<bool> DeleteHistoryEntryAsync(int id)
        {
            try
            {
                await _apiService.DeleteHistoryEntryAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting credit score history entry", ex);
            }
        }

        public List<CreditScoreHistory> GetHistoryWeekly(string userCnp)
        {
            var history = GetHistoryForUser(userCnp);
            var oneWeekAgo = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
            return history.Where(h => h.Date >= oneWeekAgo).ToList();
        }

        public List<CreditScoreHistory> GetHistoryMonthly(string userCnp)
        {
            var history = GetHistoryForUser(userCnp);
            var oneMonthAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
            return history.Where(h => h.Date >= oneMonthAgo).ToList();
        }

        public List<CreditScoreHistory> GetHistoryYearly(string userCnp)
        {
            var history = GetHistoryForUser(userCnp);
            var oneYearAgo = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
            return history.Where(h => h.Date >= oneYearAgo).ToList();
        }
    }
}
