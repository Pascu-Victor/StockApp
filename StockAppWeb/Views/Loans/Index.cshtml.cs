using Microsoft.AspNetCore.Mvc.RazorPages;
using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace StockAppWeb.Views.Loans
{
    public class IndexModel : PageModel
    {
        private readonly ILoanService _loanService;

        public IndexModel(ILoanService loanService)
        {
            _loanService = loanService;
        }

        public List<Loan> Loans { get; private set; } = new();
        public string? ErrorMessage { get; private set; }

        public async Task OnGetAsync()
        {
            try
            {
                Loans = await _loanService.GetLoansAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading loans: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostIncrementPaymentAsync(int loanId, decimal penalty)
        {
            try
            {
                await _loanService.IncrementMonthlyPaymentsCompletedAsync(loanId, penalty);
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update loan: {ex.Message}";
                return Page();
            }
        }
    }
}
