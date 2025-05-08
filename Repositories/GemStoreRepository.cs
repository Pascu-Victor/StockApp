using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StockApp.Database;
using StockApp.Models;
using StockApp.Exceptions;

namespace StockApp.Repositories
{
    /// <summary>
    /// Repository for retrieving and updating user gem balances and CNP values.
    /// </summary>
    public class GemStoreRepository : IGemStoreRepository
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GemStoreRepository"/> class with the specified database context.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="userRepository">The user repository for accessing current user information.</param>
        public GemStoreRepository(AppDbContext context, IUserRepository userRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Retrieves the CNP for the current user.
        /// </summary>
        /// <returns>The CNP string, or empty if not found.</returns>
        public string GetCnp()
        {
            return _userRepository.CurrentUserCNP;
        }

        /// <summary>
        /// Retrieves the current gem balance for the specified user.
        /// </summary>
        /// <param name="cnp">User identifier (CNP).</param>
        /// <returns>User's gem balance as an integer.</returns>
        public async Task<int> GetUserGemBalanceAsync(string cnp)
        {
            try
            {
                var gemStore = await _context.GemStores
                    .FirstOrDefaultAsync(g => g.UserCnp == cnp);

                return gemStore?.GemBalance ?? 0;
            }
            catch (Exception ex)
            {
                throw new RepositoryPersistenceException("Error retrieving gem balance", ex);
            }
        }

        /// <summary>
        /// Updates the gem balance for a given user.
        /// </summary>
        /// <param name="cnp">User identifier (CNP).</param>
        /// <param name="newBalance">New gem balance to set.</param>
        public async Task UpdateUserGemBalanceAsync(string cnp, int newBalance)
        {
            try
            {
                var gemStore = await _context.GemStores
                    .FirstOrDefaultAsync(g => g.UserCnp == cnp);

                if (gemStore == null)
                {
                    gemStore = new GemStore
                    {
                        UserCnp = cnp,
                        GemBalance = newBalance,
                        LastUpdated = DateTime.UtcNow
                    };
                    await _context.GemStores.AddAsync(gemStore);
                }
                else
                {
                    gemStore.GemBalance = newBalance;
                    gemStore.LastUpdated = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new RepositoryPersistenceException("Error updating gem balance", ex);
            }
        }

        /// <summary>
        /// Determines if the specified user is considered a guest (no record in the database).
        /// </summary>
        /// <param name="cnp">User identifier (CNP).</param>
        /// <returns><c>true</c> if no user record exists; otherwise, <c>false</c>.</returns>
        public async Task<bool> IsGuestAsync(string cnp)
        {
            try
            {
                return !await _context.GemStores.AnyAsync(g => g.UserCnp == cnp);
            }
            catch (Exception ex)
            {
                throw new RepositoryPersistenceException("Error checking if user is guest", ex);
            }
        }
    }
}
