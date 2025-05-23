using Common.Services;
using Microsoft.AspNetCore.Mvc;
using StockAppWeb.Views.LoanRequest;

namespace StockAppWeb.Controllers
{
    public class LoanRequestController : Controller
    {
        private readonly ILoanRequestService _loanRequestService;

        public LoanRequestController(ILoanRequestService loanRequestService)
        {
            _loanRequestService = loanRequestService ?? throw new ArgumentNullException(nameof(loanRequestService));
        }

        public IActionResult Index()
        {
            var model = new IndexModel(_loanRequestService);
            return View(model);
        }
    }
}
