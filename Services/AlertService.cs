namespace StockApp.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using StockApp.Models;
    using StockApp.Repositories;

    public class AlertService : IAlertService
    {
        private readonly AlertRepository repository = new();

        public List<Alert> GetAllAlerts() => this.repository.Alerts;

        public List<Alert> GetAllAlertsOn() => this.repository.Alerts.FindAll(a => a.ToggleOnOff);

        public Alert? GetAlertById(int alertId) => this.repository.Alerts.Find(a => a.AlertId == alertId);

        public Alert CreateAlert(string stockName, string name, int upperBound, int lowerBound, bool toggleOnOff) =>
            this.repository.AddAlert(stockName, name, upperBound, lowerBound, toggleOnOff);

        public void UpdateAlert(int alertId, string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff) =>
            this.repository.UpdateAlert(alertId, stockName, name, upperBound, lowerBound, toggleOnOff);

        public void RemoveAlert(int alertId) => this.repository.DeleteAlert(alertId);
    }
}