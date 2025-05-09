namespace StockApp.Views.Components
{
    using System;
    using System.ComponentModel;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Models;
    using StockApp.Services;

    /// <summary>
    /// Represents a component for managing loan-related operations.
    /// </summary>
    public sealed partial class LoanComponent : Page
    {
        public static readonly DependencyProperty LoanProperty =
            DependencyProperty.Register(nameof(Loan), typeof(Loan), typeof(LoanComponent), new PropertyMetadata(null, OnLoanPropertyChanged));

        private Loan loan;

        public Loan Loan
        {
            get => this.loan;
            set
            {
                if (this.loan != value)
                {
                    this.loan = value;
                    this.OnPropertyChanged(nameof(this.Loan));
                }
            }
        }

        private static void OnLoanPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoanComponent component)
            {
                component.Loan = e.NewValue as Loan;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly ILoanService loanServices;

        /// <summary>
        /// Occurs when the loan is updated.
        /// </summary>
        public event EventHandler? LoanUpdated;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoanComponent"/> class.
        /// </summary>
        public LoanComponent()
        {
            this.loanServices = App.Host.Services.GetService<ILoanService>() ?? throw new InvalidOperationException("LoanService not registered");
            this.InitializeComponent();
        }

        public async void OnSolveClick(object sender, RoutedEventArgs e)
        {
            if (this.Loan != null)
            {
                await this.loanServices.IncrementMonthlyPaymentsCompletedAsync(this.Loan.Id, this.Loan.Penalty);
                this.LoanUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}