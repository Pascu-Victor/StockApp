using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StockApp.Database;
using StockApp.Exceptions;
using StockApp.Models;

namespace StockApp.Repositories
{
    /// <summary>
    /// Repository for managing <see cref="Alert"/> entities and their triggered instances in the database.
    /// </summary>
    public class AlertRepository : IAlertRepository
    {
        private readonly AppDbContext _context;
        private readonly List<TriggeredAlert> _triggeredAlerts = new();

        public AlertRepository()
        {
            _context = new AppDbContext();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public AlertRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets all alerts from the database.
        /// </summary>
        public List<Alert> GetAllAlerts()
        {
            return _context.Alerts.ToList();
        }

        /// <summary>
        /// Retrieves a single alert by its identifier.
        /// </summary>
        /// <param name="alertId">The unique ID of the alert.</param>
        /// <returns>The matching <see cref="Alert"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no alert with the given ID exists.</exception>
        public Alert GetAlertById(int alertId)
        {
            return _context.Alerts.FirstOrDefault(a => a.AlertId == alertId)
                ?? throw new KeyNotFoundException($"Alert with ID {alertId} not found.");
        }

        /// <summary>
        /// Adds a new alert to the database.
        /// </summary>
        /// <param name="alert">The alert to add.</param>
        public void AddAlert(Alert alert)
        {
            if (alert == null)
                throw new ArgumentNullException(nameof(alert));

            _context.Alerts.Add(alert);
            _context.SaveChanges();
        }

        /// <summary>
        /// Updates an existing alert's properties.
        /// </summary>
        /// <param name="alert">The alert to update.</param>
        /// <exception cref="AlertRepositoryException">Thrown if the database update fails.</exception>
        public void UpdateAlert(Alert alert)
        {
            if (alert == null)
                throw new ArgumentNullException(nameof(alert));

            try
            {
                var existingAlert = _context.Alerts.FirstOrDefault(a => a.AlertId == alert.AlertId);
                if (existingAlert == null)
                    throw new KeyNotFoundException($"Alert with ID {alert.AlertId} not found.");

                _context.Entry(existingAlert).CurrentValues.SetValues(alert);
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                throw new AlertRepositoryException($"Failed to update alert with ID {alert.AlertId}.", ex);
            }
        }

        /// <summary>
        /// Deletes an alert from the database.
        /// </summary>
        /// <param name="alertId">ID of the alert to delete.</param>
        public void DeleteAlert(int alertId)
        {
            var alert = _context.Alerts.FirstOrDefault(a => a.AlertId == alertId);
            if (alert != null)
            {
                _context.Alerts.Remove(alert);
                _context.SaveChanges();
            }
        }

        /// <summary>
        /// Checks whether the specified stock price triggers any alert.
        /// </summary>
        /// <param name="stockName">Name of the stock.</param>
        /// <param name="currentPrice">Current price of the stock.</param>
        /// <returns><c>true</c> if an enabled alert is triggered; otherwise, <c>false</c>.</returns>
        public bool IsAlertTriggered(string stockName, decimal currentPrice)
        {
            return _context.Alerts.Any(a => 
                a.StockName == stockName && 
                a.ToggleOnOff && 
                (currentPrice >= a.UpperBound || currentPrice <= a.LowerBound));
        }

        /// <summary>
        /// Adds a <see cref="TriggeredAlert"/> entry if the given stock price triggers the alert.
        /// </summary>
        /// <param name="stockName">Name of the stock.</param>
        /// <param name="currentPrice">Current price of the stock.</param>
        public void TriggerAlert(string stockName, decimal currentPrice)
        {
            if (!IsAlertTriggered(stockName, currentPrice))
                return;

            var alert = _context.Alerts.First(a => a.StockName == stockName);
            string message = $"Alert triggered for {stockName}: Price = {currentPrice}, Bounds: [{alert.LowerBound} - {alert.UpperBound}]";

            _triggeredAlerts.Add(new TriggeredAlert
            {
                StockName = stockName,
                Message = message
            });
        }

        /// <summary>
        /// Gets the collection of alerts that have been triggered since load or last clear.
        /// </summary>
        public List<TriggeredAlert> GetTriggeredAlerts()
        {
            return _triggeredAlerts.ToList();
        }

        /// <summary>
        /// Clears the in-memory list of triggered alerts.
        /// </summary>
        public void ClearTriggeredAlerts()
        {
            _triggeredAlerts.Clear();
        }
    }
}
