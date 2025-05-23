using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace StockAppWeb.Views.Loans
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ILoanService _loanService;

        public CreateModel(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Range(100, 100000)]
            public decimal Amount { get; set; }
            [FutureDate(ErrorMessage = "Repayment date must be in the future.", MinimumMonthsAdvance = 1)]
            public DateTime RepaymentDate { get; set; } = DateTime.Now;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors below.";
                return Page();
            }

            var userCnp = User.FindFirstValue("CNP");

            if (string.IsNullOrEmpty(userCnp))
            {
                ErrorMessage = "Unable to identify user. Please log in again.";
                return Page();
            }

            var loanRequest = new Common.Models.LoanRequest
            {
                UserCnp = userCnp,
                Amount = Input.Amount,
                ApplicationDate = DateTime.UtcNow,
                RepaymentDate = Input.RepaymentDate,
                Status = "Pending"
            };

            try
            {
                await _loanService.AddLoanAsync(loanRequest);
                SuccessMessage = "Loan request submitted successfully!";
                Input = new InputModel();
                ModelState.Clear();
                return Page();
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"An error occurred while submitting your loan request: {ex.Message}";
                return Page();
            }
        }
    }
}
