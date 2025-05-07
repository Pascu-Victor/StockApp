namespace StockApp.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using StockApp.Database;
    using Src.Model;

    public class ActivityRepository : IActivityRepository
    {
        private readonly AppDbContext _dbContext;

        public ActivityRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddActivityAsync(string userCnp, string activityName, int amount, string details)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            
            if (string.IsNullOrWhiteSpace(activityName))
                throw new ArgumentException("Activity name cannot be empty", nameof(activityName));
            
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0", nameof(amount));

            var activity = new ActivityLog
            {
                UserCnp = userCnp,
                ActivityName = activityName,
                LastModifiedAmount = amount,
                ActivityDetails = details,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.ActivityLogs.AddAsync(activity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<ActivityLog>> GetActivityForUserAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));

            return await _dbContext.ActivityLogs
                .Where(a => a.UserCnp == userCnp)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<ActivityLog>> GetAllActivitiesAsync()
        {
            return await _dbContext.ActivityLogs
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<ActivityLog> GetActivityByIdAsync(int id)
        {
            return await _dbContext.ActivityLogs
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task DeleteActivityAsync(int id)
        {
            var activity = await _dbContext.ActivityLogs.FindAsync(id);
            if (activity != null)
            {
                _dbContext.ActivityLogs.Remove(activity);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}