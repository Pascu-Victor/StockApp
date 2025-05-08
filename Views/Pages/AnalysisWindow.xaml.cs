namespace StockApp.Views.Pages
{
    using System;
    using System.Collections.Generic;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using StockApp.Models;
    using StockApp.Services;

    public sealed partial class AnalysisWindow : Window
    {
        private User user;
        private readonly IActivityService activityService;
        private readonly IHistoryService historyService;
        private bool isLoading;

        public AnalysisWindow(User selectedUser, IActivityService activityService, IHistoryService historyService)
        {
            this.InitializeComponent();
            this.user = selectedUser;
            this.activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
            this.historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));

            this.LoadUserData();
            this.LoadHistory(this.historyService.GetHistoryMonthly(this.user.CNP));
            this.LoadUserActivitiesAsync();
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

        public async void LoadUserActivitiesAsync()
        {
            if (isLoading) return;

            try
            {
                isLoading = true;
                var activities = await this.activityService.GetActivityForUser(this.user.CNP);
                this.ActivityListView.ItemsSource = activities;
            }
            catch (Exception exception)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Error loading activities: {exception.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
            finally
            {
                isLoading = false;
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

                    barSeries.Items.Add(new BarItem(record.Score, i) { Color = barColor });
                }

                plotModel.Series.Add(barSeries);

                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Time"
                };

                for (int i = 0; i < history.Count; i++)
                {
                    categoryAxis.Labels.Add(history[i].Date.ToString("MM/dd/yyyy"));
                }

                plotModel.Axes.Add(categoryAxis);

                var valueAxis = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Score",
                    Minimum = 0,
                    Maximum = 100
                };

                plotModel.Axes.Add(valueAxis);

                this.CreditScorePlotView.Model = plotModel;
            }
            catch (Exception exception)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Error loading history: {exception.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                _ = dialog.ShowAsync();
            }
        }

        private void OnWeeklyClick(object sender, RoutedEventArgs e)
        {
            var history = this.historyService.GetHistoryWeekly(this.user.CNP);
            this.LoadHistory(history);
        }

        private void OnMonthlyClick(object sender, RoutedEventArgs e)
        {
            var history = this.historyService.GetHistoryMonthly(this.user.CNP);
            this.LoadHistory(history);
        }

        private void OnYearlyClick(object sender, RoutedEventArgs e)
        {
            var history = this.historyService.GetHistoryYearly(this.user.CNP);
            this.LoadHistory(history);
        }
    }
}
