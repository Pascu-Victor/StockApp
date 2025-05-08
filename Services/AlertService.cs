namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;
    using StockApp.Repositories;

    /// <summary>
    /// Service for managing stock alerts.
    /// </summary>
    public class AlertService : IAlertService
    {
        /// <summary>
        /// The repository for managing alerts.
        /// </summary>
        private readonly IAlertRepository repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlertService"/> class.
        /// </summary>
        /// <param name="repository">The repository for managing alerts.</param>
        public AlertService(IAlertRepository repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Gets all alerts asynchronously.
        /// </summary>
        /// <returns>A list of all alerts.</returns>
        public async Task<List<Alert>> GetAllAlertsAsync() => await this.repository.GetAllAlertsAsync();

        /// <summary>
        /// Gets all alerts that are currently toggled on asynchronously.
        /// </summary>
        /// <returns>A list of alerts that are toggled on.</returns>
        public async Task<List<Alert>> GetAllAlertsOnAsync()
        {
            var alerts = await this.repository.GetAllAlertsAsync();
            return alerts.FindAll(a => a.ToggleOnOff);
        }

        /// <summary>
        /// Gets an alert by its unique identifier asynchronously.
        /// </summary>
        /// <param name="alertId">The unique identifier of the alert.</param>
        /// <returns>The alert with the specified identifier, or null if not found.</returns>
        public async Task<Alert?> GetAlertByIdAsync(int alertId) =>
            await this.repository.GetAlertByIdAsync(alertId);

        /// <summary>
        /// Creates a new alert asynchronously.
        /// </summary>
        /// <param name="stockName">The stock name associated with the alert.</param>
        /// <param name="name">The name of the alert.</param>
        /// <param name="upperBound">The upper price boundary for the alert.</param>
        /// <param name="lowerBound">The lower price boundary for the alert.</param>
        /// <param name="toggleOnOff">A value indicating whether the alert is active.</param>
        /// <returns>The newly created alert.</returns>
        public async Task<Alert> CreateAlertAsync(string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff)
        {
            var alert = new Alert
            {
                StockName = stockName,
                Name = name,
                UpperBound = upperBound,
                LowerBound = lowerBound,
                ToggleOnOff = toggleOnOff
            };

            return await this.repository.AddAlertAsync(alert);
        }

        /// <summary>
        /// Updates an existing alert asynchronously.
        /// </summary>
        /// <param name="alertId">The unique identifier of the alert to update.</param>
        /// <param name="stockName">The updated stock name.</param>
        /// <param name="name">The updated name of the alert.</param>
        /// <param name="upperBound">The updated upper price boundary.</param>
        /// <param name="lowerBound">The updated lower price boundary.</param>
        /// <param name="toggleOnOff">The updated toggle state of the alert.</param>
        public async Task UpdateAlertAsync(int alertId, string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff)
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

            await this.repository.UpdateAlertAsync(alert);
        }

        /// <summary>
        /// Updates an existing alert with the specified alert object asynchronously.
        /// </summary>
        /// <param name="alert">The alert object with updated properties.</param>
        public async Task UpdateAlertAsync(Alert alert) =>
            await this.repository.UpdateAlertAsync(alert);

        /// <summary>
        /// Removes an alert by its unique identifier asynchronously.
        /// </summary>
        /// <param name="alertId">The unique identifier of the alert to remove.</param>
        public async Task RemoveAlertAsync(int alertId) =>
            await this.repository.DeleteAlertAsync(alertId);
    }
}