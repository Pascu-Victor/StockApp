namespace StockApp.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using StockApp.Models;
    using StockApp.Service;

    public class AlertViewModel
    {
        private readonly AlertService alertService = new ();

        public List<Alert> Alerts => this.alertService.GetAllAlerts();

        public Alert GetAlertById(int alertId) =>
            this.alertService.GetAlertById(alertId) ?? throw new Exception($"Alert with ID {alertId} not found.");

        public Alert CreateAlert(string stockName, string name, int upperBound, int lowerBound, bool toggleOnOff) =>
            this.alertService.CreateAlert(stockName, name, upperBound, lowerBound, toggleOnOff);

        public void UpdateAlert(int alertId, string stockName, string name, int upperBound, int lowerBound, bool toggleOnOff) =>
            this.alertService.UpdateAlert(alertId, stockName, name, upperBound, lowerBound, toggleOnOff);

        public void UpdateAlert(Alert alert) =>
            this.alertService.UpdateAlert(
                alert.AlertId,
                alert.StockName,
                alert.Name,
                alert.UpperBound,
                alert.LowerBound,
                alert.ToggleOnOff);

        public void DeleteAlert(int alertId) => this.alertService.RemoveAlert(alertId);
    }
}
