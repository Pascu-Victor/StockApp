﻿using System.Collections.Generic;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Services
{
    public interface IProfileService
    {
        Task<User> CurrentUserProfile { get; }

        Task UpdateIsAdminAsync(bool newIsAdmin);

        Task UpdateUserAsync(string newUsername, string newImage, string newDescription, bool newHidden);

        Task<List<Stock>> GetUserStocksAsync();
    }
}