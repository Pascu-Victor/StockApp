using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;
using StockApp.Model;
using StockApp.Service;

namespace StockApp.ViewModel
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly TransactionLogService service;

        private string _stockNameFilter;
        private ComboBoxItem _selectedTransactionType;
        private ComboBoxItem _selectedSortBy;
        private ComboBoxItem _selectedSortOrder;
        private ComboBoxItem _selectedExportFormat;
        private string _minTotalValue;
        private string _maxTotalValue;
        private DateTime? _startDate;
        private DateTime? _endDate;

        public event Action<string, string> ShowMessageBoxRequested;

        public ObservableCollection<TransactionLogTransaction> Transactions { get; set; } = new ObservableCollection<TransactionLogTransaction>();

        public string StockNameFilter
        {
            get => _stockNameFilter;
            set { _stockNameFilter = value; OnPropertyChanged(nameof(StockNameFilter)); }
        }

        public ComboBoxItem SelectedTransactionType
        {
            get => _selectedTransactionType;
            set
            {
                _selectedTransactionType = value;
                OnPropertyChanged(nameof(SelectedTransactionType));
                LoadTransactions(); // Reload transactions when the selected type changes
            }
        }

        public ComboBoxItem SelectedSortBy
        {
            get => _selectedSortBy;
            set
            {
                _selectedSortBy = value;
                OnPropertyChanged(nameof(SelectedSortBy));
                LoadTransactions(); // Reload transactions when the sorting criteria change
            }
        }

        public ComboBoxItem SelectedSortOrder
        {
            get => _selectedSortOrder;
            set
            {
                _selectedSortOrder = value;
                OnPropertyChanged(nameof(SelectedSortOrder));
                LoadTransactions(); // Reload transactions when the sort order changes
            }
        }

        public ComboBoxItem SelectedExportFormat
        {
            get => _selectedExportFormat;
            set
            {
                _selectedExportFormat = value;
                OnPropertyChanged(nameof(SelectedExportFormat));
            }
        }

        public string MinTotalValue
        {
            get => _minTotalValue;
            set
            {
                if (ValidateNumericValue(value))
                {
                    _minTotalValue = value;
                    OnPropertyChanged(nameof(MinTotalValue));
                    LoadTransactions();
                }
                else
                {
                    ShowMessageBox("Invalid Input", "Min Total Value must be a valid number.");
                }
            }
        }

        public string MaxTotalValue
        {
            get => _maxTotalValue;
            set
            {
                if (ValidateNumericValue(value))
                {
                    _maxTotalValue = value;
                    OnPropertyChanged(nameof(MaxTotalValue));
                    LoadTransactions();
                }
                else
                {
                    ShowMessageBox("Invalid Input", "Max Total Value must be a valid number.");
                }
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(nameof(StartDate)); LoadTransactions(); }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(nameof(EndDate)); LoadTransactions(); }
        }

        public ICommand SearchCommand { get; }
        public ICommand ExportCommand { get; }

        public ViewModel(TransactionLogService service)
        {
            this.service = service;

            // Initialize ComboBoxItems for options if they are null
            SelectedTransactionType = new ComboBoxItem { Content = "ALL" };
            SelectedSortBy = new ComboBoxItem { Content = "Date" };
            SelectedSortOrder = new ComboBoxItem { Content = "ASC" };
            SelectedExportFormat = new ComboBoxItem { Content = "CSV" };

            // Set up commands
            SearchCommand = new Command.Command(Search);
            ExportCommand = new Command.Command(async () => await Export());

            LoadTransactions();
        }

        private void Search()
        {
            LoadTransactions();
        }

        private async Task Export()
        {
            string format = SelectedExportFormat.Content?.ToString();
            string fileName = "transactions";

            // Save the file to the user's Documents folder
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string fullPath = Path.Combine(documentsPath, $"{fileName}.{format.ToLower()}");

            // Export the transactions
            service.ExportTransactions(Transactions.ToList(), fullPath, format);

            // ShowMessageBox("Export Successful", $"File saved successfully to: {documentsPath}");
        }

        // Show the message box for feedback
        public void ShowMessageBox(string title, string content)
        {
            ShowMessageBoxRequested?.Invoke(title, content);
        }

        // Validation for numeric values (MinTotalValue & MaxTotalValue)
        private bool ValidateNumericValue(string value)
        {
            return int.TryParse(value, out _); // Check if the value is a valid integer
        }

        // Validate MinTotalValue < MaxTotalValue
        private bool ValidateTotalValues(string minTotalValue, string maxTotalValue)
        {
            if (int.TryParse(minTotalValue, out int min) && int.TryParse(maxTotalValue, out int max))
            {
                return min < max; // Check if min is less than max
            }
            return true; // Return true if validation is not applicable (e.g., empty fields)
        }

        // Validate StartDate < EndDate
        private bool ValidateDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                return startDate.Value < endDate.Value; // Ensure start date is before end date
            }
            return true; // Return true if validation is not applicable (e.g., empty fields)
        }

        public void LoadTransactions()
        {
            // Add null checks here for all ComboBoxItem properties to prevent null reference
            string transactionType = SelectedTransactionType?.Content?.ToString() ?? "ALL";
            string sortBy = SelectedSortBy?.Content?.ToString() ?? "Date";
            string sortOrder = SelectedSortOrder?.Content?.ToString() ?? "ASC";

            // Validate MinTotalValue < MaxTotalValue
            if (!ValidateTotalValues(MinTotalValue, MaxTotalValue))
            {
                ShowMessageBox("Invalid Total Values", "Min Total Value must be less than Max Total Value.");
                return;
            }

            // Validate StartDate < EndDate
            if (!ValidateDateRange(StartDate, EndDate))
            {
                ShowMessageBox("Invalid Date Range", "Start Date must be earlier than End Date.");
                return;
            }

            DateTime startDate = StartDate ?? DateTime.Now.AddYears(-10);
            DateTime endDate = EndDate ?? DateTime.Now;

            Transactions.Clear();

            var transactions = service.GetFilteredTransactions(new TransactionFilterCriteria
            {
                StockName = StockNameFilter,
                Type = transactionType == "ALL" ? null : transactionType,
                MinTotalValue = MinTotalValue == null || MinTotalValue == "" ? null : Convert.ToInt32(MinTotalValue),
                MaxTotalValue = MaxTotalValue == null || MaxTotalValue == "" ? null : Convert.ToInt32(MaxTotalValue),
                StartDate = startDate,
                EndDate = endDate
            });

            foreach (var transaction in transactions)
            {
                Transactions.Add(transaction);
            }

            SortTransactions(sortBy, sortOrder == "ASC");
        }

        private void SortTransactions(string sortBy, bool isAscending)
        {
            var sortedTransactions = service.SortTransactions(Transactions.ToList(), sortBy, isAscending);

            Transactions.Clear();
            foreach (var transaction in sortedTransactions)
            {
                Transactions.Add(transaction);
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
