namespace StockApp.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using StockApp.Models;

    public class UserRepoProxy : IUserRepoProxy
    {
        private readonly HttpClient _httpClient;

        public UserRepoProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<User>>("api/User") ?? new List<User>();
        }

        public async Task<User> GetUserByCnpAsync(string cnp)
        {
            return await _httpClient.GetFromJsonAsync<User>($"api/User/{cnp}")
                ?? throw new KeyNotFoundException();
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _httpClient.GetFromJsonAsync<User>($"api/User/username/{username}")
                ?? throw new KeyNotFoundException();
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("api/User", user);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/User/{user.CNP}", user);
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

        public async Task<bool> IncrementOffensesCountAsync(string cnp)
        {
            var response = await _httpClient.PostAsync($"api/User/{cnp}/increment-offenses", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserCreditScoreAsync(string cnp, int creditScore)
        {
            var response = await _httpClient.PostAsync($"api/User/{cnp}/update-creditscore/{creditScore}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserROIAsync(string cnp, decimal roi)
        {
            var response = await _httpClient.PostAsync($"api/User/{cnp}/update-roi/{roi}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUserRiskScoreAsync(string cnp, int riskScore)
        {
            var response = await _httpClient.PostAsync($"api/User/{cnp}/update-riskscore/{riskScore}", null);
            return response.IsSuccessStatusCode;
        }
    }
}
