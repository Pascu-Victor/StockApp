﻿namespace StockApp.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Models;
    using StockApp.Repositories;
    using StockApp.Services;

    public class TransactionLogViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly ITransactionLogService Service;

        private string StockNameFilter;
        private string SelectedTransactionType;
        private string SelectedSortBy;
        private string SelectedSortOrder;
        private string SelectedExportFormat;
        private string MinTotalValue;
        private string MaxTotalValue;
        private DateTime? StartDate;
        private DateTime? EndDate;

        public event Action<string, string> ShowMessageBoxRequested;

        public ObservableCollection<TransactionLogTransaction> Transactions { get; set; } = new ObservableCollection<TransactionLogTransaction>();

        public string StockNameFilter
        {
            get => StockNameFilter;
            set { StockNameFilter = value; OnPropertyChanged(nameof(StockNameFilter)); }
        }

        public string SelectedTransactionType
        {
            get => SelectedTransactionType;
            set
            {
                SelectedTransactionType = value;
                OnPropertyChanged(nameof(SelectedTransactionType));
                LoadTransactions(); // Reload transactions when the selected type changes
            }
        }

        public string SelectedSortBy
        {
            get => SelectedSortBy;
            set
            {
                SelectedSortBy = value;
                OnPropertyChanged(nameof(SelectedSortBy));
                LoadTransactions(); // Reload transactions when the sorting criteria change
            }
        }

        public string SelectedSortOrder
        {
            get => SelectedSortOrder;
            set
            {
                SelectedSortOrder = value;
                OnPropertyChanged(nameof(SelectedSortOrder));
                LoadTransactions(); // Reload transactions when the sort order changes
            }
        }

        public string SelectedExportFormat
        {
            get => SelectedExportFormat;
            set
            {
                SelectedExportFormat = value;
                OnPropertyChanged(nameof(SelectedExportFormat));
            }
        }

        public string MinTotalValue
        {
            get => MinTotalValue;
            set
            {
                if (ValidateNumericValue(value))
                {
                    MinTotalValue = value;
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
            get => MaxTotalValue;
            set
            {
                if (ValidateNumericValue(value))
                {
                    MaxTotalValue = value;
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
            get => StartDate;
            set { StartDate = value; OnPropertyChanged(nameof(StartDate)); LoadTransactions(); }
        }

        public DateTime? EndDate
        {
            get => EndDate;
            set { EndDate = value; OnPropertyChanged(nameof(EndDate)); LoadTransactions(); }
        }

        public ICommand SearchCommand { get; }
        public ICommand ExportCommand { get; }

        public TransactionLogViewModel(ITransactionLogService service)
        {
            this.Service = service ?? throw new ArgumentNullException(nameof(service));

            // Initialize ComboBoxItems for options if they are null
            SelectedTransactionType = "ALL";
            SelectedSortBy = "Date";
            SelectedSortOrder = "ASC";
            SelectedExportFormat = "CSV";

            // Set up commands
            SearchCommand = new Commands.Command(Search);
            ExportCommand = new Commands.Command(async () => await Export());

            LoadTransactions();
        }

        public TransactionLogViewModel()
          : this(new TransactionLogService(new TransactionRepository()))
        { }

        private void Search()
        {
            LoadTransactions();
        }

        private async Task Export()
        {
            string format = SelectedExportFormat.ToString();
            string fileName = "transactions";

            // Save the file to the user's Documents folder
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string fullPath = Path.Combine(documentsPath, $"{fileName}.{format.ToLower()}");

            // Export the transactions
            Service.ExportTransactions(Transactions.ToList(), fullPath, format);

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
            if (Service == null)
                throw new InvalidOperationException("Transaction service is not initialized");

            // Add null checks here for all ComboBoxItem properties to prevent null reference
            string transactionType = SelectedTransactionType?.ToString() ?? "ALL";
            string sortBy = SelectedSortBy?.ToString() ?? "Date";
            string sortOrder = SelectedSortOrder?.ToString() ?? "ASC";

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

            var filterCriteria = new TransactionFilterCriteria
            {
                StockName = StockNameFilter,
                Type = transactionType == "ALL" ? null : transactionType,
                MinTotalValue = string.IsNullOrEmpty(MinTotalValue) ? null : Convert.ToInt32(MinTotalValue),
                MaxTotalValue = string.IsNullOrEmpty(MaxTotalValue) ? null : Convert.ToInt32(MaxTotalValue),
                StartDate = startDate,
                EndDate = endDate
            };

            filterCriteria.Validate();

            var transactions = Service.GetFilteredTransactions(filterCriteria)
                ?? throw new InvalidOperationException("Transaction service returned null");

            foreach (var transaction in transactions)
            {
                Transactions.Add(transaction);
            }

            SortTransactions(sortBy, sortOrder == "ASC");
        }

        private void SortTransactions(string sortBy, bool isAscending)
        {
            var sortedTransactions = Service.SortTransactions(Transactions.ToList(), sortBy, isAscending);

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
