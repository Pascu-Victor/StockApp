using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StockApp.Database;
using StockApp.Models;

namespace StockApp.Repositories
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly AppDbContext _context;

        public HistoryRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public List<CreditScoreHistory> GetHistoryForUser(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty");
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
                throw new Exception("Error retrieving credit score history", ex);
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