using Common.Models;
using System.ComponentModel.DataAnnotations;

namespace StockAppWeb.Models
{
    public class NewsListViewModel
    {
        public List<NewsArticle> Articles { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        [Required(ErrorMessage = "Category is required")]
        public string SelectedCategory { get; set; } = "All";
        [StringLength(100, ErrorMessage = "Search query cannot exceed 100 characters")]
        public string SearchQuery { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsLoggedIn { get; set; }
        public User? CurrentUser { get; set; }
        public bool IsEmptyState => !Articles.Any();
        public bool IsLoading { get; set; }
        public bool IsRefreshing { get; set; }
    }
} 