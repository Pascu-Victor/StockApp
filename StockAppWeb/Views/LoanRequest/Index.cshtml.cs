using Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StockAppWeb.Views.LoanRequest
{
    using Common.Models;

    public partial class IndexModel : PageModel
    {
        private readonly ILoanRequestService _loanRequestService;

        public IndexModel(ILoanRequestService loanRequestService)
        {
            _loanRequestService = loanRequestService;
        }

        public List<LoanRequest> Requests { get; private set; } = new();
        public Dictionary<int, string> Suggestions { get; private set; } = new();
        public string? ErrorMessage { get; private set; }

        public async Task OnGetAsync()
        {
            try
            {
                Requests = await _loanRequestService.GetUnsolvedLoanRequests();

                foreach (var request in Requests)
                {
                    var suggestion = await _loanRequestService.GiveSuggestion(request);
                    Suggestions[request.Id] = suggestion;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading loan requests: {ex.Message}";
            }
        }

        public async Task<IActionResult> OnPostSolveAsync(int id)
        {
            try
            {
                await _loanRequestService.SolveLoanRequest(id);
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to solve loan request: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _loanRequestService.DeleteLoanRequest(id);
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to delete loan request: {ex.Message}";
                return Page();
            }
        }
    }
}