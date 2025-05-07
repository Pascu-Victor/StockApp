using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockApp.Models;
using StockApp.Repositories;
using StockApp.Database;

namespace StockApp.Services
{
    public class AlertService : IAlertService
    {
        /// <summary>
        /// The repository for managing alerts.
        /// </summary>
        private readonly IAlertRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertService"/> class.
        /// </summary>
        public AlertService()
        {
            var context = new AppDbContext();
            _repository = new AlertRepository(context);
        }

        public AlertService(IAlertRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Gets all alerts that are currently toggled on.
        /// </summary>
        /// <returns></returns>
        public List<Alert> GetAllAlertsOn()
        {
            return _repository.GetAllAlerts().FindAll(a => a.ToggleOnOff);
        }

        /// <summary>
        /// Gets an alert by its unique identifier.
        /// </summary>
        /// <param name="alertId"></param>
        /// <returns></returns>
        public Alert? GetAlertById(int alertId)
        {
            try
            {
                return _repository.GetAlertById(alertId);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a new alert with the specified parameters.
        /// </summary>
        /// <param name="stockName"></param>
        /// <param name="name"></param>
        /// <param name="upperBound"></param>
        /// <param name="lowerBound"></param>
        /// <param name="toggleOnOff"></param>
        /// <returns></returns>
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
            _repository.AddAlert(alert);
            return alert;
        }

        /// <summary>
        /// Updates an existing alert with the specified parameters.
        /// </summary>
        /// <param name="alertId"></param>
        /// <param name="stockName"></param>
        /// <param name="name"></param>
        /// <param name="upperBound"></param>
        /// <param name="lowerBound"></param>
        /// <param name="toggleOnOff"></param>
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
            _repository.UpdateAlert(alert);
        }

        /// <summary>
        /// Updates an existing alert with the specified alert object.
        /// </summary>
        /// <param name="alert"></param>
        public void UpdateAlert(Alert alert)
        {
            _repository.UpdateAlert(alert);
        }

        /// <summary>
        /// Removes an alert by its unique identifier.
        /// </summary>
        /// <param name="alertId"></param>
        public void RemoveAlert(int alertId)
        {
            _repository.DeleteAlert(alertId);
        }

        public List<Alert> GetAllAlerts()
        {
            return _repository.GetAllAlerts();
        }

        public async Task<IEnumerable<Alert>> GetAllAlertsAsync()
        {
            return await Task.FromResult(_repository.GetAllAlerts());
        }

        public async Task<Alert> GetAlertByIdAsync(int id)
        {
            return await Task.FromResult(_repository.GetAlertById(id));
        }

        public async Task<Alert> CreateAlertAsync(Alert alert)
        {
            if (alert == null)
                throw new ArgumentNullException(nameof(alert));

            _repository.AddAlert(alert);
            return await Task.FromResult(alert);
        }

        public async Task UpdateAlertAsync(Alert alert)
        {
            if (alert == null)
                throw new ArgumentNullException(nameof(alert));

            await Task.Run(() => _repository.UpdateAlert(alert));
        }

        public async Task DeleteAlertAsync(int id)
        {
            await Task.Run(() => _repository.DeleteAlert(id));
        }

        public async Task TriggerAlertAsync(string stockName, decimal currentPrice)
        {
            await Task.Run(() => _repository.TriggerAlert(stockName, currentPrice));
        }

        public async Task<IEnumerable<TriggeredAlert>> GetTriggeredAlertsAsync()
        {
            return await Task.FromResult(_repository.GetTriggeredAlerts());
        }

        public async Task ClearTriggeredAlertsAsync()
        {
            await Task.Run(() => _repository.ClearTriggeredAlerts());
        }
    }
}