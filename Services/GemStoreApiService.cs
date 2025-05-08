using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Services
{
    public class GemStoreApiService : IGemStoreApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7001/api/GemStore";
        private readonly JsonSerializerOptions _jsonOptions;

        public GemStoreApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<GemStore> GetGemStoreByCnpAsync(string cnp)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/{cnp}");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<GemStore>(_jsonOptions);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException($"Gem store for user with CNP {cnp} not found");
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to retrieve gem store. Status code: {response.StatusCode}, Error: {errorContent}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving gem store for CNP {cnp}", ex);
            }
        }

        public async Task<int> GetGemBalanceAsync(string cnp)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/{cnp}/balance");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<int>(_jsonOptions);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException($"Gem store for user with CNP {cnp} not found");
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to retrieve gem balance. Status code: {response.StatusCode}, Error: {errorContent}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving gem balance for CNP {cnp}", ex);
            }
        }

        public async Task<GemStore> UpdateGemBalanceAsync(string cnp, int newBalance)
        {
            try
            {
                var gemStore = new GemStore
                {
                    UserCnp = cnp,
                    GemBalance = newBalance,
                    LastUpdated = DateTime.UtcNow
                };

                var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/{cnp}", gemStore);
                
                if (response.IsSuccessStatusCode)
                {
                    return gemStore;
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException($"Gem store for user with CNP {cnp} not found");
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update gem balance. Status code: {response.StatusCode}, Error: {errorContent}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating gem balance for CNP {cnp}", ex);
            }
        }

        public async Task<GemStore> CreateGemStoreAsync(string cnp, int initialBalance = 0)
        {
            try
            {
                var gemStore = new GemStore
                {
                    UserCnp = cnp,
                    GemBalance = initialBalance,
                    LastUpdated = DateTime.UtcNow
                };

                var response = await _httpClient.PostAsJsonAsync(_baseUrl, gemStore);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<GemStore>(_jsonOptions);
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create gem store. Status code: {response.StatusCode}, Error: {errorContent}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while creating gem store for CNP {cnp}", ex);
            }
        }
    }
} 