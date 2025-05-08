namespace StockApp.Services
{
    using System;
    using System.Threading.Tasks;
    using StockApp.Exceptions;
    using StockApp.Models;
    using StockApp.Repositories;

    public class StoreService : IStoreService
    {
        private readonly IGemStoreRepository _repository;
        private readonly IUserRepository _userRepository;

        public StoreService(IGemStoreRepository repository, IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }


        /// <summary>
        /// Retrieves the current gem balance for the specified user.
        /// </summary>
        /// <param name="cnp"></param>
        /// <returns></returns>
        public async Task<int> GetUserGemBalanceAsync(string cnp)
        {
            return await _repository.GetUserGemBalanceAsync(cnp);
        }

        /// <summary>
        /// Updates the gem balance for a given user.
        /// </summary>
        /// <param name="cnp"></param>
        /// <param name="newBalance"></param>
        public async Task UpdateUserGemBalanceAsync(string cnp, int newBalance)
        {
            await _repository.UpdateUserGemBalanceAsync(cnp, newBalance);
        }

        /// <summary>
        /// Processes a bank transaction for buying or selling gems.
        /// </summary>
        /// <param name="userCNP"></param>
        /// <param name="deal"></param>
        /// <param name="selectedAccountId"></param>
        /// <returns></returns>
        /// <exception cref="GuestUserOperationException"></exception>
        /// <exception cref="GemTransactionFailedException"></exception>
        public async Task<string> BuyGems(string userCNP, GemDeal deal, string selectedAccountId)
        {
            if (_userRepository.IsGuest)
            {
                throw new GuestUserOperationException("Guests cannot buy gems.");
            }

            bool transactionSuccess = await ProcessBankTransaction(selectedAccountId, -deal.Price);
            if (!transactionSuccess)
            {
                throw new GemTransactionFailedException("Transaction failed. Please check your bank account balance.");
            }

            int currentBalance = await GetUserGemBalanceAsync(userCNP);
            await UpdateUserGemBalanceAsync(userCNP, currentBalance + deal.GemAmount);

            return $"Successfully purchased {deal.GemAmount} gems for {deal.Price}€";
        }

        /// <summary>
        /// Processes a bank transaction for selling gems.
        /// </summary>
        /// <param name="cnp"> The CNP of the user.</param>
        /// <param name="gemAmount"> The amount of gems to sell.</param>
        /// <param name="selectedAccountId"> The selected account ID for the transaction.</param>
        /// <returns></returns>
        /// <exception cref="GuestUserOperationException"></exception>
        /// <exception cref="InsufficientGemsException"></exception>
        /// <exception cref="GemTransactionFailedException"></exception>
        public async Task<string> SellGems(string cnp, int gemAmount, string selectedAccountId)
        {
            if (_userRepository.IsGuest)
            {
                throw new GuestUserOperationException("Guests cannot sell gems.");
            }

            int currentBalance = await GetUserGemBalanceAsync(cnp);
            if (gemAmount > currentBalance)
            {
                throw new InsufficientGemsException($"Not enough gems to sell. You have {currentBalance}, attempted to sell {gemAmount}.");
            }

            double moneyEarned = gemAmount / 100.0;
            bool transactionSuccess = await ProcessBankTransaction(selectedAccountId, moneyEarned);
            if (!transactionSuccess)
            {
                throw new GemTransactionFailedException("Transaction failed. Unable to deposit funds.");
            }

            await UpdateUserGemBalanceAsync(cnp, currentBalance - gemAmount);
            return $"Successfully sold {gemAmount} gems for {moneyEarned}€";
        }

        private static async Task<bool> ProcessBankTransaction(string accountId, double amount)
        {
            return true;
        }
    }
}
