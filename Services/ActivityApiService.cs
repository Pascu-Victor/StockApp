namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Src.Model;

    public class ActivityApiService : IActivityService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ActivityApiService> _logger;

        public ActivityApiService(HttpClient httpClient, ILogger<ActivityApiService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ActivityLog>> GetActivityForUserAsync(string userCnp)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ActivityLog>>($"api/activity/user/{userCnp}");
                return response ?? new List<ActivityLog>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activities for user {UserCnp}", userCnp);
                throw;
            }
        }

        public async Task<List<ActivityLog>> GetAllActivitiesAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ActivityLog>>("api/activity");
                return response ?? new List<ActivityLog>();
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
                var response = await _httpClient.GetFromJsonAsync<ActivityLog>($"api/activity/{id}");
                return response ?? throw new KeyNotFoundException($"Activity with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity {Id}", id);
                throw;
            }
        }

        public async Task<ActivityLog> CreateActivityAsync(ActivityLog activity)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/activity", activity);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ActivityLog>() 
                    ?? throw new InvalidOperationException("Failed to deserialize response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity");
                throw;
            }
        }

        public async Task DeleteActivityAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/activity/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity {Id}", id);
                throw;
            }
        }
    }
} 