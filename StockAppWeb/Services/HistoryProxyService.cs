using Common.Models;
using Common.Services;
using System.Net.Http.Json;

namespace StockAppWeb.Services
{
    public class HistoryProxyService : IHistoryService
    {
        private readonly HttpClient _httpClient;

        public HistoryProxyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task AddHistoryAsync(CreditScoreHistory history)
        {
            var response = await _httpClient.PostAsJsonAsync("api/history", history);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteHistoryAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/history/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<CreditScoreHistory>> GetAllHistoryAsync()
        {
            var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>("api/history");
            return response ?? new List<CreditScoreHistory>();
        }

        public async Task<CreditScoreHistory?> GetHistoryByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<CreditScoreHistory>($"api/history/{id}");
        }

        public async Task<List<CreditScoreHistory>> GetHistoryForUserAsync(string userCnp)
        {
            var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"api/history/user/{userCnp}");
            return response ?? new List<CreditScoreHistory>();
        }

        public async Task<List<CreditScoreHistory>> GetHistoryMonthlyAsync(string? userCnp = null)
        {
            if (string.IsNullOrEmpty(userCnp))
            {
                // Current user
                var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>("api/history/user/monthly");
                return response ?? new List<CreditScoreHistory>();
            }
            else
            {
                // Admin: get for specific user
                var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"api/history/user/{userCnp}/monthly");
                return response ?? new List<CreditScoreHistory>();
            }
        }

        public async Task<List<CreditScoreHistory>> GetHistoryWeeklyAsync(string userCnp)
        {
            var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"api/history/user/{userCnp}/weekly");
            return response ?? new List<CreditScoreHistory>();
        }

        public async Task<List<CreditScoreHistory>> GetHistoryYearlyAsync(string userCnp)
        {
            var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"api/history/user/{userCnp}/yearly");
            return response ?? new List<CreditScoreHistory>();
        }

        public async Task UpdateHistoryAsync(CreditScoreHistory history)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/history/{history.Id}", history);
            response.EnsureSuccessStatusCode();
        }
    }
} 