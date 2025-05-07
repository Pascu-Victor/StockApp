namespace StockApp.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Src.Model;

    public interface IActivityRepository
    {
        Task AddActivityAsync(string userCnp, string activityName, int amount, string details);
        Task<List<ActivityLog>> GetActivityForUserAsync(string userCnp);
        Task<List<ActivityLog>> GetAllActivitiesAsync();
        Task<ActivityLog> GetActivityByIdAsync(int id);
        Task DeleteActivityAsync(int id);
    }
}
