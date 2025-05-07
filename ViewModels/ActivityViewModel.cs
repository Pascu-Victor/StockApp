namespace StockApp.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.UI.Dispatching;
    using Src.Model;
    using StockApp.Services;

    public class ActivityViewModel : INotifyPropertyChanged
    {
        private readonly IActivityService _activityService;
        private readonly DispatcherQueue _dispatcherQueue;
        private ObservableCollection<ActivityLog> _activities;
        private bool _isLoading;
        private string _errorMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public ActivityViewModel(IActivityService activityService)
        {
            _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _activities = new ObservableCollection<ActivityLog>();
        }

        public ObservableCollection<ActivityLog> Activities
        {
            get => _activities;
            set
            {
                _activities = value;
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

        public async Task LoadActivitiesForUserAsync(string userCnp)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var activities = await _activityService.GetActivityForUserAsync(userCnp);
                
                _dispatcherQueue.TryEnqueue(() =>
                {
                    Activities = new ObservableCollection<ActivityLog>(activities);
                });
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

        public async Task CreateActivityAsync(ActivityLog activity)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var createdActivity = await _activityService.CreateActivityAsync(activity);
                
                _dispatcherQueue.TryEnqueue(() =>
                {
                    Activities.Insert(0, createdActivity);
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating activity: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task DeleteActivityAsync(int id)
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await _activityService.DeleteActivityAsync(id);
                
                _dispatcherQueue.TryEnqueue(() =>
                {
                    var activityToRemove = Activities.FirstOrDefault(a => a.Id == id);
                    if (activityToRemove != null)
                    {
                        Activities.Remove(activityToRemove);
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting activity: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
