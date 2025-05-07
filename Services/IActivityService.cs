namespace StockApp.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Src.Model;

    public interface IActivityService
    {
        Task<List<ActivityLog>> GetActivityForUserAsync(string userCnp);
        Task<List<ActivityLog>> GetAllActivitiesAsync();
        Task<ActivityLog> GetActivityByIdAsync(int id);
        Task<ActivityLog> CreateActivityAsync(ActivityLog activity);
        Task DeleteActivityAsync(int id);
    }
}
