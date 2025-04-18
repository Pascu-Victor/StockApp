using System.Collections.Generic;
using StockApp.Models;

namespace StockApp.Services
{
    public interface IAlertService
    {
        List<Alert> GetAllAlerts();

        List<Alert> GetAllAlertsOn();

        Alert CreateAlert(string stockName, string name, int upperBound, int lowerBound, bool toggleOnOff);

        void UpdateAlert(int alertId, string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff);

        void RemoveAlert(int alertId);
    }
}
