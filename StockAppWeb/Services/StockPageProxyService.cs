using Common.Models;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StockAppWeb.Services
{
    public class StockPageProxyService(HttpClient httpClient) : IProxyService, IStockPageService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<bool> BuyStockAsync(string stockName, int quantity, string? userCNP = null)
        {
            var response = await _httpClient.PostAsJsonAsync("api/StockPage/buy", new { stockName, quantity });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<bool> GetFavoriteAsync(string stockName, string? userCNP = null)
        {
            return await _httpClient.GetFromJsonAsync<bool>($"api/StockPage/favorite/{stockName}");
        }

        public async Task<int> GetOwnedStocksAsync(string stockName, string? userCNP = null)
        {
            return await _httpClient.GetFromJsonAsync<int>($"api/StockPage/owned-stocks/{stockName}");
        }

        public async Task<List<int>> GetStockHistoryAsync(string stockName)
        {
            return await _httpClient.GetFromJsonAsync<List<int>>($"api/StockPage/history/{stockName}") ??
                throw new InvalidOperationException("Failed to deserialize stock history response.");
        }

        public async Task<UserStock> GetUserStockAsync(string stockName, string? userCNP = null)
        {
            return await _httpClient.GetFromJsonAsync<UserStock>($"api/StockPage/user-stock/{stockName}") ??
                throw new InvalidOperationException("Failed to deserialize user stock response.");
        }

        public async Task<bool> SellStockAsync(string stockName, int quantity, string? userCNP = null)
        {
            var response = await _httpClient.PostAsJsonAsync("api/StockPage/sell", new { stockName, quantity });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task ToggleFavoriteAsync(string stockName, bool state, string? userCNP = null)
        {
            var response = await _httpClient.PostAsJsonAsync("api/StockPage/favorite/toggle", new { stockName, state });
            response.EnsureSuccessStatusCode();
        }
    }
} 