using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Services
{
    public class AlertApiService : IAlertService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/alerts";

        public AlertApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
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
            return await _httpClient.GetFromJsonAsync<List<Alert>>(BaseUrl) ?? new List<Alert>();
        }

        public async Task<Alert> GetAlertByIdAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<Alert>($"{BaseUrl}/{id}") 
                ?? throw new KeyNotFoundException($"Alert with ID {id} not found.");
        }

        public async Task<Alert> CreateAlertAsync(Alert alert)
        {
            if (alert == null)
                throw new ArgumentNullException(nameof(alert));

            var response = await _httpClient.PostAsJsonAsync(BaseUrl, alert);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Alert>() 
                ?? throw new InvalidOperationException("Failed to deserialize created alert.");
        }

        public async Task UpdateAlertAsync(Alert alert)
        {
            if (alert == null)
                throw new ArgumentNullException(nameof(alert));

            var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{alert.AlertId}", alert);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAlertAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task TriggerAlertAsync(string stockName, decimal currentPrice)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/{stockName}/trigger", currentPrice);
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<TriggeredAlert>> GetTriggeredAlertsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TriggeredAlert>>($"{BaseUrl}/triggered") 
                ?? new List<TriggeredAlert>();
        }

        public async Task ClearTriggeredAlertsAsync()
        {
            var response = await _httpClient.DeleteAsync($"{BaseUrl}/triggered");
            response.EnsureSuccessStatusCode();
        }
    }
} 