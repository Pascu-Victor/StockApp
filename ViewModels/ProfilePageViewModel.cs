namespace StockApp.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.UI.Xaml.Media.Imaging;
    using StockApp.Models;
    using StockApp.Services;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using StockApp.Exceptions;
    using System.Collections.ObjectModel;

    /// <summary>
    /// View model for the profile page, managing the user's profile image and information.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ProfilePageViewModel"/> class with the specified profile service.
    /// </remarks>
    /// <param name="profileService">Service used to retrieve profile data.</param>
    public class ProfilePageViewModel : ViewModelBase
    {
        private readonly IProfileService _profileService;
        private Profile? _currentProfile;
        private ObservableCollection<Stock>? _userStocks;
        private bool _isLoading;
        private string? _errorMessage;
        private BitmapImage _imageSource;

        public ProfilePageViewModel(IProfileService profileService)
        {
            _profileService = profileService ?? throw new ArgumentNullException(nameof(profileService));
            LoadProfileCommand = new RelayCommand(async _ => await LoadProfileAsync());
            UpdateProfileCommand = new RelayCommand(async _ => await UpdateProfileAsync());
        }

        public Profile? CurrentProfile
        {
            get => _currentProfile;
            set
            {
                _currentProfile = value;
                OnPropertyChanged();
                if (value?.Image != null)
                {
                    ImageSource = new BitmapImage(new Uri(value.Image));
                }
            }
        }

        public ObservableCollection<Stock>? UserStocks
        {
            get => _userStocks;
            set
            {
                _userStocks = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage ImageSource
        {
            get => _imageSource;
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadProfileCommand { get; }
        public ICommand UpdateProfileCommand { get; }

        private async Task LoadProfileAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                CurrentProfile = await _profileService.GetCurrentProfileAsync();
                var stocks = await _profileService.GetUserStocksAsync(CurrentProfile.CNP);
                UserStocks = new ObservableCollection<Stock>(stocks);
            }
            catch (ProfileNotFoundException ex)
            {
                ErrorMessage = "Profile not found. Please try again.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateProfileAsync()
        {
            if (CurrentProfile == null) return;

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                await _profileService.UpdateProfileAsync(
                    CurrentProfile.CNP,
                    CurrentProfile.Username,
                    CurrentProfile.Image,
                    CurrentProfile.Description,
                    CurrentProfile.IsHidden
                );
                await LoadProfileAsync();
            }
            catch (ProfileNotFoundException ex)
            {
                ErrorMessage = "Profile not found. Please try again.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Gets the CNP of the currently logged-in user.
        /// </summary>
        /// <returns>The user's CNP as a string.</returns>
        public string GetLoggedInUserCnp()
        {
            return _profileService.GetLoggedInUserCnp();
        }

        /// <summary>
        /// Gets the username of the currently logged-in user.
        /// </summary>
        /// <returns>The username.</returns>
        public string GetUsername() => _profileService.GetUsername();

        /// <summary>
        /// Gets the description of the user.
        /// </summary>
        /// <returns>The user description.</returns>
        public string GetDescription() => _profileService.GetDescription();

        /// <summary>
        /// Determines whether the user's profile is hidden.
        /// </summary>
        /// <returns><c>true</c> if the profile is hidden; otherwise, <c>false</c>.</returns>
        public bool IsHidden() => _profileService.IsHidden();

        /// <summary>
        /// Determines whether the user has administrative privileges.
        /// </summary>
        /// <returns><c>true</c> if the user is an admin; otherwise, <c>false</c>.</returns>
        public bool IsAdmin() => _profileService.IsAdmin();

        /// <summary>
        /// Gets the list of stocks associated with the user.
        /// </summary>
        /// <returns>A list of <see cref="Stock"/> objects.</returns>
        public List<Stock> GetUserStocks() => _profileService.GetUserStocks();

        /// <summary>
        /// Updates the administrative mode of the user.
        /// </summary>
        /// <param name="newIsAdmin">If set to <c>true</c>, grants admin mode; otherwise, revokes it.</param>
        public void UpdateAdminMode(bool newIsAdmin)
        {
            _profileService.UpdateIsAdmin(newIsAdmin);
        }
    }
}
