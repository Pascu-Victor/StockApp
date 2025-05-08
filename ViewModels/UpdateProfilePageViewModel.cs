namespace StockApp.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using StockApp.Models;
    using StockApp.Services;
    using StockApp.Repositories;
    using StockApp.Database;

    /// <summary>
    /// ViewModel for updating user profile details including username, image, description, and visibility.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="UpdateProfilePageViewModel"/> class with a specified service.
    /// </remarks>
    /// <param name="service">Service used to retrieve and update profile information.</param>
    public class UpdateProfilePageViewModel : ViewModelBase
    {
        private readonly IProfileService _profileService;
        private string _username;
        private string _description;
        private string _image;
        private bool _isProfileHidden;
        private bool _isLoading;
        private string _errorMessage;
        private Profile _currentProfile;

        public UpdateProfilePageViewModel(string cnp)
        {
            var dbContext = new AppDbContext();
            var profileRepository = new ProfileRepository(dbContext, cnp);
            _profileService = new ProfileService(profileRepository, cnp);

            LoadProfileCommand = new RelayCommand(async _ => await LoadProfileAsync());
            UpdateProfileCommand = new RelayCommand(async _ => await UpdateProfileAsync());
            GenerateUsernameCommand = new RelayCommand(async _ => await GenerateUsernameAsync());
        }

        public ICommand LoadProfileCommand { get; }
        public ICommand UpdateProfileCommand { get; }
        public ICommand GenerateUsernameCommand { get; }

        public Profile CurrentProfile
        {
            get => _currentProfile;
            set
            {
                _currentProfile = value;
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public string Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        public bool IsProfileHidden
        {
            get => _isProfileHidden;
            set
            {
                _isProfileHidden = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProfilePageViewModel"/> class with the default profile service.
        /// </summary>
        public UpdateProfilePageViewModel()
            : this("default_user_cnp") // Using a default CNP for initialization
        {
            // Default constructor chaining to use ProfileService implementation
        }

        /// <summary>
        /// Gets the URL of the user's profile image.
        /// </summary>
        /// <returns>The image URL as a string.</returns>
        public string GetImage()
        {
            // Inline: delegate image retrieval to service
            return this._profileService.GetImage();
        }

        /// <summary>
        /// Gets the username of the current user.
        /// </summary>
        /// <returns>The username as a string.</returns>
        public string GetUsername()
        {
            // Inline: delegate username retrieval to service
            return this._profileService.GetUsername();
        }

        /// <summary>
        /// Gets the description text for the current user.
        /// </summary>
        /// <returns>The description as a string.</returns>
        public string GetDescription()
        {
            // Inline: delegate description retrieval to service
            return this._profileService.GetDescription();
        }

        /// <summary>
        /// Determines whether the user's profile is hidden.
        /// </summary>
        /// <returns><c>true</c> if hidden; otherwise, <c>false</c>.</returns>
        public bool IsHidden()
        {
            // Inline: delegate visibility check to service
            return this._profileService.IsHidden();
        }

        /// <summary>
        /// Determines whether the current user has administrative privileges.
        /// </summary>
        /// <returns><c>true</c> if admin; otherwise, <c>false</c>.</returns>
        public bool IsAdmin()
        {
            // Inline: delegate admin check to service
            return this._profileService.IsAdmin();
        }

        /// <summary>
        /// Gets the list of stocks associated with the user.
        /// </summary>
        /// <returns>A list of <see cref="Stock"/> objects.</returns>
        public List<Stock> GetUserStocks()
        {
            // Inline: retrieve user's stocks from service
            return this._profileService.GetUserStocks();
        }

        /// <summary>
        /// Updates all user profile fields at once.
        /// </summary>
        /// <param name="newUsername">The new username.</param>
        /// <param name="newImage">The new profile image URL.</param>
        /// <param name="newDescription">The new description text.</param>
        /// <param name="newHidden">New hidden status for the profile.</param>
        public void UpdateAll(string newUsername, string newImage, string newDescription, bool newHidden)
        {
            // TODO: Validate inputs (e.g., non-null, length constraints)
            // FIXME: Consider handling exceptions from service to provide user feedback
            this._profileService.UpdateUser(newUsername, newImage, newDescription, newHidden); // Inline: perform bulk update
        }

        /// <summary>
        /// Updates only the administrative mode of the user.
        /// </summary>
        /// <param name="newIsAdmin"><c>true</c> to grant admin; otherwise, <c>false</c>.</param>
        public void UpdateAdminMode(bool newIsAdmin)
        {
            // Inline: delegate admin mode toggle to service
            this._profileService.UpdateIsAdmin(newIsAdmin);
        }

        private async Task LoadProfileAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                CurrentProfile = await _profileService.GetCurrentProfileAsync();
                Username = CurrentProfile.Username;
                Description = CurrentProfile.Description;
                Image = CurrentProfile.Image;
                IsProfileHidden = CurrentProfile.IsHidden;
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
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                await _profileService.UpdateProfileAsync(
                    _profileService.GetLoggedInUserCnp(),
                    Username,
                    Image,
                    Description,
                    IsProfileHidden
                );
                await LoadProfileAsync();
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

        private async Task GenerateUsernameAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                CurrentProfile.Username = await _profileService.GenerateUsernameAsync();
                OnPropertyChanged(nameof(CurrentProfile));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error generating username: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
