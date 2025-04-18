﻿namespace StockApp.Services
{
    using System.Collections.Generic;
    using StockApp.Models;

    public interface IAlertService
    {
        Alert CreateAlert(string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff);

        List<Alert> GetAllAlerts();

        List<Alert> GetAllAlertsOn();

        Alert? GetAlertById(int alertId);

        void RemoveAlert(int alertId);

        void UpdateAlert(int alertId, string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff);
    }
}