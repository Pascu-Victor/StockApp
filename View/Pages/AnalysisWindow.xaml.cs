namespace Src.View.Pages
{
    using System;
    using System.Collections.Generic;
    using Microsoft.UI.Xaml;
    using Src.Data;
    using Src.Model;
    using Src.Repos;
    using StockApp.Models;
    using StockApp.Services;

    public sealed partial class AnalysisWindow : Window
    {
        private User user;
        private readonly IActivityService activityService;
        private readonly IHistoryService historyService;

        public AnalysisWindow(User selectedUser)
        {
            this.InitializeComponent();
            user = selectedUser;
            activityService = new ActivityService(new ActivityRepository(new DatabaseConnection(), new UserRepository(new DatabaseConnection())));
            historyService = new HistoryService(new HistoryRepository(new DatabaseConnection()));
            LoadUserData();
            LoadHistory(historyService.GetHistoryMonthly(user.Cnp));
            LoadUserActivities();
        }

        public void LoadUserData()
        {
            IdTextBlock.Text = $"Id: {user.Id}";
            FirstNameTextBlock.Text = $"First name: {user.FirstName}";
            LastNameTextBlock.Text = $"Last name: {user.LastName}";
            CNPTextBlock.Text = $"CNP: {user.Cnp}";
            EmailTextBlock.Text = $"Email: {user.Email}";
            PhoneNumberTextBlock.Text = $"Phone number: {user.PhoneNumber}";
        }

        public void LoadUserActivities()
        {
            try
            {
                var activities = activityService.GetActivityForUser(user.Cnp);

                ActivityListView.ItemsSource = activities;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error loading activities: {exception.Message}");
            }
        }

        public void LoadHistory(List<CreditScoreHistory> history)
        {
            try
            {
                if (history.Count == 0)
                {
                    return;
                }
                var plotModel = new PlotModel { Title = string.Empty };

                var barSeries = new BarSeries
                {
                    Title = "Credit Score",
                    StrokeThickness = 1
                };

                for (int i = 0; i < history.Count; i++)
                {
                    var record = history[i];
                    OxyColor barColor;

                    if (i == 0)
                    {
                        barColor = OxyColor.FromRgb(0, 255, 0);
                    }
                    else
                    {
                        var previousRecord = history[i - 1];
                        if (record.Score > previousRecord.Score)
                        {
                            barColor = OxyColor.FromRgb(0, 255, 0);
                        }
                        else if (record.Score == previousRecord.Score)
                        {
                            barColor = OxyColor.FromRgb(255, 255, 0);
                        }
                        else
                        {
                            barColor = OxyColor.FromRgb(255, 0, 0);
                        }
                    }
                    barSeries.Items.Add(new BarItem
                    {
                        Value = record.Score,
                        Color = barColor
                    });
                }
                foreach (var record in history)
                {
                    barSeries.Items.Add(new BarItem { Value = record.Score });
                }
                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Left
                };
                foreach (var record in history)
                {
                    categoryAxis.Labels.Add(record.Date.ToString("MM/dd"));
                }

                plotModel.Axes.Add(categoryAxis);
                plotModel.Series.Add(barSeries);

                CreditScorePlotView.Model = plotModel;
                CreditScorePlotView.InvalidatePlot(true);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error loading credit score history: {exception.Message}");
            }
        }

        private async void OnMonthlyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var history = historyService.GetHistoryMonthly(user.Cnp);
                LoadHistory(history);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading credit score history: {ex.Message}");
            }
        }

        private async void OnYearlyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var history = historyService.GetHistoryYearly(user.Cnp);
                LoadHistory(history);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error loading credit score history: {exception.Message}");
            }
        }

        private async void OnWeeklyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var history = historyService.GetHistoryWeekly(user.Cnp);
                LoadHistory(history);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error loading credit score history: {exception.Message}");
            }
        }
    }
}
