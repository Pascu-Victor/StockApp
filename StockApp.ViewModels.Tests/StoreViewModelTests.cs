using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StockApp.Models;
using StockApp.Services;

namespace StockApp.ViewModels.Tests
{
    [TestClass]
    public class StoreViewModelTests
    {
        private Mock<IStoreService> _svc;
        private StoreViewModel _vm;

        [TestInitialize]
        public void Init()
        {
            _svc = new Mock<IStoreService>(MockBehavior.Strict);

            _svc.Setup(s => s.GetUserGemBalanceAsync("cnp")).ReturnsAsync(500);
            _svc.Setup(s => s.BuyGems("cnp", It.IsAny<GemDeal>(), It.IsAny<string>()))
                .ReturnsAsync("Successfully purchased");
            _svc.Setup(s => s.SellGems("cnp", It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync("Successfully sold");

            _vm = new StoreViewModel(_svc.Object, Mock.Of<IUserService>());
        }

        [TestMethod]
        public async Task BuyGemsAsync_NoAccount_ReturnsError()
        {
            var deal = new GemDeal("D", 100, 1.0);
            var msg = await _vm.BuyGemsAsync(deal, "");
            Assert.AreEqual("No bank account selected.", msg);
        }

        [TestMethod]
        public async Task BuyGemsAsync_WithAccount_UpdatesUserGems()
        {
            var deal = new GemDeal("D", 100, 1.0);
            var msg = await _vm.BuyGemsAsync(deal, "acct");
            Assert.AreEqual("Successfully purchased", msg);
            Assert.AreEqual(600, _vm.UserGems);
        }

        [TestMethod]
        public async Task SellGemsAsync_NoAccount_ReturnsError()
        {
            var msg = await _vm.SellGemsAsync(50, "");
            Assert.AreEqual("No bank account selected.", msg);
        }

        [TestMethod]
        public async Task SellGemsAsync_InvalidAmount_ReturnsError()
        {
            var msg = await _vm.SellGemsAsync(0, "acct");
            Assert.AreEqual("Invalid amount.", msg);
        }

        [TestMethod]
        public async Task SellGemsAsync_ExceedBalance_ReturnsError()
        {
            var msg = await _vm.SellGemsAsync(1000, "acct");
            Assert.AreEqual("Not enough Gems.", msg);
        }

        [TestMethod]
        public async Task SellGemsAsync_Valid_UpdatesUserGems()
        {
            var msg = await _vm.SellGemsAsync(100, "acct");
            Assert.AreEqual("Successfully sold", msg);
            Assert.AreEqual(400, _vm.UserGems);
        }

        [TestMethod]
        public void GetUserBankAccounts_ReturnsStaticList()
        {
            var list = StoreViewModel.GetUserBankAccounts();
            CollectionAssert.AreEqual(
                new[] { "Account 1", "Account 2", "Account 3" },
                list);
        }
    }
}