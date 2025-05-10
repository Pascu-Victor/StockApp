using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StockApp.Repository.Tests
{
    // Mock model classes for testing
    public class TransactionLogTransaction
    {
        public string StockSymbol { get; }
        public string StockName { get; }
        public string Type { get; }
        public int Amount { get; }
        public int PricePerStock { get; }
        public int TotalValue => this.Amount * this.PricePerStock;
        public DateTime Date { get; }
        public string Author { get; }

        public TransactionLogTransaction(
            string stockSymbol,
            string stockName,
            string type,
            int amount,
            int pricePerStock,
            DateTime date,
            string author)
        {
            this.StockSymbol = stockSymbol;
            this.StockName = stockName;
            this.Type = type;
            this.Amount = amount;
            this.PricePerStock = pricePerStock;
            this.Date = date;
            this.Author = author;
        }
    }

    public class TransactionFilterCriteria
    {
        public string? StockName { get; set; }
        public string? Type { get; set; }
        public int? MinTotalValue { get; set; }
        public int? MaxTotalValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public void Validate() { }
    }

    public interface ITransactionRepository
    {
        Task<List<TransactionLogTransaction>> getAllTransactions();
        Task<List<TransactionLogTransaction>> GetByFilterCriteria(TransactionFilterCriteria criteria);
        Task AddTransaction(TransactionLogTransaction transaction);
    }

    [TestClass]
    public class TransactionRepositoryTests
    {
        private ITransactionRepository _repo;

        [TestInitialize]
        public void Init()
        {
            _repo = new FakeTransactionRepository();
        }

        [TestMethod]
        public async Task AddTransaction_AppendsToList()
        {
            var transaction = new TransactionLogTransaction(
                stockSymbol: "AAPL",
                stockName: "Apple",
                type: "BUY",
                amount: 10,
                pricePerStock: 15,
                date: DateTime.Now,
                author: "1234567890123");

            await _repo.AddTransaction(transaction);

            var transactions = await _repo.getAllTransactions();
            Assert.AreEqual(1, transactions.Count);
            Assert.AreEqual("Apple", transactions[0].StockName);
        }

        [TestMethod]
        public async Task GetByFilterCriteria_FiltersByType()
        {
            await _repo.AddTransaction(new TransactionLogTransaction(
                stockSymbol: "MSFT",
                stockName: "Microsoft",
                type: "SELL",
                amount: 20,
                pricePerStock: 50,
                date: DateTime.Today,
                author: "999"));

            var result = await _repo.GetByFilterCriteria(new TransactionFilterCriteria { Type = "SELL" });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("SELL", result[0].Type);
        }

        private class FakeTransactionRepository : ITransactionRepository
        {
            private List<TransactionLogTransaction> _transactions = new List<TransactionLogTransaction>();

            public async Task<List<TransactionLogTransaction>> getAllTransactions()
            {
                return await Task.FromResult(_transactions);
            }

            public async Task AddTransaction(TransactionLogTransaction transaction)
            {
                _transactions.Add(transaction);
                await Task.CompletedTask;
            }

            public async Task<List<TransactionLogTransaction>> GetByFilterCriteria(TransactionFilterCriteria criteria)
            {
                return await Task.FromResult(_transactions.FindAll(t =>
                    (string.IsNullOrEmpty(criteria.Type) || t.Type == criteria.Type)
                ));
            }
        }
    }
}
