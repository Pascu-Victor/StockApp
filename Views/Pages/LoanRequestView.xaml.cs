namespace StockApp.Views.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Models;
    using StockApp.Services;
    using StockApp.Views.Components;

    public sealed partial class LoanRequestView : Page
    {
        private readonly ILoanRequestService service;
        private readonly Func<LoanRequestComponent> componentFactory;

        public LoanRequestView(ILoanRequestService loanRequestService, Func<LoanRequestComponent> componentFactory)
        {
            this.InitializeComponent();
            this.service = loanRequestService;
            this.componentFactory = componentFactory;
            this.LoadLoanRequests().ConfigureAwait(false);
        }

        private async Task LoadLoanRequests()
        {
            this.LoanRequestContainer.Items.Clear();

            try
            {
                List<LoanRequest> loanRequests = await this.service.GetUnsolvedLoanRequests();

                if (loanRequests.Count == 0)
                {
                    this.LoanRequestContainer.Items.Add("There are no loan requests that need solving.");
                    return;
                }

                foreach (var request in loanRequests)
                {
                    LoanRequestComponent requestComponent = this.componentFactory();
                    requestComponent.SetRequestData(
                        request.Id,
                        request.UserCnp,
                        request.Amount,
                        request.ApplicationDate,
                        request.RepaymentDate,
                        request.Status,
                        await this.service.GiveSuggestion(request));

                    requestComponent.LoanRequestSolved += this.OnLoanRequestSolved;

                    this.LoanRequestContainer.Items.Add(requestComponent);
                }
            }
            catch (Exception ex)
            {
                this.LoanRequestContainer.Items.Add($"Error loading loan requests: {ex.Message}");
            }
        }

        private void OnLoanRequestSolved(object sender, EventArgs e)
        {
            this.LoadLoanRequests();
        }
    }
}
