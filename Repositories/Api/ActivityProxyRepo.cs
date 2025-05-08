namespace StockApp.Repositories.Api
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using StockApp.Models;

    public class ActivityProxyRepo : IActivityRepo
    {
        private readonly HttpClient _httpClient;

        public ActivityProxyRepo(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<ActivityLog>> GetActivityForUser(string userCnp)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ActivityLog>>($"api/Activity/user/{userCnp}");
                return response ?? new List<ActivityLog>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error retrieving activities for user {userCnp}", ex);
            }
        }

        public async Task<ActivityLog> AddActivity(ActivityLog activityLog)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Activity", activityLog);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<ActivityLog>()
                    ?? throw new Exception("Failed to deserialize response");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error adding activity for user {activityLog.UserCnp}", ex);
            }
        }

        public async Task<List<ActivityLog>> GetAllActivities()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ActivityLog>>("api/Activity");
                return response ?? new List<ActivityLog>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error retrieving all activities", ex);
            }
        }

        public async Task<ActivityLog> GetActivityById(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ActivityLog>($"api/Activity/{id}");
                return response ?? throw new KeyNotFoundException($"Activity with ID {id} not found");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error retrieving activity with ID {id}", ex);
            }
        }

        public async Task<bool> DeleteActivity(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Activity/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error deleting activity with ID {id}", ex);
            }
        }
    }
}