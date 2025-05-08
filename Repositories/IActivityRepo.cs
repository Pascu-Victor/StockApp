namespace StockApp.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;

    public interface IActivityRepo
    {
        Task<List<ActivityLog>> GetActivityForUser(string userCNP);

        Task<ActivityLog> AddActivity(ActivityLog activity);

        Task<List<ActivityLog>> GetAllActivities();

        Task<ActivityLog> GetActivityById(int id);

        Task<bool> DeleteActivity(int id);
    }
}
