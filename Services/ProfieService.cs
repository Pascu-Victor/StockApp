using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockApp.Models;
using StockApp.Repositories;
using StockApp.Exceptions;

namespace StockApp.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly string _currentUserCnp;

        public ProfileService(IProfileRepository profileRepository, string currentUserCnp)
        {
            _profileRepository = profileRepository ?? throw new ArgumentNullException(nameof(profileRepository));
            _currentUserCnp = currentUserCnp ?? throw new ArgumentNullException(nameof(currentUserCnp));
        }

        public async Task<Profile> GetCurrentProfileAsync()
        {
            return await _profileRepository.GetUserProfileAsync(_currentUserCnp);
        }

        public async Task<Profile> GetProfileByCnpAsync(string cnp)
        {
            return await _profileRepository.GetUserProfileAsync(cnp);
        }

        public async Task<string> GenerateUsernameAsync()
        {
            return await _profileRepository.GenerateUsernameAsync();
        }

        public async Task UpdateProfileAsync(string cnp, string newUsername, string newImage, string newDescription, bool newHidden)
        {
            await _profileRepository.UpdateProfileAsync(cnp, newUsername, newImage, newDescription, newHidden);
        }

        public async Task UpdateIsAdminAsync(string cnp, bool isAdmin)
        {
            await _profileRepository.UpdateIsAdminAsync(cnp, isAdmin);
        }

        public async Task<List<Stock>> GetUserStocksAsync(string cnp)
        {
            return await _profileRepository.GetUserStocksAsync(cnp);
        }

        public string GetLoggedInUserCnp() => _currentUserCnp;

        public string GetUsername() => _profileRepository.CurrentUser().Username;

        public string GetDescription() => _profileRepository.CurrentUser().Description;

        public bool IsHidden() => _profileRepository.CurrentUser().IsHidden;

        public bool IsAdmin() => _profileRepository.CurrentUser().IsModerator;

        public List<Stock> GetUserStocks() => _profileRepository.GetUserStocksAsync(_currentUserCnp).Result;

        public void UpdateIsAdmin(bool newIsAdmin)
        {
            _profileRepository.UpdateIsAdminAsync(_currentUserCnp, newIsAdmin).Wait();
        }

        public string GetImage() => _profileRepository.CurrentUser().Image;

        public void UpdateUser(string newUsername, string newImage, string newDescription, bool newHidden)
        {
            _profileRepository.UpdateProfileAsync(_currentUserCnp, newUsername, newImage, newDescription, newHidden).Wait();
        }
    }
} 