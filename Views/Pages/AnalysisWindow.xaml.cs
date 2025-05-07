namespace StockApp.Views.Pages
{
    using System;
    using System.Collections.Generic;
    using Microsoft.UI.Xaml;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using StockApp.Database;
    using StockApp.Models;
    using StockApp.Repositories;
    using StockApp.Services;
    using System.Net.Http;
    using Src.Data;

    public sealed partial class AnalysisWindow : Window
    {
        private User user;
        private readonly IActivityService activityService;
        private readonly IHistoryService historyService;

        public AnalysisWindow(User selectedUser)
        {
            this.InitializeComponent();
            this.user = selectedUser;
            
            // Create HTTP client for HistoryApiService
            var httpClient = new HttpClient();
            var historyApiService = new HistoryApiService(httpClient, "http://localhost:5000");
            
            // Initialize services with correct dependencies
            this.activityService = new ActivityService(new ActivityRepository(new DatabaseConnection(), new UserRepository()));
            this.historyService = new HistoryService(historyApiService);
            
            this.LoadUserData();
            this.LoadHistory(this.historyService.GetHistoryMonthly(this.user.CNP));
            this.LoadUserActivities();
        }

        public void LoadUserData()
        {
            this.IdTextBlock.Text = $"Id: {this.user.Id}";
            this.FirstNameTextBlock.Text = $"First name: {this.user.FirstName}";
            this.LastNameTextBlock.Text = $"Last name: {this.user.LastName}";
            this.CNPTextBlock.Text = $"CNP: {this.user.CNP}";
            this.EmailTextBlock.Text = $"Email: {this.user.Email}";
            this.PhoneNumberTextBlock.Text = $"Phone number: {this.user.PhoneNumber}";
        }

        public void LoadUserActivities()
        {
            try
            {
                var activities = this.activityService.GetActivityForUser(this.user.CNP);

                this.ActivityListView.ItemsSource = activities;
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

                this.CreditScorePlotView.Model = plotModel;
                this.CreditScorePlotView.InvalidatePlot(true);
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
                var history = this.historyService.GetHistoryMonthly(this.user.CNP);
                this.LoadHistory(history);
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
                var history = this.historyService.GetHistoryYearly(this.user.CNP);
                this.LoadHistory(history);
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
                var history = this.historyService.GetHistoryWeekly(this.user.CNP);
                this.LoadHistory(history);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error loading credit score history: {exception.Message}");
            }
        }
    }
}
