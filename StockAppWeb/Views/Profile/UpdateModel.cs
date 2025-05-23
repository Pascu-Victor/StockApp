using Common.Services;
using StockAppWeb.Models;
using System.ComponentModel.DataAnnotations;

namespace StockAppWeb.Views.Profile
{
    public class UpdateModel
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;

        public UpdateModel(IUserService userService, IAuthenticationService authenticationService)
        {
            _userService = userService;
            _authenticationService = authenticationService;
        }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public InputModel Input { get; set; } = new InputModel();
        public bool IsAuthenticated => _authenticationService.IsUserLoggedIn();

        public class InputModel
        {
            [Required(ErrorMessage = "Username is required")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Image URL is required")]
            [Url(ErrorMessage = "Please enter a valid URL for the profile image")]
            public string ImageUrl { get; set; } = string.Empty;

            [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
            public string Description { get; set; } = string.Empty;

            public bool ClearDescription { get; set; }

            public bool IsHidden { get; set; }
        }

        public async Task OnGetAsync()
        {
            try
            {
                if (!_authenticationService.IsUserLoggedIn())
                {
                    ErrorMessage = "You must be logged in to update your profile.";
                    return;
                }

                var user = await _userService.GetCurrentUserAsync();
                Input = new InputModel
                {
                    Username = user.UserName ?? string.Empty,
                    ImageUrl = user.Image ?? string.Empty,
                    Description = user.Description ?? string.Empty,
                    IsHidden = user.IsHidden
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load profile data: {ex.Message}";
            }
        }

        public async Task OnPostAsync(InputModel input)
        {
            try
            {
                if (!_authenticationService.IsUserLoggedIn())
                {
                    ErrorMessage = "You must be logged in to update your profile.";
                    return;
                }

                string description = input.ClearDescription ? string.Empty : input.Description;

                await _userService.UpdateUserAsync(
                    input.Username,
                    input.ImageUrl,
                    description,
                    input.IsHidden
                );

                SuccessMessage = "Profile updated successfully!";
                Input = input; // Repopulate the input model with the submitted values
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update profile: {ex.Message}";
            }
        }
    }
} 