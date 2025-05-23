using Common.Models;
using Common.Services;
using StockApp.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockApp.ViewModels
{
    public class CreateLoanDialogViewModel : INotifyPropertyChanged
    {
        private readonly ILoanService _loanService;
        private decimal _amount;
        private DateTimeOffset _repaymentDate = DateTime.Now.AddMonths(1);
        private string _errorMessage = string.Empty;
        private string _successMessage = string.Empty;
        private bool _isSubmitting = false;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<LoanRequest>? LoanRequestSubmitted;
        public event EventHandler? DialogClosed;

        public ICommand SubmitCommand { get; private set; }

        public double Amount
        {
            get => (double)_amount;
            set
            {
                _amount = (decimal)value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public DateTimeOffset RepaymentDate
        {
            get => _repaymentDate;
            set
            {
                _repaymentDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public DateTimeOffset MinDate => DateTime.Now.AddMonths(1);

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsSubmitting
        {
            get => _isSubmitting;
            private set
            {
                _isSubmitting = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => ValidateInputs() && !IsSubmitting;

        public CreateLoanDialogViewModel(ILoanService loanService)
        {
            _loanService = loanService;
            SubmitCommand = new RelayCommand(async (o) => await SubmitAsync(), (o) => IsValid);
        }

        public async Task SubmitRequestAsync()
        {
            await SubmitAsync();
        }

        private async Task SubmitAsync()
        {
            if (!ValidateInputs() || IsSubmitting)
            {
                return;
            }

            IsSubmitting = true;
            ErrorMessage = string.Empty;

            var request = new LoanRequest
            {
                Amount = _amount,
                RepaymentDate = RepaymentDate.DateTime,
                ApplicationDate = DateTime.UtcNow,
                Status = "Pending"
            };

            try
            {
                await _loanService.AddLoanAsync(request);
                SuccessMessage = "Loan request submitted successfully!";
                LoanRequestSubmitted?.Invoke(this, request);

                // Close the dialog after a short delay to show the success message
                await Task.Delay(1000);
                DialogClosed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred while submitting your loan request: {ex.Message}";
                IsSubmitting = false;
            }
        }

        private void ResetForm()
        {
            Amount = 0;
            RepaymentDate = DateTime.Now.AddMonths(1);
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            IsSubmitting = false;
        }

        private bool ValidateInputs()
        {
            if (Amount < 100 || Amount > 100000)
            {
                ErrorMessage = "Amount must be between 100 and 100,000";
                return false;
            }

            if (RepaymentDate < MinDate)
            {
                ErrorMessage = "Repayment date must be at least 1 month in the future";
                return false;
            }
            ErrorMessage = string.Empty;
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}