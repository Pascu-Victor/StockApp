using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApi.Data;
using BankApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankApi.Repositories
{
    public class BillSplitReportRepository : IBillSplitReportRepository
    {
        private readonly ApiDbContext _dbContext;
        private readonly ILogger<BillSplitReportRepository> _logger;

        public BillSplitReportRepository(ApiDbContext dbContext, ILogger<BillSplitReportRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<BillSplitReport>> GetAllReportsAsync()
        {
            try
            {
                return await _dbContext.BillSplitReports.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all bill split reports");
                throw;
            }
        }

        public async Task<BillSplitReport> GetReportByIdAsync(int id)
        {
            try
            {
                var report = await _dbContext.BillSplitReports.FindAsync(id);
                
                if (report == null)
                {
                    throw new KeyNotFoundException($"Bill split report with ID {id} not found");
                }
                
                return report;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error retrieving bill split report with ID {ReportId}", id);
                throw;
            }
        }

        public async Task<BillSplitReport> AddReportAsync(BillSplitReport report)
        {
            try
            {
                _dbContext.BillSplitReports.Add(report);
                await _dbContext.SaveChangesAsync();
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding bill split report");
                throw;
            }
        }

        public async Task<BillSplitReport> UpdateReportAsync(BillSplitReport report)
        {
            try
            {
                var existingReport = await _dbContext.BillSplitReports.FindAsync(report.Id);
                
                if (existingReport == null)
                {
                    throw new KeyNotFoundException($"Bill split report with ID {report.Id} not found");
                }

                _dbContext.Entry(existingReport).CurrentValues.SetValues(report);
                await _dbContext.SaveChangesAsync();
                
                return existingReport;
            }
            catch (Exception ex) when (!(ex is KeyNotFoundException))
            {
                _logger.LogError(ex, "Error updating bill split report with ID {ReportId}", report.Id);
                throw;
            }
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            try
            {
                var report = await _dbContext.BillSplitReports.FindAsync(id);
                
                if (report == null)
                {
                    return false;
                }
                
                _dbContext.BillSplitReports.Remove(report);
                await _dbContext.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting bill split report with ID {ReportId}", id);
                throw;
            }
        }

        public async Task<int> GetCurrentBalanceAsync(string userCnp)
        {
            try
            {
                // For this example, we'll assume there's a Users table with a Balance column
                // This would typically be in a separate repository, but we're including it here
                // for compatibility with the original interface
                var user = await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT Balance FROM Users WHERE CNP = {0}", userCnp);
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current balance for user {UserCnp}", userCnp);
                throw;
            }
        }

        public async Task<float> SumTransactionsSinceReportAsync(string userCnp, DateTime sinceDate)
        {
            try
            {
                // This would typically query a TransactionLogs table
                var result = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT SUM(Amount) FROM TransactionLogs WHERE SenderCnp = {userCnp} AND TransactionDate > {sinceDate}");
                
                return (float)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error summing transactions since {SinceDate} for user {UserCnp}", sinceDate, userCnp);
                throw;
            }
        }

        public async Task<int> GetCurrentCreditScoreAsync(string userCnp)
        {
            try
            {
                // This would typically query a Users table with a CreditScore column
                var score = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT CreditScore FROM Users WHERE CNP = {userCnp}");
                
                return score;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credit score for user {UserCnp}", userCnp);
                throw;
            }
        }

        public async Task UpdateCreditScoreAsync(string userCnp, int newCreditScore)
        {
            try
            {
                // Update credit score
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Users SET CreditScore = {newCreditScore} WHERE CNP = {userCnp}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating credit score for user {UserCnp}", userCnp);
                throw;
            }
        }

        public async Task IncrementBillSharesPaidAsync(string userCnp)
        {
            try
            {
                // Increment the number of bill shares paid
                await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Users SET NumberOfBillSharesPaid = NumberOfBillSharesPaid + 1 WHERE CNP = {userCnp}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing bill shares paid for user {UserCnp}", userCnp);
                throw;
            }
        }
    }
} 