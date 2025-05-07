namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using StockApp.Models;

    public class UserApiService : IUserApiService
    {
        private readonly HttpClient _httpClient;

        public UserApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var response = await _httpClient.GetAsync("api/User");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();
        }

        public async Task<User> GetUserByCnpAsync(string cnp)
        {
            var response = await _httpClient.GetAsync($"api/User/{cnp}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>() ?? throw new Exception("User not found.");
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var response = await _httpClient.GetAsync($"api/User/username/{username}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>() ?? throw new Exception("User not found.");
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("api/User", user);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserAsync(string cnp, User user)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/User/{cnp}", user);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(string cnp)
        {
            var response = await _httpClient.DeleteAsync($"api/User/{cnp}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PenalizeUserAsync(string cnp, int amount)
        {
            var response = await _httpClient.PostAsync($"api/User/{cnp}/penalize/{amount}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> IncrementOffensesAsync(string cnp)
        {
            var response = await _httpClient.PostAsync($"api/User/{cnp}/increment-offenses", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateCreditScoreAsync(string cnp, int creditScore)
        {
            var response = await _httpClient.PostAsync($"api/User/{cnp}/update-creditscore/{creditScore}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateROIAsync(string cnp, decimal roi)
        {
            var response = await _httpClient.PostAsync($"api/User/{cnp}/update-roi/{roi}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateRiskScoreAsync(string cnp, int riskScore)
        {
            var response = await _httpClient.PostAsync($"api/User/{cnp}/update-riskscore/{riskScore}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
