using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StockApp.Models;
using StockApp.Services;

namespace StockApp.Service.Tests
{
    [TestClass]
    public class AlertServiceTests
    {
        private IAlertService _service;

        [TestInitialize]
        public void Init()
        {
            _service = new InMemoryAlertService();
        }

        [TestMethod]
        public async Task CreateAlert_AddsAlert()
        {
            var alert = await _service.CreateAlertAsync("AAPL", "Test Alert", 200, 100, true);

            Assert.IsNotNull(alert);
            Assert.AreEqual("AAPL", alert.StockName);
            Assert.AreEqual("Test Alert", alert.Name);
            Assert.AreEqual(200, alert.UpperBound);
            Assert.AreEqual(100, alert.LowerBound);
            Assert.IsTrue(alert.ToggleOnOff);
        }

        [TestMethod]
        public async Task UpdateAlert_ChangesFields()
        {
            var alert = await _service.CreateAlertAsync("AAPL", "Old Alert", 300, 150, true);
            await _service.UpdateAlertAsync(alert.AlertId, "AAPL", "Updated Alert", 400, 200, false);

            var updated = await _service.GetAlertByIdAsync(alert.AlertId);

            Assert.IsNotNull(updated);
            Assert.AreEqual("Updated Alert", updated.Name);
            Assert.AreEqual(400, updated.UpperBound);
            Assert.IsFalse(updated.ToggleOnOff);
        }

        [TestMethod]
        public async Task RemoveAlert_DeletesSuccessfully()
        {
            var alert = await _service.CreateAlertAsync("TSLA", "Temp Alert", 800, 600, true);
            await _service.RemoveAlertAsync(alert.AlertId);

            var found = await _service.GetAlertByIdAsync(alert.AlertId);
            Assert.IsNull(found);
        }
    }

    internal class InMemoryAlertService : IAlertService
    {
        private readonly List<Alert> _alerts = new();
        private int _nextId = 1;

        public Task<List<Alert>> GetAllAlertsAsync() => Task.FromResult(new List<Alert>(_alerts));

        public Task<List<Alert>> GetAllAlertsOnAsync() => Task.FromResult(_alerts.FindAll(a => a.ToggleOnOff));

        public Task<Alert?> GetAlertByIdAsync(int alertId) => Task.FromResult(_alerts.Find(a => a.AlertId == alertId));

        public Task<Alert> CreateAlertAsync(string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff)
        {
            var alert = new Alert
            {
                AlertId = _nextId++,
                StockName = stockName,
                Name = name,
                UpperBound = upperBound,
                LowerBound = lowerBound,
                ToggleOnOff = toggleOnOff
            };
            _alerts.Add(alert);
            return Task.FromResult(alert);
        }

        public Task UpdateAlertAsync(int alertId, string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff)
        {
            var alert = _alerts.Find(a => a.AlertId == alertId);
            if (alert != null)
            {
                alert.StockName = stockName;
                alert.Name = name;
                alert.UpperBound = upperBound;
                alert.LowerBound = lowerBound;
                alert.ToggleOnOff = toggleOnOff;
            }
            return Task.CompletedTask;
        }

        public Task UpdateAlertAsync(Alert alert) =>
            UpdateAlertAsync(alert.AlertId, alert.StockName, alert.Name, alert.UpperBound, alert.LowerBound, alert.ToggleOnOff);

        public Task RemoveAlertAsync(int alertId)
        {
            _alerts.RemoveAll(a => a.AlertId == alertId);
            return Task.CompletedTask;
        }
    }
}
