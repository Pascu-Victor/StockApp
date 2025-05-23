using Common.Models;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StockApp.Services
{
    public class ActivityProxyService(HttpClient httpClient) : IProxyService, IActivityService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<ActivityLog> AddActivity(string userCnp, string activityName, int amount, string details)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Activity", new { userCnp, activityName, amount, details });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ActivityLog>() ?? throw new InvalidOperationException("Failed to deserialize activity response.");
        }

        public async Task<bool> DeleteActivity(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Activity/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<List<ActivityLog>> GetActivityForUser(string userCNP)
        {
            // The controller expects the userCNP from the claims, not as a query parameter for this specific endpoint.
            return await _httpClient.GetFromJsonAsync<List<ActivityLog>>($"api/Activity/user/{userCNP}") ?? throw new InvalidOperationException("Failed to deserialize user activities response.");
        }

        public async Task<ActivityLog> GetActivityById(int id)
        {
            return await _httpClient.GetFromJsonAsync<ActivityLog>($"api/Activity/{id}") ?? throw new InvalidOperationException("Failed to deserialize activity response.");
        }

        public async Task<List<ActivityLog>> GetAllActivities()
        {
            return await _httpClient.GetFromJsonAsync<List<ActivityLog>>("api/Activity/all") ?? throw new InvalidOperationException("Failed to deserialize activities response.");
        }
    }
}