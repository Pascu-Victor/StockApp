namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using StockApp.Models;

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

        public List<CreditScoreHistory> GetAllHistory()
        {
            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"{_baseUrl}/api/history").GetAwaiter().GetResult();
                return response ?? new List<CreditScoreHistory>();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, "Failed to retrieve all credit score history: " + ex.Message);
                throw;
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }

        public CreditScoreHistory GetHistoryById(int id)
        {
            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = _httpClient.GetFromJsonAsync<CreditScoreHistory>($"{_baseUrl}/api/history/{id}").GetAwaiter().GetResult();
                return response ?? throw new KeyNotFoundException($"History entry with ID {id} not found.");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to retrieve history entry with ID {id}: " + ex.Message);
                throw;
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }

        public void AddHistory(CreditScoreHistory history)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = _httpClient.PostAsJsonAsync($"{_baseUrl}/api/history", history).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
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

        public void UpdateHistory(CreditScoreHistory history)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = _httpClient.PutAsJsonAsync($"{_baseUrl}/api/history/{history.Id}", history).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to update history entry with ID {history.Id}: " + ex.Message);
                throw;
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }

        public void DeleteHistory(int id)
        {
            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = _httpClient.DeleteAsync($"{_baseUrl}/api/history/{id}").GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Failed to delete history entry with ID {id}: " + ex.Message);
                throw;
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }

        public List<CreditScoreHistory> GetHistoryForUser(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"{_baseUrl}/api/history/user/{userCnp}").GetAwaiter().GetResult();
                return response ?? new List<CreditScoreHistory>();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, "Failed to retrieve credit score history for user: " + ex.Message);
                throw;
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }

        public List<CreditScoreHistory> GetHistoryWeekly(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"{_baseUrl}/api/history/user/{userCnp}/weekly").GetAwaiter().GetResult();
                return response ?? new List<CreditScoreHistory>();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, "Failed to retrieve weekly credit score history: " + ex.Message);
                throw;
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }

        public List<CreditScoreHistory> GetHistoryMonthly(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"{_baseUrl}/api/history/user/{userCnp}/monthly").GetAwaiter().GetResult();
                return response ?? new List<CreditScoreHistory>();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, "Failed to retrieve monthly credit score history: " + ex.Message);
                throw;
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }

        public List<CreditScoreHistory> GetHistoryYearly(string userCnp)
        {
            if (string.IsNullOrWhiteSpace(userCnp))
            {
                throw new ArgumentException("User CNP cannot be empty", nameof(userCnp));
            }

            try
            {
                LoadingStateChanged?.Invoke(this, true);
                var response = _httpClient.GetFromJsonAsync<List<CreditScoreHistory>>($"{_baseUrl}/api/history/user/{userCnp}/yearly").GetAwaiter().GetResult();
                return response ?? new List<CreditScoreHistory>();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, "Failed to retrieve yearly credit score history: " + ex.Message);
                throw;
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }
    }
} 