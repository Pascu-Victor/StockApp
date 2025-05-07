namespace StockApp.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using StockApp.Models;
    using StockApp.Services;

    public class HistoryViewModel : INotifyPropertyChanged
    {
        private readonly IHistoryService _historyService;
        private List<CreditScoreHistory> _weeklyHistory;
        private List<CreditScoreHistory> _monthlyHistory;
        private List<CreditScoreHistory> _yearlyHistory;
        private string _userCnp;

        public event PropertyChangedEventHandler PropertyChanged;

        public HistoryViewModel(IHistoryService historyService)
        {
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }

        public string UserCnp
        {
            get => _userCnp;
            set
            {
                if (_userCnp != value)
                {
                    _userCnp = value;
                    LoadHistory();
                    OnPropertyChanged();
                }
            }
        }

        public List<CreditScoreHistory> WeeklyHistory
        {
            get => _weeklyHistory;
            private set
            {
                if (_weeklyHistory != value)
                {
                    _weeklyHistory = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<CreditScoreHistory> MonthlyHistory
        {
            get => _monthlyHistory;
            private set
            {
                if (_monthlyHistory != value)
                {
                    _monthlyHistory = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<CreditScoreHistory> YearlyHistory
        {
            get => _yearlyHistory;
            private set
            {
                if (_yearlyHistory != value)
                {
                    _yearlyHistory = value;
                    OnPropertyChanged();
                }
            }
        }

        private void LoadHistory()
        {
            if (string.IsNullOrWhiteSpace(_userCnp))
                return;

            try
            {
                WeeklyHistory = _historyService.GetHistoryWeekly(_userCnp);
                MonthlyHistory = _historyService.GetHistoryMonthly(_userCnp);
                YearlyHistory = _historyService.GetHistoryYearly(_userCnp);
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                System.Diagnostics.Debug.WriteLine($"Error loading history: {ex.Message}");
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
