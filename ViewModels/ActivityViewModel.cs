namespace StockApp.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using StockApp.Models;
    using StockApp.Services;

    /// <summary>
    /// ViewModel for managing user activities, providing data binding and command handling for activity-related operations.
    /// </summary>
    public class ActivityViewModel : ViewModelBase
    {
        private readonly IActivityService _activityService;
        private ObservableCollection<ActivityLog> _activities;
        private string _userCnp;
        private bool _isLoading;
        private string _errorMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityViewModel"/> class.
        /// </summary>
        /// <param name="activityService">The service for managing activities.</param>
        public ActivityViewModel(IActivityService activityService)
        {
            _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
            _activities = new ObservableCollection<ActivityLog>();
        }

        /// <summary>
        /// Gets or sets the collection of activities.
        /// </summary>
        public ObservableCollection<ActivityLog> Activities
        {
            get => _activities;
            set => SetProperty(ref _activities, value);
        }

        /// <summary>
        /// Gets or sets the user's CNP identifier.
        /// </summary>
        public string UserCnp
        {
            get => _userCnp;
            set
            {
                if (SetProperty(ref _userCnp, value))
                {
                    LoadActivitiesAsync();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether activities are being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Gets or sets the current error message.
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// Loads activities for the current user asynchronously.
        /// </summary>
        public async Task LoadActivitiesAsync()
        {
            if (string.IsNullOrWhiteSpace(_userCnp))
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var activities = await _activityService.GetActivityForUser(_userCnp);
                Activities.Clear();
                foreach (var activity in activities)
                {
                    Activities.Add(activity);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading activities: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Adds a new activity asynchronously.
        /// </summary>
        /// <param name="activityName">The name of the activity.</param>
        /// <param name="amount">The amount associated with the activity.</param>
        /// <param name="details">Additional details about the activity.</param>
        public async Task AddActivityAsync(string activityName, int amount, string details)
        {
            if (string.IsNullOrWhiteSpace(_userCnp))
            {
                ErrorMessage = "User CNP is not set";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var activity = await _activityService.AddActivity(_userCnp, activityName, amount, details);
                Activities.Insert(0, activity); // Add to the beginning of the collection
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding activity: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
