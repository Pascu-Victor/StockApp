namespace StockApp.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using StockApp.Database;
    using StockApp.Exceptions;
    using StockApp.Models;

    public class HistoryRepository : IHistoryRepository
    {
        private readonly AppDbContext _context;

        public HistoryRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public List<CreditScoreHistory> GetAllHistory()
        {
            try
            {
                return _context.CreditScoreHistories
                    .OrderByDescending(h => h.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving all credit score history", ex);
            }
        }

        public CreditScoreHistory GetHistoryById(int id)
        {
            try
            {
                return _context.CreditScoreHistories
                    .FirstOrDefault(h => h.Id == id)
                    ?? throw new KeyNotFoundException($"History entry with ID {id} not found.");
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException($"Error retrieving history entry with ID {id}", ex);
            }
        }

        public void AddHistory(CreditScoreHistory history)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            if (history.Score < 0 || history.Score > 1000)
            {
                throw new HistoryServiceException("Credit score must be between 0 and 1000");
            }

            try
            {
                _context.CreditScoreHistories.Add(history);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error adding credit score history entry", ex);
            }
        }

        public void UpdateHistory(CreditScoreHistory history)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            if (history.Score < 0 || history.Score > 1000)
            {
                throw new HistoryServiceException("Credit score must be between 0 and 1000");
            }

            try
            {
                var existingHistory = _context.CreditScoreHistories.Find(history.Id);
                if (existingHistory == null)
                {
                    throw new KeyNotFoundException($"History entry with ID {history.Id} not found.");
                }

                existingHistory.UserCnp = history.UserCnp;
                existingHistory.Date = history.Date;
                existingHistory.Score = history.Score;

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException($"Error updating history entry with ID {history.Id}", ex);
            }
        }

        public void DeleteHistory(int id)
        {
            try
            {
                var history = _context.CreditScoreHistories.Find(id);
                if (history == null)
                {
                    throw new KeyNotFoundException($"History entry with ID {id} not found.");
                }

                _context.CreditScoreHistories.Remove(history);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException($"Error deleting history entry with ID {id}", ex);
            }
        }

        public List<CreditScoreHistory> GetHistoryForUser(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return _context.CreditScoreHistories
                    .Where(h => h.UserCnp == userCnp)
                    .OrderByDescending(h => h.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving credit score history for user", ex);
            }
        }

        public List<CreditScoreHistory> GetHistoryWeekly(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                var oneWeekAgo = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
                return _context.CreditScoreHistories
                    .Where(h => h.UserCnp == userCnp && h.Date >= oneWeekAgo)
                    .OrderByDescending(h => h.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving weekly credit score history", ex);
            }
        }

        public List<CreditScoreHistory> GetHistoryMonthly(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                var oneMonthAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
                return _context.CreditScoreHistories
                    .Where(h => h.UserCnp == userCnp && h.Date >= oneMonthAgo)
                    .OrderByDescending(h => h.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving monthly credit score history", ex);
            }
        }

        public List<CreditScoreHistory> GetHistoryYearly(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                var oneYearAgo = DateOnly.FromDateTime(DateTime.Now.AddYears(-1));
                return _context.CreditScoreHistories
                    .Where(h => h.UserCnp == userCnp && h.Date >= oneYearAgo)
                    .OrderByDescending(h => h.Date)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new HistoryServiceException("Error retrieving yearly credit score history", ex);
            }
        }

        public async Task<List<CreditScoreHistory>> GetHistoryForUserAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty");
            }

            try
            {
                return await _context.CreditScoreHistories
                    .Where(h => h.UserCnp == userCnp)
                    .OrderByDescending(h => h.Date)
                    .ToListAsync();
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
                _context.CreditScoreHistories.Add(history);
                await _context.SaveChangesAsync();
                return history;
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
                var history = await _context.CreditScoreHistories.FindAsync(id);
                if (history == null)
                {
                    return false;
                }

                _context.CreditScoreHistories.Remove(history);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting credit score history entry", ex);
            }
        }
    }
}