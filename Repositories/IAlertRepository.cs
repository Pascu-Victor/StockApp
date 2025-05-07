using System.Collections.Generic;
using StockApp.Models;

namespace StockApp.Repositories
{
    public interface IAlertRepository
    {
        List<Alert> GetAllAlerts();
        Alert GetAlertById(int alertId);
        void AddAlert(Alert alert);
        void UpdateAlert(Alert alert);
        void DeleteAlert(int alertId);
        bool IsAlertTriggered(string stockName, decimal currentPrice);
        void TriggerAlert(string stockName, decimal currentPrice);
        List<TriggeredAlert> GetTriggeredAlerts();
        void ClearTriggeredAlerts();
    }
}
