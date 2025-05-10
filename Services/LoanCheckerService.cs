namespace StockApp.Services
{
    using System;
    using Microsoft.UI.Xaml;

    public class LoanCheckerService : ILoanCheckerService
    {
        private readonly ILoanService loanServices;
        private readonly DispatcherTimer timer;
        private bool initialCheckDone = false;

        public event EventHandler LoansUpdated;

        public LoanCheckerService(ILoanService loanServices)
        {
            this.loanServices = loanServices;
            this.timer = new DispatcherTimer
            {
                // Set to a more reasonable interval since this checks monthly payments
                // For production, this could be even longer (e.g., hours)
                Interval = TimeSpan.FromMinutes(5)
            };
            this.timer.Tick += this.Timer_Tick;
        }

        private async void Timer_Tick(object sender, object e)
        {
            await this.loanServices.CheckLoansAsync();
            this.LoansUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Start()
        {
            // Perform an initial check when starting
            if (!initialCheckDone)
            {
                // Use fire-and-forget pattern for the initial check
                PerformInitialCheckAsync();
            }
            
            this.timer.Start();
        }

        private async void PerformInitialCheckAsync()
        {
            await this.loanServices.CheckLoansAsync();
            initialCheckDone = true;
            this.LoansUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            this.timer.Stop();
        }
    }
}
