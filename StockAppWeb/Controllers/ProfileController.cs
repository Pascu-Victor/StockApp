using Common.Services;
using Microsoft.AspNetCore.Mvc;
using StockAppWeb.Views.Profile;

namespace StockAppWeb.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IStockService _stockService;
        private readonly IAuthenticationService _authenticationService;

        public ProfileController(IUserService userService, IStockService stockService, IAuthenticationService authenticationService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexModel(_userService, _stockService, _authenticationService);
            await model.OnGetAsync();
            return View(model);
        }

        public async Task<IActionResult> Update()
        {
            var model = new UpdateModel(_userService, _authenticationService);
            await model.OnGetAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateModel.InputModel input)
        {
            var model = new UpdateModel(_userService, _authenticationService);
            
            if (ModelState.IsValid)
            {
                await model.OnPostAsync(input);
                
                if (string.IsNullOrEmpty(model.ErrorMessage))
                {
                    return RedirectToAction("Index");
                }
                
                // If there's an error, copy it to ModelState
                ModelState.AddModelError(string.Empty, model.ErrorMessage);
            }
            
            // Repopulate the model
            await model.OnGetAsync();
            return View(model);
        }
    }
} 