using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StockApp.Repositories
{
    public class GemStoreProxyRepo : IGemStoreRepository
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7001/api/GemStore";

        public GemStoreProxyRepo(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<int> GetUserGemBalanceAsync(string cnp)
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/balance/{cnp}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task UpdateUserGemBalanceAsync(string cnp, int newBalance)
        {
            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/balance/{cnp}", newBalance);
            response.EnsureSuccessStatusCode();
        }

    }
}