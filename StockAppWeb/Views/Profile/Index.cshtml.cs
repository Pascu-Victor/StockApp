using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockAppWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace StockAppWeb.Views.Profile
{
    public class IndexModel
    {
        private readonly IUserService _userService;
        private readonly IStockService _stockService;
        private readonly IAuthenticationService _authenticationService;

        public IndexModel(IUserService userService, IStockService stockService, IAuthenticationService authenticationService)
        {
            _userService = userService;
            _stockService = stockService;
            _authenticationService = authenticationService;
        }

        public string UserName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsHidden { get; set; }
        public List<Stock> UserStocks { get; set; } = new();
        public Stock? SelectedStock { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsAuthenticated => _authenticationService.IsUserLoggedIn();

        public async Task OnGetAsync()
        {
            try
            {
                if (!_authenticationService.IsUserLoggedIn())
                {
                    ErrorMessage = "You must be logged in to view your profile.";
                    return;
                }

                var user = await _userService.GetCurrentUserAsync();
                UserName = user.UserName ?? string.Empty;
                ImageUrl = user.Image ?? string.Empty;
                Description = user.Description ?? string.Empty;
                IsAdmin = _authenticationService.IsUserAdmin();
                IsHidden = user.IsHidden;
                
                // Load user stocks
                UserStocks = await _stockService.UserStocksAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load profile data: {ex.Message}";
            }
        }
    }
} 