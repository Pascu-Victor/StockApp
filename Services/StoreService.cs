namespace StockApp.Services
{
    using System;
    using System.Threading.Tasks;
    using StockApp.Exceptions;
    using StockApp.Models;

    public class StoreService : IStoreService
    {
        private readonly IGemStoreService _gemStoreService;

        public StoreService(IGemStoreService gemStoreService)
        {
            _gemStoreService = gemStoreService ?? throw new ArgumentNullException(nameof(gemStoreService));
        }

        /// <summary>
        /// Retrieves the CNP for the current user.
        /// </summary>
        /// <returns></returns>
        public string GetCnp()
        {
            return _gemStoreService.GetCnp();
        }

        /// <summary>
        /// Checks if the user is a guest.
        /// </summary>
        /// <param name="cnp"></param>
        /// <returns></returns>
        public bool IsGuest(string cnp)
        {
            return _gemStoreService.IsGuest(cnp);
        }

        /// <summary>
        /// Retrieves the current gem balance for the specified user.
        /// </summary>
        /// <param name="cnp"></param>
        /// <returns></returns>
        public int GetUserGemBalance(string cnp)
        {
            return _gemStoreService.GetUserGemBalance(cnp);
        }

        /// <summary>
        /// Updates the gem balance for a given user.
        /// </summary>
        /// <param name="cnp"></param>
        /// <param name="newBalance"></param>
        public void UpdateUserGemBalance(string cnp, int newBalance)
        {
            _gemStoreService.UpdateUserGemBalance(cnp, newBalance);
        }

        /// <summary>
        /// Processes a bank transaction for buying or selling gems.
        /// </summary>
        /// <param name="deal">The gem deal to purchase.</param>
        /// <param name="selectedAccountId">The selected account ID for the transaction.</param>
        /// <returns>A message indicating the result of the transaction.</returns>
        /// <exception cref="GuestUserOperationException">Thrown when a guest user attempts to buy gems.</exception>
        /// <exception cref="GemTransactionFailedException">Thrown when the bank transaction fails.</exception>
        public async Task<string> BuyGemsAsync(GemDeal deal, string selectedAccountId)
        {
            if (await _gemStoreService.IsCurrentUserGuestAsync())
            {
                throw new GuestUserOperationException("Guests cannot buy gems.");
            }

            bool transactionSuccess = await ProcessBankTransaction(selectedAccountId, -deal.Price);
            if (!transactionSuccess)
            {
                throw new GemTransactionFailedException("Transaction failed. Please check your bank account balance.");
            }

            int currentBalance = await _gemStoreService.GetCurrentUserGemBalanceAsync();
            await _gemStoreService.UpdateCurrentUserGemBalanceAsync(currentBalance + deal.GemAmount);

            return $"Successfully purchased {deal.GemAmount} gems for {deal.Price}€";
        }

        /// <summary>
        /// Processes a bank transaction for selling gems.
        /// </summary>
        /// <param name="gemAmount">The amount of gems to sell.</param>
        /// <param name="selectedAccountId">The selected account ID for the transaction.</param>
        /// <returns>A message indicating the result of the transaction.</returns>
        /// <exception cref="GuestUserOperationException">Thrown when a guest user attempts to sell gems.</exception>
        /// <exception cref="InsufficientGemsException">Thrown when the user doesn't have enough gems to sell.</exception>
        /// <exception cref="GemTransactionFailedException">Thrown when the bank transaction fails.</exception>
        public async Task<string> SellGemsAsync(int gemAmount, string selectedAccountId)
        {
            if (await _gemStoreService.IsCurrentUserGuestAsync())
            {
                throw new GuestUserOperationException("Guests cannot sell gems.");
            }

            int currentBalance = await _gemStoreService.GetCurrentUserGemBalanceAsync();
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

            await _gemStoreService.UpdateCurrentUserGemBalanceAsync(currentBalance - gemAmount);
            return $"Successfully sold {gemAmount} gems for {moneyEarned}€";
        }

        private static async Task<bool> ProcessBankTransaction(string accountId, double amount)
        {
            // TODO: Implement actual bank transaction logic
            return true;
        }
    }
}
