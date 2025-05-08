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
                return await _httpClient.GetFromJsonAsync<int>($"api/ChatReport/tips-count/{userCnp}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching tip count: {ex.Message}");
                return 0;
            }
        }

        public async Task UpdateActivityLogAsync(string userCnp, int amount)
        {
            var payload = new { userCnp, amount };
            var response = await _httpClient.PostAsJsonAsync("api/ChatReport/update-log", payload);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateScoreHistoryForUserAsync(string userCnp, int newScore)
        {
            var payload = new { userCnp, newScore };
            var response = await _httpClient.PostAsJsonAsync("api/ChatReport/update-score", payload);
            response.EnsureSuccessStatusCode();
        }
    }
}
