using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace StockAppWeb.Views.TransactionLog
{
    public class IndexModel : PageModel
    {
        private readonly ITransactionService _transactionService;
        private readonly ITransactionLogService _transactionLogService;

        public IndexModel(ITransactionService transactionService, ITransactionLogService transactionLogService)
        {
            _transactionService = transactionService;
            _transactionLogService = transactionLogService;
        }

        public List<TransactionLogTransaction> Transactions { get; private set; } = new();
        public string? ErrorMessage { get; private set; }
        public string? SuccessMessage { get; private set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public string? StockNameFilter { get; set; }
            public string SelectedTransactionType { get; set; } = "ALL";
            public string SelectedSortBy { get; set; } = "Date";
            public string SelectedSortOrder { get; set; } = "ASC";
            public string SelectedExportFormat { get; set; } = "CSV";
            public string? MinTotalValue { get; set; }
            public string? MaxTotalValue { get; set; }
            public DateTime StartDate { get; set; } = DateTime.UnixEpoch;
            public DateTime EndDate { get; set; } = DateTime.Now;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var criteria = new TransactionFilterCriteria
                {
                    StockName = Input.StockNameFilter,
                    Type = Input.SelectedTransactionType == "ALL" ? null : Input.SelectedTransactionType,
                    MinTotalValue = string.IsNullOrEmpty(Input.MinTotalValue) ? null : int.Parse(Input.MinTotalValue),
                    MaxTotalValue = string.IsNullOrEmpty(Input.MaxTotalValue) ? null : int.Parse(Input.MaxTotalValue),
                    StartDate = Input.StartDate,
                    EndDate = Input.EndDate
                };

                Transactions = await _transactionService.GetByFilterCriteriaAsync(criteria);
                Transactions = _transactionLogService.SortTransactions(
                    Transactions,
                    Input.SelectedSortBy,
                    Input.SelectedSortOrder == "ASC");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading transactions: {ex.Message}";
            }
        }

        public async Task OnPostAsync(InputModel input)
        {
            Input = input;
            await OnGetAsync();
        }
    }
} 