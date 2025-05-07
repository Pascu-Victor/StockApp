using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using StockApp.Models;
using StockApp.Exceptions;

namespace StockApp.Services
{
    public class AlertApiService : IAlertService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/alerts";

        public event EventHandler<bool>? LoadingStateChanged;
        public event EventHandler<string>? ErrorOccurred;

        public AlertApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        private async Task<T> HandleApiCallAsync<T>(Func<Task<T>> apiCall)
        {
            try
            {
                LoadingStateChanged?.Invoke(this, true);
                return await apiCall();
            }
            catch (HttpRequestException ex)
            {
                var message = ex.StatusCode switch
                {
                    HttpStatusCode.NotFound => "The requested resource was not found.",
                    HttpStatusCode.BadRequest => "The request was invalid.",
                    _ => "An error occurred while communicating with the server."
                };
                ErrorOccurred?.Invoke(this, message);
                throw new AlertRepositoryException(message, ex);
            }
            catch (JsonException ex)
            {
                var message = "Failed to process the server response.";
                ErrorOccurred?.Invoke(this, message);
                throw new AlertRepositoryException(message, ex);
            }
            finally
            {
                LoadingStateChanged?.Invoke(this, false);
            }
        }

        // Synchronous methods
        public List<Alert> GetAllAlerts()
        {
            return GetAllAlertsAsync().GetAwaiter().GetResult().ToList();
        }

        public List<Alert> GetAllAlertsOn()
        {
            return GetAllAlerts().FindAll(a => a.ToggleOnOff);
        }

        public Alert? GetAlertById(int alertId)
        {
            try
            {
                return GetAlertByIdAsync(alertId).GetAwaiter().GetResult();
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public Alert CreateAlert(string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff)
        {
            var alert = new Alert
            {
                StockName = stockName,
                Name = name,
                UpperBound = upperBound,
                LowerBound = lowerBound,
                ToggleOnOff = toggleOnOff
            };
            return CreateAlertAsync(alert).GetAwaiter().GetResult();
        }

        public void UpdateAlert(int alertId, string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff)
        {
            var alert = new Alert
            {
                AlertId = alertId,
                StockName = stockName,
                Name = name,
                UpperBound = upperBound,
                LowerBound = lowerBound,
                ToggleOnOff = toggleOnOff
            };
            UpdateAlertAsync(alert).GetAwaiter().GetResult();
        }

        public void UpdateAlert(Alert alert)
        {
            UpdateAlertAsync(alert).GetAwaiter().GetResult();
        }

        public void RemoveAlert(int alertId)
        {
            DeleteAlertAsync(alertId).GetAwaiter().GetResult();
        }

        // Async methods
        public async Task<IEnumerable<Alert>> GetAllAlertsAsync()
        {
            return await HandleApiCallAsync(async () =>
            {
                var response = await _httpClient.GetFromJsonAsync<List<Alert>>(BaseUrl);
                return response ?? new List<Alert>();
            });
        }

        public async Task<Alert> GetAlertByIdAsync(int id)
        {
            return await HandleApiCallAsync(async () =>
            {
                var response = await _httpClient.GetFromJsonAsync<Alert>($"{BaseUrl}/{id}");
                return response ?? throw new KeyNotFoundException($"Alert with ID {id} not found.");
            });
        }

        public async Task<Alert> CreateAlertAsync(Alert alert)
        {
            if (alert == null)
                throw new ArgumentNullException(nameof(alert));

            return await HandleApiCallAsync(async () =>
            {
                var response = await _httpClient.PostAsJsonAsync(BaseUrl, alert);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Alert>() 
                    ?? throw new InvalidOperationException("Failed to deserialize created alert.");
            });
        }

        public async Task UpdateAlertAsync(Alert alert)
        {
            if (alert == null)
                throw new ArgumentNullException(nameof(alert));

            await HandleApiCallAsync(async () =>
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{alert.AlertId}", alert);
                response.EnsureSuccessStatusCode();
                return true;
            });
        }

        public async Task DeleteAlertAsync(int id)
        {
            await HandleApiCallAsync(async () =>
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
                response.EnsureSuccessStatusCode();
                return true;
            });
        }

        public async Task TriggerAlertAsync(string stockName, decimal currentPrice)
        {
            await HandleApiCallAsync(async () =>
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/{stockName}/trigger", currentPrice);
                response.EnsureSuccessStatusCode();
                return true;
            });
        }

        public async Task<IEnumerable<TriggeredAlert>> GetTriggeredAlertsAsync()
        {
            return await HandleApiCallAsync(async () =>
            {
                var response = await _httpClient.GetFromJsonAsync<List<TriggeredAlert>>($"{BaseUrl}/triggered");
                return response ?? new List<TriggeredAlert>();
            });
        }

        public async Task ClearTriggeredAlertsAsync()
        {
            await HandleApiCallAsync(async () =>
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/triggered");
                response.EnsureSuccessStatusCode();
                return true;
            });
        }
    }
} 