using Common.Models;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockAppWeb.Models;
using StockAppWeb.Services;
using System.Security.Claims;

namespace StockAppWeb.Controllers
{
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;

        public NewsController(INewsService newsService, IUserService userService, IAuthenticationService authService)
        {
            _newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new NewsListViewModel
            {
                Articles = await _newsService.GetFilteredArticlesAsync(),
                Categories = new List<string> { "All", "Stock News", "Company News", "Market Analysis", "Economic News", "Functionality News" },
                SelectedCategory = "All",
                IsAdmin = _authService.IsUserAdmin(),
                IsLoggedIn = _authService.IsUserLoggedIn(),
                CurrentUser = await _userService.GetCurrentUserAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(string articleId)
        {
            if (string.IsNullOrEmpty(articleId))
            {
                return BadRequest("Article ID is required");
            }

            var success = await _newsService.MarkArticleAsReadAsync(articleId);
            if (!success)
            {
                return BadRequest("Failed to mark article as read");
            }

            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Article(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var article = await _newsService.GetNewsArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Filter(string category, string searchQuery)
        {
            var filteredArticles = await _newsService.GetFilteredArticlesAsync(category, searchQuery);
            return PartialView("_ArticleList", filteredArticles);
        }
    }
} 