using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Mvc;
using StockAppWeb.Views.Loans;

namespace StockAppWeb.Controllers
{
    public class LoansController : Controller
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexModel(_loanService);
            await model.OnGetAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(MakePaymentDTO payment)
        {
            var model = new IndexModel(_loanService);
            await _loanService.IncrementMonthlyPaymentsCompletedAsync(payment.loanId, payment.penalty);
            return RedirectToAction("Index");
        }
        public IActionResult Create()
        {
            var model = new CreateModel(_loanService);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateModel.InputModel input)
        {
            var model = new CreateModel(_loanService);
            if (ModelState.IsValid)
            {
                var request = new LoanRequest
                {
                    Amount = input.Amount,
                    RepaymentDate = input.RepaymentDate,
                    Status = "Pending"
                };

                await _loanService.AddLoanAsync(request);
                model.SuccessMessage = "Loan request submitted successfully.";
                ModelState.Clear();
                return RedirectToAction("Index");
            }
            return View(model);
        }

    }
    public class MakePaymentDTO
    {
        public int loanId { get; set; }
        public decimal penalty { get; set; }
    }
}