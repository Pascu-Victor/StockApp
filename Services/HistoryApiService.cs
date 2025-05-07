using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Services
{
    public class HistoryApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public event EventHandler<string>? ErrorOccurred;
        public event EventHandler<bool>? LoadingStateChanged;

        public HistoryApiService(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<List<CreditScoreHistory>> GetHistoryForUserAsync(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = await _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"{_baseUrl}/api/history/user/{userCnp}");
                return response ?? new List<CreditScoreHistory>();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, "Failed to retrieve credit score history: " + ex.Message);
                throw;
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }

        public async Task<CreditScoreHistory> AddHistoryEntryAsync(CreditScoreHistory history)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/history", history);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<CreditScoreHistory>() 
                    ?? throw new Exception("Failed to deserialize response");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, "Failed to add credit score history entry: " + ex.Message);
                throw;
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }

        public async Task DeleteHistoryEntryAsync(int id)
        {
            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/history/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, "Failed to delete credit score history entry: " + ex.Message);
                throw;
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }
    }
} 