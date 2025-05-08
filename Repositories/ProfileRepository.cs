using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StockApp.Database;
using StockApp.Models;
using StockApp.Exceptions;

namespace StockApp.Repositories
{
    /// <summary>
    /// Repository for managing user profiles and related operations.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProfileRepository"/> class.
    /// </remarks>
    /// <param name="currentUserCnp">The CNP of the user whose profile is being managed.</param>
    public class ProfileRepository : IProfileRepository
    {
        private readonly AppDbContext _context;
        private readonly string _currentUserCnp;

        public ProfileRepository(AppDbContext context, string currentUserCnp)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentUserCnp = currentUserCnp ?? throw new ArgumentNullException(nameof(currentUserCnp));
        }

        /// <summary>
        /// Gets the current user's profile.
        /// </summary>
        /// <returns>A <see cref="User"/> object representing the current user.</returns>
        public Profile CurrentUser()
        {
            return _context.Profiles
                .FirstOrDefault(p => p.CNP == _currentUserCnp)
                ?? throw new ProfileNotFoundException($"Profile not found for CNP: {_currentUserCnp}");
        }

        /// <summary>
        /// Gets the profile of a user by their CNP.
        /// </summary>
        /// <param name="cnp">The CNP of the user whose profile is being retrieved.</param>
        /// <returns>A <see cref="User"/> object representing the user.</returns>
        public async Task<Profile> GetUserProfileAsync(string cnp)
        {
            var profile = await _context.Profiles
                .Include(p => p.UserStocks)
                .FirstOrDefaultAsync(p => p.CNP == cnp);

            if (profile == null)
            {
                throw new ProfileNotFoundException($"Profile with CNP {cnp} not found.");
            }

            return profile;
        }

        /// <summary>
        /// Generates a random username from a predefined list.
        /// </summary>
        /// <returns>A randomly selected username.</returns>
        public async Task<string> GenerateUsernameAsync()
        {
            var random = new Random();
            string username;
            do
            {
                username = $"user{random.Next(1000, 9999)}";
            } while (await _context.Profiles.AnyAsync(p => p.Username == username));

            return username;
        }

        /// <summary>
        /// Updates the profile of the current user.
        /// </summary>
        /// <param name="cnp">The CNP of the user whose profile is being updated.</param>
        /// <param name="newUsername">The new username.</param>
        /// <param name="newImage">The new profile picture URL.</param>
        /// <param name="newDescription">The new description.</param>
        /// <param name="newHidden">A value indicating whether the profile should be hidden.</param>
        public async Task UpdateProfileAsync(string cnp, string newUsername, string newImage, string newDescription, bool newHidden)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.CNP == cnp);
            if (profile == null)
            {
                throw new ProfileNotFoundException($"Profile with CNP {cnp} not found.");
            }

            profile.Username = newUsername;
            profile.Image = newImage;
            profile.Description = newDescription;
            profile.IsHidden = newHidden;
            profile.LastModifiedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ProfilePersistenceException("Failed to update profile.", ex);
            }
        }

        /// <summary>
        /// Updates the admin status of the current user.
        /// </summary>
        /// <param name="cnp">The CNP of the user whose admin status is being updated.</param>
        /// <param name="isAdmin">A value indicating whether the user should be an admin.</param>
        public async Task UpdateIsAdminAsync(string cnp, bool isAdmin)
        {
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.CNP == cnp);
            if (profile == null)
            {
                throw new ProfileNotFoundException($"Profile with CNP {cnp} not found.");
            }

            profile.IsModerator = isAdmin;
            profile.LastModifiedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ProfilePersistenceException("Failed to update admin status.", ex);
            }
        }

        /// <summary>
        /// Gets the list of stocks owned by the current user.
        /// </summary>
        /// <param name="cnp">The CNP of the user whose stocks are being retrieved.</param>
        /// <returns>A list of the user's stocks.</returns>
        public async Task<List<Stock>> GetUserStocksAsync(string cnp)
        {
            var profile = await _context.Profiles
                .Include(p => p.UserStocks)
                .FirstOrDefaultAsync(p => p.CNP == cnp);

            if (profile == null)
            {
                throw new ProfileNotFoundException($"Profile with CNP {cnp} not found.");
            }

            return profile.UserStocks?.ToList() ?? new List<Stock>();
        }

        private static User MapToUser(Profile profile)
        {
            return new User
            {
                Id = profile.Id,
                CNP = profile.CNP,
                Username = profile.Username,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                Description = profile.Description,
                IsModerator = profile.IsModerator,
                Image = profile.Image,
                IsHidden = profile.IsHidden,
                GemBalance = profile.GemBalance,
                NumberOfOffenses = profile.NumberOfOffenses,
                RiskScore = profile.RiskScore,
                ROI = profile.ROI,
                CreditScore = profile.CreditScore,
                Birthday = profile.Birthday,
                ZodiacSign = profile.ZodiacSign,
                ZodiacAttribute = profile.ZodiacAttribute,
                NumberOfBillSharesPaid = profile.NumberOfBillSharesPaid,
                Income = profile.Income,
                Balance = profile.Balance,
                HashedPassword = profile.HashedPassword
            };
        }
    }
}