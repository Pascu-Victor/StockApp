namespace StockApp.Repositories.Api
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Src.Model;
    using HttpClient = System.Net.Http.HttpClient;

    public class ChatReportRepoProxy : IChatReportRepository
    {
        private readonly HttpClient _httpClient;

        public ChatReportRepoProxy(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ChatReport>> GetAllChatReportsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ChatReport>>("api/ChatReport") ?? new List<ChatReport>();
        }

        public async Task<ChatReport?> GetChatReportByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ChatReport>($"api/ChatReport/{id}");
            }
            catch (HttpRequestException)
            {
                return null; // Optionally log the error or rethrow
            }
        }

        public async Task AddChatReportAsync(ChatReport report) // Fixed return type to match interface
        {
            var response = await _httpClient.PostAsJsonAsync("api/ChatReport", report);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteChatReportAsync(int id) // Fixed return type to match interface
        {
            var response = await _httpClient.DeleteAsync($"api/ChatReport/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> DoNotPunishUser(ChatReport chatReport)
        {
            var response = await _httpClient.PostAsJsonAsync("api/ChatReport/do-not-punish", chatReport);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PunishUser(ChatReport chatReport)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/ChatReport/punish", chatReport);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException error)
            {
                Console.WriteLine(error.Message);
                return false;
            }
        }

        public async Task<int> GetNumberOfGivenTipsForUserAsync(string userCnp)
        {
            try
            {
                if (string.IsNullOrEmpty(userCnp))
                {
                    System.Diagnostics.Debug.WriteLine("GetNumberOfGivenTipsForUserAsync called with null or empty userCnp");
                    return 0;
                }

                var response = await _httpClient.GetAsync($"api/ChatReport/tips-count/{userCnp}");
                
                if (response == null)
                {
                    System.Diagnostics.Debug.WriteLine("NULL RESPONSE from tips-count API call");
                    return 0;
                }

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Error response from tips-count API: {response.StatusCode}");
                    return 0;
                }
                
                var count = await response.Content.ReadFromJsonAsync<int>();
                return count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in GetNumberOfGivenTipsForUserAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task UpdateActivityLogAsync(string userCnp, int amount)
        {
            try
            {
                if (string.IsNullOrEmpty(userCnp))
                {
                    System.Diagnostics.Debug.WriteLine("UpdateActivityLogAsync called with null or empty userCnp");
                    return;
                }

                var payload = new { UserCnp = userCnp, Amount = amount };
                var response = await _httpClient.PostAsJsonAsync("api/ChatReport/activity-log", payload);
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating activity log: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in UpdateActivityLogAsync: {ex.Message}");
            }
        }

        public async Task UpdateScoreHistoryForUserAsync(string userCnp, int newScore)
        {
            var payload = new { userCnp, newScore };
            var response = await _httpClient.PostAsJsonAsync("api/ChatReport/update-score", payload);
            response.EnsureSuccessStatusCode();
        }
    }
}
