using System.Collections.Generic;
using System.Threading.Tasks;
using BankApi.Models;

namespace BankApi.Repositories
{
    public interface IActivityRepository
    {
        Task<List<ActivityLog>> GetActivityForUserAsync(string userCnp);
        Task<ActivityLog> AddActivityAsync(string userCnp, string activityName, int amount, string details);
        Task<List<ActivityLog>> GetAllActivitiesAsync();
        Task<ActivityLog> GetActivityByIdAsync(int id);
        Task<bool> DeleteActivityAsync(int id);
    }
} 