using BankApi.Data;
using BankApi.Repositories;
using BankApi.Repositories.Impl;
using Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockApp.Repository.Tests
{
    [TestClass]
    public class LoanRequestRepositoryTests
    {
        private ApiDbContext _context;
        private ILoanRequestRepository _repository;
        private Mock<ILogger<LoanRequestRepository>> _mockLogger;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApiDbContext(options);
            _mockLogger = new Mock<ILogger<LoanRequestRepository>>();
            _repository = new LoanRequestRepository(_context, _mockLogger.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public async Task GetLoanRequestsAsync_ShouldReturnAllRequests()
        {
            // Arrange
            var requests = new List<LoanRequest>
            {
                new LoanRequest { Id = 1, UserCnp = "123", Amount = 1000, Status = "Pending", ApplicationDate = DateTime.UtcNow },
                new LoanRequest { Id = 2, UserCnp = "456", Amount = 2000, Status = "Solved", ApplicationDate = DateTime.UtcNow }
            };
            await _context.LoanRequests.AddRangeAsync(requests);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetLoanRequestsAsync();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(r => r.UserCnp == "123"));
            Assert.IsTrue(result.Any(r => r.UserCnp == "456"));
        }

        [TestMethod]
        public async Task GetUnsolvedLoanRequestsAsync_ShouldReturnOnlyUnsolvedRequests()
        {
            // Arrange
            var requests = new List<LoanRequest>
            {
                new LoanRequest { Id = 1, UserCnp = "123", Amount = 1000, Status = "Pending", ApplicationDate = DateTime.UtcNow },
                new LoanRequest { Id = 2, UserCnp = "456", Amount = 2000, Status = "Solved", ApplicationDate = DateTime.UtcNow },
                new LoanRequest { Id = 3, UserCnp = "789", Amount = 3000, Status = "Solved", ApplicationDate = DateTime.UtcNow }
            };
            await _context.LoanRequests.AddRangeAsync(requests);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUnsolvedLoanRequestsAsync();

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(r => r.Status != "Solved"));
        }

        [TestMethod]
        public async Task SolveLoanRequestAsync_ShouldUpdateRequestStatus()
        {
            // Arrange
            var request = new LoanRequest
            {
                Id = 1,
                UserCnp = "123",
                Amount = 1000,
                Status = "Pending",
                ApplicationDate = DateTime.UtcNow
            };
            await _context.LoanRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            // Act
            await _repository.SolveLoanRequestAsync(1);

            // Assert
            var updatedRequest = await _context.LoanRequests.FindAsync(1);
            Assert.AreEqual("Solved", updatedRequest.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SolveLoanRequestAsync_WithInvalidId_ShouldThrowException()
        {
            // Act
            await _repository.SolveLoanRequestAsync(0);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task SolveLoanRequestAsync_WithNonExistentId_ShouldThrowException()
        {
            // Act
            await _repository.SolveLoanRequestAsync(999);
        }

        [TestMethod]
        public async Task DeleteLoanRequestAsync_ShouldDeleteRequest()
        {
            // Arrange
            var request = new LoanRequest
            {
                Id = 1,
                UserCnp = "123",
                Amount = 1000,
                Status = "Pending",
                ApplicationDate = DateTime.UtcNow
            };
            await _context.LoanRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteLoanRequestAsync(1);

            // Assert
            Assert.AreEqual(0, await _context.LoanRequests.CountAsync());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteLoanRequestAsync_WithInvalidId_ShouldThrowException()
        {
            // Act
            await _repository.DeleteLoanRequestAsync(0);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task DeleteLoanRequestAsync_WithNonExistentId_ShouldThrowException()
        {
            // Act
            await _repository.DeleteLoanRequestAsync(999);
        }
    }
} 