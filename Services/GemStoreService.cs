using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockApp.Models;
using StockApp.Exceptions;
using StockApp.Repositories;

namespace StockApp.Services
{
    public class GemStoreService : IGemStoreService
    {
        private readonly IGemStoreApiService _apiService;
        private readonly IUserRepository _userRepository;

        public GemStoreService(IGemStoreApiService apiService, IUserRepository userRepository)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public string GetCnp()
        {
            return _userRepository.CurrentUserCNP;
        }

        public bool IsGuest(string cnp)
        {
            try
            {
                _apiService.GetGemStoreByCnpAsync(cnp).GetAwaiter().GetResult();
                return false;
            }
            catch (KeyNotFoundException)
            {
                return true;
            }
        }

        public int GetUserGemBalance(string cnp)
        {
            return _apiService.GetGemBalanceAsync(cnp).GetAwaiter().GetResult();
        }

        public void UpdateUserGemBalance(string cnp, int newBalance)
        {
            if (newBalance < 0)
            {
                throw new ArgumentException("Gem balance cannot be negative", nameof(newBalance));
            }

            _apiService.UpdateGemBalanceAsync(cnp, newBalance).GetAwaiter().GetResult();
        }

        public async Task<int> GetCurrentUserGemBalanceAsync()
        {
            try
            {
                var currentUserCnp = _userRepository.CurrentUserCNP;
                if (string.IsNullOrEmpty(currentUserCnp))
                {
                    throw new InvalidOperationException("No user is currently logged in");
                }

                return await _apiService.GetGemBalanceAsync(currentUserCnp);
            }
            catch (Exception ex)
            {
                throw new GemStorePersistenceException("Error retrieving gem balance", ex);
            }
        }

        public async Task UpdateCurrentUserGemBalanceAsync(int newBalance)
        {
            try
            {
                if (newBalance < 0)
                {
                    throw new ArgumentException("Gem balance cannot be negative", nameof(newBalance));
                }

                var currentUserCnp = _userRepository.CurrentUserCNP;
                if (string.IsNullOrEmpty(currentUserCnp))
                {
                    throw new InvalidOperationException("No user is currently logged in");
                }

                await _apiService.UpdateGemBalanceAsync(currentUserCnp, newBalance);
            }
            catch (Exception ex)
            {
                throw new GemStorePersistenceException("Error updating gem balance", ex);
            }
        }

        public async Task<bool> IsCurrentUserGuestAsync()
        {
            try
            {
                var currentUserCnp = _userRepository.CurrentUserCNP;
                if (string.IsNullOrEmpty(currentUserCnp))
                {
                    return true;
                }

                try
                {
                    await _apiService.GetGemStoreByCnpAsync(currentUserCnp);
                    return false;
                }
                catch (KeyNotFoundException)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new GemStorePersistenceException("Error checking if user is guest", ex);
            }
        }

        public async Task InitializeGemStoreForCurrentUserAsync(int initialBalance = 0)
        {
            try
            {
                var currentUserCnp = _userRepository.CurrentUserCNP;
                if (string.IsNullOrEmpty(currentUserCnp))
                {
                    throw new InvalidOperationException("No user is currently logged in");
                }

                await _apiService.CreateGemStoreAsync(currentUserCnp, initialBalance);
            }
            catch (Exception ex)
            {
                throw new GemStorePersistenceException("Error initializing gem store", ex);
            }
        }
    }
} 