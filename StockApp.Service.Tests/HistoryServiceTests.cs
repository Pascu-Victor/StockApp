using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankApi.Services;
using BankApi.Repositories;
using Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace StockApp.Service.Tests
{
    [TestClass]
    public class HistoryServiceTests
    {
        private Mock<IHistoryRepository> _mockRepo;
        private HistoryService _service;

        [TestInitialize]
        public void Init()
        {
            _mockRepo = new Mock<IHistoryRepository>();
            _service = new HistoryService(_mockRepo.Object);
        }

        [TestMethod]
        public async Task GetAllHistoryAsync_HappyCase_ReturnsList()
        {
            var history = new List<CreditScoreHistory> { new CreditScoreHistory { Id = 1, Score = 500 } };
            _mockRepo.Setup(r => r.GetAllHistoryAsync()).ReturnsAsync(history);
            var result = await _service.GetAllHistoryAsync();
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(Common.Exceptions.HistoryServiceException))]
        public async Task GetAllHistoryAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetAllHistoryAsync()).ThrowsAsync(new Exception());
            await _service.GetAllHistoryAsync();
        }

        [TestMethod]
        public async Task GetHistoryByIdAsync_HappyCase_ReturnsHistory()
        {
            var history = new CreditScoreHistory { Id = 1, Score = 500 };
            _mockRepo.Setup(r => r.GetHistoryByIdAsync(1)).ReturnsAsync(history);
            var result = await _service.GetHistoryByIdAsync(1);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(Common.Exceptions.HistoryServiceException))]
        public async Task GetHistoryByIdAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetHistoryByIdAsync(1)).ThrowsAsync(new Exception());
            await _service.GetHistoryByIdAsync(1);
        }

        [TestMethod]
        public async Task AddHistoryAsync_HappyCase_AddsHistory()
        {
            var history = new CreditScoreHistory { Id = 1, Score = 500 };
            _mockRepo.Setup(r => r.AddHistoryAsync(history)).Returns(Task.FromResult(history));
            await _service.AddHistoryAsync(history);
            _mockRepo.Verify(r => r.AddHistoryAsync(history), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddHistoryAsync_NullHistory_ThrowsArgumentNullException()
        {
            await _service.AddHistoryAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Common.Exceptions.HistoryServiceException))]
        public async Task AddHistoryAsync_RepositoryThrows_ThrowsException()
        {
            var history = new CreditScoreHistory { Id = 1, Score = 500 };
            _mockRepo.Setup(r => r.AddHistoryAsync(history)).ThrowsAsync(new Exception());
            await _service.AddHistoryAsync(history);
        }

        [TestMethod]
        public async Task UpdateHistoryAsync_HappyCase_UpdatesHistory()
        {
            var history = new CreditScoreHistory { Id = 1, Score = 500 };
            _mockRepo.Setup(r => r.UpdateHistoryAsync(history)).Returns(Task.FromResult(history));
            await _service.UpdateHistoryAsync(history);
            _mockRepo.Verify(r => r.UpdateHistoryAsync(history), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateHistoryAsync_NullHistory_ThrowsArgumentNullException()
        {
            await _service.UpdateHistoryAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(Common.Exceptions.HistoryServiceException))]
        public async Task UpdateHistoryAsync_RepositoryThrows_ThrowsException()
        {
            var history = new CreditScoreHistory { Id = 1, Score = 500 };
            _mockRepo.Setup(r => r.UpdateHistoryAsync(history)).ThrowsAsync(new Exception());
            await _service.UpdateHistoryAsync(history);
        }

        [TestMethod]
        public async Task DeleteHistoryAsync_HappyCase_DeletesHistory()
        {
            _mockRepo.Setup(r => r.DeleteHistoryAsync(1)).Returns(Task.CompletedTask);
            await _service.DeleteHistoryAsync(1);
            _mockRepo.Verify(r => r.DeleteHistoryAsync(1), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Common.Exceptions.HistoryServiceException))]
        public async Task DeleteHistoryAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.DeleteHistoryAsync(1)).ThrowsAsync(new Exception());
            await _service.DeleteHistoryAsync(1);
        }

        [TestMethod]
        public async Task GetHistoryForUserAsync_HappyCase_ReturnsList()
        {
            var history = new List<CreditScoreHistory> { new CreditScoreHistory { Id = 1, Score = 500 } };
            _mockRepo.Setup(r => r.GetHistoryForUserAsync("123")).ReturnsAsync(history);
            var result = await _service.GetHistoryForUserAsync("123");
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetHistoryForUserAsync_EmptyCnp_ThrowsArgumentException()
        {
            await _service.GetHistoryForUserAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(Common.Exceptions.HistoryServiceException))]
        public async Task GetHistoryForUserAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetHistoryForUserAsync("123")).ThrowsAsync(new Exception());
            await _service.GetHistoryForUserAsync("123");
        }

        [TestMethod]
        public async Task GetHistoryWeeklyAsync_HappyCase_ReturnsList()
        {
            var history = new List<CreditScoreHistory> { new CreditScoreHistory { Id = 1, Score = 500 } };
            _mockRepo.Setup(r => r.GetHistoryWeeklyAsync("123")).ReturnsAsync(history);
            var result = await _service.GetHistoryWeeklyAsync("123");
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetHistoryWeeklyAsync_EmptyCnp_ThrowsArgumentException()
        {
            await _service.GetHistoryWeeklyAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(Common.Exceptions.HistoryServiceException))]
        public async Task GetHistoryWeeklyAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetHistoryWeeklyAsync("123")).ThrowsAsync(new Exception());
            await _service.GetHistoryWeeklyAsync("123");
        }

        [TestMethod]
        public async Task GetHistoryMonthlyAsync_HappyCase_ReturnsList()
        {
            var history = new List<CreditScoreHistory> { new CreditScoreHistory { Id = 1, Score = 500 } };
            _mockRepo.Setup(r => r.GetHistoryMonthlyAsync("123")).ReturnsAsync(history);
            var result = await _service.GetHistoryMonthlyAsync("123");
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetHistoryMonthlyAsync_EmptyCnp_ThrowsArgumentException()
        {
            await _service.GetHistoryMonthlyAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(Common.Exceptions.HistoryServiceException))]
        public async Task GetHistoryMonthlyAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetHistoryMonthlyAsync("123")).ThrowsAsync(new Exception());
            await _service.GetHistoryMonthlyAsync("123");
        }

        [TestMethod]
        public async Task GetHistoryYearlyAsync_HappyCase_ReturnsList()
        {
            var history = new List<CreditScoreHistory> { new CreditScoreHistory { Id = 1, Score = 500 } };
            _mockRepo.Setup(r => r.GetHistoryYearlyAsync("123")).ReturnsAsync(history);
            var result = await _service.GetHistoryYearlyAsync("123");
            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetHistoryYearlyAsync_EmptyCnp_ThrowsArgumentException()
        {
            await _service.GetHistoryYearlyAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(Common.Exceptions.HistoryServiceException))]
        public async Task GetHistoryYearlyAsync_RepositoryThrows_ThrowsException()
        {
            _mockRepo.Setup(r => r.GetHistoryYearlyAsync("123")).ThrowsAsync(new Exception());
            await _service.GetHistoryYearlyAsync("123");
        }
    }
} 