using System.Collections.Generic;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Services
{
    public interface IAlertService
    {
        // Synchronous methods
        List<Alert> GetAllAlerts();
        List<Alert> GetAllAlertsOn();
        Alert? GetAlertById(int alertId);
        Alert CreateAlert(string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff);
        void UpdateAlert(int alertId, string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff);
        void UpdateAlert(Alert alert);
        void RemoveAlert(int alertId);

        // Async methods
        Task<IEnumerable<Alert>> GetAllAlertsAsync();
        Task<Alert> GetAlertByIdAsync(int id);
        Task<Alert> CreateAlertAsync(Alert alert);
        Task UpdateAlertAsync(Alert alert);
        Task DeleteAlertAsync(int id);
        Task TriggerAlertAsync(string stockName, decimal currentPrice);
        Task<IEnumerable<TriggeredAlert>> GetTriggeredAlertsAsync();
        Task ClearTriggeredAlertsAsync();
    }
}