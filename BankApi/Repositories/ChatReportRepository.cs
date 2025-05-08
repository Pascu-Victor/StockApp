namespace StockApp.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;
    using BankApi.Data;
    using BankApi.Models;
    using Microsoft.EntityFrameworkCore;

    public class ChatReportRepository : IChatReportRepository
    {
        private readonly ApiDbContext _context;

        public ChatReportRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatReport>> GetAllChatReportsAsync()
        {
            return await _context.ChatReports.ToListAsync();
        }

        public async Task<ChatReport?> GetChatReportByIdAsync(int id)
        {
            return await _context.ChatReports.FindAsync(id);
        }

        public async Task<bool> AddChatReportAsync(ChatReport report)
        {
            try
            {
                _context.ChatReports.Add(report);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public async Task<bool> DeleteChatReportAsync(int id)
        {
            try
            {
                var report = await _context.ChatReports.FindAsync(id);
                if (report == null)
                    throw new Exception($"Chat report with id {id} not found.");

                _context.ChatReports.Remove(report);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

        }

        public async Task<int> GetNumberOfGivenTipsForUserAsync(string userCnp)
        {
            return await _context.GivenTips
                .Where(t => t.UserCnp == userCnp)
                .CountAsync();
        }

        public async Task UpdateActivityLogAsync(string userCnp, int amount)
        {
            var activity = await _context.ActivityLogs
                .FirstOrDefaultAsync(a => a.UserCnp == userCnp && a.ActivityName == "Chat");

            if (activity == null)
            {
                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserCnp = userCnp,
                    ActivityName = "Chat",
                    LastModifiedAmount = amount,
                    ActivityDetails = "Chat abuse"
                });
            }
            else
            {
                activity.LastModifiedAmount = amount;
                activity.ActivityDetails = "Chat abuse";
                _context.ActivityLogs.Update(activity);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateScoreHistoryForUserAsync(string userCnp, int newScore)
        {
            var today = DateTime.Today;

            var existing = await _context.CreditScoreHistories
                .FirstOrDefaultAsync(s => s.UserCnp == userCnp && s.Date == today);

            if (existing != null)
            {
                existing.Score = newScore;
                _context.CreditScoreHistories.Update(existing);
            }
            else
            {
                _context.CreditScoreHistories.Add(new CreditScoreHistory
                {
                    UserCnp = userCnp,
                    Date = today,
                    Score = newScore
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}