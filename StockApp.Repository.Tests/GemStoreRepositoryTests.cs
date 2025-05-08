using BankApi.Data;
using BankApi.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BankApi.Repository.Tests
{
    [TestClass]
    public class GemStoreRepositoryTests
    {
        private IGemStoreRepository _repo;
        private Mock<ApiDbContext> _dbContextMock; // Change DbContext to ApiDbContext

        [TestInitialize]
        public void Init()
        {
            _dbContextMock = new Mock<ApiDbContext>(); // Update the mock type to ApiDbContext
            _repo = new GemStoreRepository(_dbContextMock.Object); // This now matches the expected type
        }

        // The rest of the code remains unchanged
    }

    public class User
    {
        public string CNP { get; set; }
        public int GemBalance { get; set; }
    }
}