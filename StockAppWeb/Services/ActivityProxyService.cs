using Common.Models;
using Common.Services;

namespace StockAppWeb.Services
{
    public class ActivityProxyService : IActivityService
    {
        private readonly HttpClient _httpClient;

        public ActivityProxyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ActivityLog>> GetActivityForUser(string userCNP)
        {
            var response = await _httpClient.GetFromJsonAsync<List<ActivityLog>>($"api/Activity/user/{userCNP}");
            return response ?? new List<ActivityLog>();
        }

        public async Task<ActivityLog> AddActivity(string userCnp, string activityName, int amount, string details)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Activity", new
            {
                UserCnp = userCnp,
                ActivityName = activityName,
                LastModifiedAmount = amount,
                ActivityDetails = details
            });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ActivityLog>() ?? throw new Exception("Failed to deserialize activity log");
        }

        public async Task<List<ActivityLog>> GetAllActivities()
        {
            var response = await _httpClient.GetFromJsonAsync<List<ActivityLog>>("api/Activity");
            return response ?? new List<ActivityLog>();
        }

        public async Task<ActivityLog> GetActivityById(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<ActivityLog>($"api/Activity/{id}");
            return response ?? throw new Exception($"Activity with ID {id} not found");
        }

        public async Task<bool> DeleteActivity(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Activity/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}