namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Src.Model;
    using StockApp.Repositories;

    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _activityRepository;

        public ActivityService(IActivityRepository activityRepository)
        {
            _activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
        }

        public async Task<List<ActivityLog>> GetActivityForUserAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            return await _activityRepository.GetActivityForUserAsync(userCnp);
        }

        public async Task<List<ActivityLog>> GetAllActivitiesAsync()
        {
            return await _activityRepository.GetAllActivitiesAsync();
        }

        public async Task<ActivityLog> GetActivityByIdAsync(int id)
        {
            return await _activityRepository.GetActivityByIdAsync(id);
        }

        public async Task<ActivityLog> CreateActivityAsync(ActivityLog activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            await _activityRepository.AddActivityAsync(
                activity.UserCnp,
                activity.ActivityName,
                activity.LastModifiedAmount,
                activity.ActivityDetails);

            return activity;
        }

        public async Task DeleteActivityAsync(int id)
        {
            await _activityRepository.DeleteActivityAsync(id);
        }
    }
}
