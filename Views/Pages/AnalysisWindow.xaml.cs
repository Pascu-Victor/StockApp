namespace StockApp.Views.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using LiveChartsCore;
    using LiveChartsCore.SkiaSharpView;
    using LiveChartsCore.SkiaSharpView.Painting;
    using LiveChartsCore.Defaults;
    using SkiaSharp;
    using Src.Data;
    using Src.Model;
    using StockApp.Models;
    using StockApp.Repositories;
    using StockApp.Services;
    using StockApp.ViewModels;

    public sealed partial class AnalysisWindow : Window
    {
        private readonly User _user;
        private readonly ActivityViewModel _activityViewModel;
        private readonly IHistoryService _historyService;

        public AnalysisWindow(User selectedUser)
        {
            this.InitializeComponent();
            _user = selectedUser ?? throw new ArgumentNullException(nameof(selectedUser));
            
            // Get services from DI container
            var activityService = App.Host.Services.GetRequiredService<IActivityService>();
            _historyService = App.Host.Services.GetRequiredService<IHistoryService>();

            _activityViewModel = new ActivityViewModel(activityService);
            
            LoadUserData();
            LoadHistory(_historyService.GetHistoryMonthly(_user.CNP));
            _ = LoadUserActivitiesAsync(); // Fire and forget, but in a real app you might want to handle this differently
        }

        private void LoadUserData()
        {
            IdTextBlock.Text = $"Id: {_user.Id}";
            FirstNameTextBlock.Text = $"First name: {_user.FirstName}";
            LastNameTextBlock.Text = $"Last name: {_user.LastName}";
            CNPTextBlock.Text = $"CNP: {_user.CNP}";
            EmailTextBlock.Text = $"Email: {_user.Email}";
            PhoneNumberTextBlock.Text = $"Phone number: {_user.PhoneNumber}";
        }

        private async Task LoadUserActivitiesAsync()
        {
            try
            {
                await _activityViewModel.LoadActivitiesForUserAsync(_user.CNP);
                ActivityListView.ItemsSource = _activityViewModel.Activities;
            }
            catch (Exception ex)
            {
                // In a real app, you would want to show this error to the user in a more user-friendly way
                System.Diagnostics.Debug.WriteLine($"Error loading activities: {ex.Message}");
            }
        }

        private void LoadHistory(List<CreditScoreHistory> history)
        {
            var points = new List<DateTimePoint>();
            foreach (var item in history)
            {
                points.Add(new DateTimePoint(item.Date.ToDateTime(TimeOnly.MinValue), item.Score));
            }

            var series = new ISeries[]
            {
                new LineSeries<DateTimePoint>
                {
                    Values = points,
                    GeometrySize = 8,
                    Stroke = new SolidColorPaint(SKColors.Blue, 2),
                    GeometryStroke = new SolidColorPaint(SKColors.Blue, 2),
                    Fill = null
                }
            };

            CreditScorePlotView.Series = series;
            CreditScorePlotView.XAxes = new[]
            {
                new Axis
                {
                    Labeler = value => new DateTime((long)value).ToString("MM/dd/yyyy"),
                    UnitWidth = TimeSpan.FromDays(1).Ticks
                }
            };
            CreditScorePlotView.YAxes = new[]
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 1000
                }
            };
        }

        private async void OnWeeklyButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var history = _historyService.GetHistoryWeekly(_user.CNP);
                LoadHistory(history);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading weekly history: {ex.Message}");
            }
        }

        private async void OnMonthlyButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var history = _historyService.GetHistoryMonthly(_user.CNP);
                LoadHistory(history);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading monthly history: {ex.Message}");
            }
        }

        private async void OnYearlyButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var history = _historyService.GetHistoryYearly(_user.CNP);
                LoadHistory(history);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading yearly history: {ex.Message}");
            }
        }
    }
}
