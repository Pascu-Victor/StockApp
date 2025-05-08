// StockApp/Services/AlertProxyRepo.cs
namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;
    using StockApp.Models;
    using StockApp.Repositories;

    /// <summary>
    /// Proxy repository that implements IAlertRepository to make calls to the BankAPI
    /// </summary>
    public class AlertProxyRepo : IAlertRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7001/api/Alerts";
        private readonly JsonSerializerOptions _jsonOptions;

        public AlertProxyRepo(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <inheritdoc/>
        public async Task<Alert> AddAlertAsync(Alert alert)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(_baseUrl, alert);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Alert>(_jsonOptions);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to add alert. Status code: {response.StatusCode}, Error: {errorContent}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while adding the alert to the API.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAlertAsync(int alertId)
        {
            try
            {
                var url = $"{_baseUrl}/{alertId}";
                var response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to delete alert. Status code: {response.StatusCode}, Error: {errorContent}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while deleting the alert ID {alertId} from the API.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<List<Alert>> GetAllAlertsAsync()
        {
            try
            {
                var alerts = await _httpClient.GetFromJsonAsync<List<Alert>>(_baseUrl, _jsonOptions);
                return alerts ?? new List<Alert>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving alerts from the API.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Alert> GetAlertByIdAsync(int alertId)
        {
            try
            {
                var url = $"{_baseUrl}/{alertId}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Alert>(_jsonOptions);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException($"Alert with ID {alertId} not found.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to retrieve alert. Status code: {response.StatusCode}, Error: {errorContent}");
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while retrieving the alert with ID {alertId} from the API.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Alert> UpdateAlertAsync(Alert alert)
        {
            try
            {
                var url = $"{_baseUrl}/{alert.AlertId}";
                var response = await _httpClient.PutAsJsonAsync(url, alert);

                if (response.IsSuccessStatusCode)
                {
                    return alert;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException($"Alert with ID {alert.AlertId} not found.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to update alert. Status code: {response.StatusCode}, Error: {errorContent}");
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while updating the alert with ID {alert.AlertId} in the API.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<List<TriggeredAlert>> GetTriggeredAlertsAsync()
        {
            try
            {
                var url = $"{_baseUrl}/triggered";
                var triggeredAlerts = await _httpClient.GetFromJsonAsync<List<TriggeredAlert>>(url, _jsonOptions);
                return triggeredAlerts ?? new List<TriggeredAlert>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while retrieving triggered alerts from the API.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task ClearTriggeredAlertsAsync()
        {
            try
            {
                var url = $"{_baseUrl}/triggered";
                var response = await _httpClient.DeleteAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to clear triggered alerts. Status code: {response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred while clearing triggered alerts in the API.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsAlertTriggeredAsync(string stockName, decimal currentPrice)
        {
            try
            {
                var triggerRequest = new
                {
                    StockName = stockName,
                    CurrentPrice = currentPrice
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/trigger", triggerRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<dynamic>(_jsonOptions);
                    return result != null && result.Message == null; // If there's no message, an alert was triggered
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task TriggerAlertAsync(string stockName, decimal currentPrice)
        {
            try
            {
                var triggerRequest = new
                {
                    StockName = stockName,
                    CurrentPrice = currentPrice
                };

                await _httpClient.PostAsJsonAsync($"{_baseUrl}/trigger", triggerRequest);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while triggering alert for stock {stockName} in the API.", ex);
            }
        }
    }
}
