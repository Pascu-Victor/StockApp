using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApi.Data;
using BankApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankApi.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly ApiDbContext _context;
        private readonly ILogger<ActivityRepository> _logger;

        public ActivityRepository(ApiDbContext context, ILogger<ActivityRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ActivityLog>> GetActivityForUserAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return await _context.ActivityLogs
                    .Where(a => a.UserCnp == userCnp)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities for user {UserCnp}", userCnp);
                throw;
            }
        }

        public async Task<ActivityLog> AddActivityAsync(string userCnp, string activityName, int amount, string details)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            if (string.IsNullOrWhiteSpace(activityName))
                throw new ArgumentException("Activity name cannot be empty", nameof(activityName));
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than 0", nameof(amount));

            try
            {
                var activity = new ActivityLog
                {
                    UserCnp = userCnp,
                    ActivityName = activityName,
                    LastModifiedAmount = amount,
                    ActivityDetails = details,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.ActivityLogs.AddAsync(activity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Added new activity for user {UserCnp}", userCnp);
                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding activity for user {UserCnp}", userCnp);
                throw;
            }
        }

        public async Task<List<ActivityLog>> GetAllActivitiesAsync()
        {
            try
            {
                return await _context.ActivityLogs
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all activities");
                throw;
            }
        }

        public async Task<ActivityLog> GetActivityByIdAsync(int id)
        {
            try
            {
                var activity = await _context.ActivityLogs.FindAsync(id);
                if (activity == null)
                {
                    throw new KeyNotFoundException($"Activity with ID {id} not found");
                }
                return activity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity with ID {ActivityId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteActivityAsync(int id)
        {
            try
            {
                var activity = await _context.ActivityLogs.FindAsync(id);
                if (activity == null)
                {
                    return false;
                }

                _context.ActivityLogs.Remove(activity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity with ID {ActivityId}", id);
                throw;
            }
        }
    }
} 