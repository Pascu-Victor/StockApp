﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StockApp.Models;
using StockApp.Services;
using StockApp.ViewModels;

namespace StockApp.ViewModels.Tests
{
    [TestClass]
    public class TransactionLogViewModelTests
    {
        private Mock<ITransactionLogService> _serviceMock;
        private TransactionLogViewModel _vm;
        private bool _messageBoxShown;
        private string? _msgTitle;
        private string? _msgContent;

        [TestInitialize]
        public void Init()
        {
            _serviceMock = new Mock<ITransactionLogService>(MockBehavior.Strict);

            var sample = new List<TransactionLogTransaction>
            {
                new TransactionLogTransaction("AAPL","AAPL","BUY",1,100,DateTime.Today,"cnp1"),
                new TransactionLogTransaction("MSFT","MSFT","SELL",2,200,DateTime.Today,"cnp2")
            };
            _serviceMock
                .Setup(s => s.GetFilteredTransactions(It.IsAny<TransactionFilterCriteria>()))
                .Returns(sample);
            _serviceMock
                .Setup(s => s.SortTransactions(It.IsAny<List<TransactionLogTransaction>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns((List<TransactionLogTransaction> tx, string _, bool __) => tx);
            _serviceMock
                .Setup(s => s.ExportTransactions(It.IsAny<List<TransactionLogTransaction>>(), It.IsAny<string>(), It.IsAny<string>()));

            _vm = new TransactionLogViewModel(_serviceMock.Object);

            _vm.ShowMessageBoxRequested += (t, c) =>
            {
                _messageBoxShown = true;
                _msgTitle = t;
                _msgContent = c;
            };
        }

        [TestMethod]
        public void Constructor_InitializesFilterStrings()
        {
            Assert.AreEqual("ALL", _vm.SelectedTransactionType);
            Assert.AreEqual("Date", _vm.SelectedSortBy);
            Assert.AreEqual("ASC", _vm.SelectedSortOrder);
            Assert.AreEqual("CSV", _vm.SelectedExportFormat);
        }

        [TestMethod]
        public void LoadTransactions_PopulatesTransactions()
        {
            Assert.AreEqual(2, _vm.Transactions.Count);
            CollectionAssert.AreEqual(
                new[] { "AAPL", "MSFT" },
                _vm.Transactions.Select(tx => tx.StockName).ToArray());
        }

        [TestMethod]
        public void MinGreaterThanMax_ShowsTotalValueError()
        {
            _messageBoxShown = false;

            _vm.MinTotalValue = "500";
            _vm.MaxTotalValue = "100";

            Assert.IsTrue(_messageBoxShown);
            Assert.AreEqual("Invalid Total Values", _msgTitle);
        }

        [TestMethod]
        public void DateRangeInvalid_ShowsDateError()
        {
            _messageBoxShown = false;

            _vm.EndDate = DateTime.Today;
            _vm.StartDate = DateTime.Today.AddDays(1);

            Assert.IsTrue(_messageBoxShown);
            Assert.AreEqual("Invalid Date Range", _msgTitle);
        }

        [TestMethod]
        public void SearchCommand_ReinvokesService()
        {
            bool called = false;
            _serviceMock
                .Setup(s => s.GetFilteredTransactions(It.IsAny<TransactionFilterCriteria>()))
                .Callback(() => called = true)
                .Returns(new List<TransactionLogTransaction>());
            _serviceMock
                .Setup(s => s.SortTransactions(It.IsAny<List<TransactionLogTransaction>>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new List<TransactionLogTransaction>());

            _vm.SearchCommand.Execute(null);

            Assert.IsTrue(called);
        }

        [TestMethod]
        public void ExportCommand_CallsExportOnService()
        {
            _vm.Transactions.Clear();

            var tx = new TransactionLogTransaction("X", "X", "BUY", 1, 50, DateTime.Today, "cnp");
            _vm.Transactions.Add(tx);

            _vm.ExportCommand.Execute(null);

            _serviceMock.Verify(s =>
                s.ExportTransactions(
                    It.Is<List<TransactionLogTransaction>>(l => l.Count == 1 && l[0] == tx),
                    It.Is<string>(path => path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)),
                    "CSV"
                ),
                Times.Once);
        }
    }
}