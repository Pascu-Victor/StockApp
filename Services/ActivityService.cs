namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;
    using StockApp.Repositories;

    /// <summary>
    /// Service for managing user activities.
    /// </summary>
    internal class ActivityService : IActivityService
    {
        private readonly IActivityRepo activityRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityService"/> class.
        /// </summary>
        /// <param name="activityRepo">The activity repository.</param>
        /// <exception cref="ArgumentNullException">Thrown when activityRepo is null.</exception>
        public ActivityService(IActivityRepo activityRepo)
        {
            this.activityRepo = activityRepo ?? throw new ArgumentNullException(nameof(activityRepo));
        }

        /// <inheritdoc/>
        public async Task<List<ActivityLog>> GetActivityForUser(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                return await this.activityRepo.GetActivityForUser(userCnp);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving activities for user {userCnp}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ActivityLog> AddActivity(string userCnp, string activityName, int amount, string details)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            if (string.IsNullOrWhiteSpace(activityName))
            {
                throw new ArgumentException("Activity name cannot be empty", nameof(activityName));
            }

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than 0", nameof(amount));
            }

            try
            {
                ActivityLog activity = new ActivityLog
                {
                    UserCnp = userCnp,
                    ActivityName = activityName,
                    LastModifiedAmount = amount,
                    ActivityDetails = details,
                };
                return await this.activityRepo.AddActivity(activity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding activity for user {userCnp}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<List<ActivityLog>> GetAllActivities()
        {
            try
            {
                return await this.activityRepo.GetAllActivities();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all activities", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<ActivityLog> GetActivityById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Activity ID must be greater than 0", nameof(id));
            }

            try
            {
                return await this.activityRepo.GetActivityById(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving activity with ID {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteActivity(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Activity ID must be greater than 0", nameof(id));
            }

            try
            {
                return await this.activityRepo.DeleteActivity(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting activity with ID {id}", ex);
            }
        }
    }
}
