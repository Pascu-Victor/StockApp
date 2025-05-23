using Common.Models;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;

namespace StockApp.Services
{
    public class StockProxyService(HttpClient httpClient) : IProxyService, IStockService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<Stock> CreateStockAsync(Stock stock)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Stock", stock);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Stock>() ?? throw new InvalidOperationException("Failed to deserialize stock response.");
        }

        public async Task<bool> DeleteStockAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/Stock/{id}");
            response.EnsureSuccessStatusCode();
            // Assuming API returns true/false in body for now.
            return response.StatusCode == System.Net.HttpStatusCode.NoContent || await response.Content.ReadFromJsonAsync<bool>();
        }

        public async Task<IEnumerable<Stock>> GetAllStocksAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Stock>>("api/Stock") ?? throw new InvalidOperationException("Failed to deserialize stocks response.");
        }

        public async Task<Stock?> GetStockByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Stock?>($"api/Stock/{id}");
        }
        public async Task<Stock?> GetStockByNameAsync(string name)
        {
            var allStocks = await this.GetAllStocksAsync();
            return allStocks.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Stock?> UpdateStockAsync(int id, Stock updatedStock)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Stock/{id}", updatedStock);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Stock?>();
        }

        public async Task<List<Stock>> UserStocksAsync(string? userCNP)
        {
            return userCNP == null
                ? await _httpClient.GetFromJsonAsync<List<Stock>>($"api/Stock/user") ?? throw new InvalidOperationException("Failed to deserialize user stocks response.")
                : await _httpClient.GetFromJsonAsync<List<Stock>>($"api/Stock/user/{userCNP}") ?? throw new InvalidOperationException("Failed to deserialize user stocks response.");
        }

        public async Task AddToFavoritesAsync(HomepageStock stock)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Stock/favorites/add", stock);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveFromFavoritesAsync(HomepageStock stock)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Stock/favorites/remove", stock);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<HomepageStock>> GetFilteredAndSortedStocksAsync(string query, string sortOption, bool favoritesOnly, string? userCNP = null)
        {
            var uriBuilder = new UriBuilder(_httpClient.BaseAddress + "api/Stock/stocks");
            var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);
            queryParams["query"] = query;
            queryParams["sortOption"] = sortOption;
            queryParams["favoritesOnly"] = favoritesOnly.ToString();
            // userCNP is not sent as a query parameter as the controller resolves it from the authenticated user.
            uriBuilder.Query = queryParams.ToString();
            return await _httpClient.GetFromJsonAsync<List<HomepageStock>>(uriBuilder.ToString()) ?? throw new InvalidOperationException("Failed to deserialize filtered and sorted stocks response.");
        }
    }
}