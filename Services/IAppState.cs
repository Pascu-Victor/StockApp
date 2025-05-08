namespace StockApp.Services
{
    using StockApp.Models;

    /// <summary>
    /// Interface for application state management
    /// </summary>
    public interface IAppState
    {
        /// <summary>
        /// Gets or sets the current logged-in user
        /// </summary>
        User CurrentUser { get; set; }
    }
} 