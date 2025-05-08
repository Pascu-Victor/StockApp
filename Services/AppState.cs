namespace StockApp.Services
{
    using System;
    using StockApp.Models;

    /// <summary>
    /// Singleton implementation of application state
    /// </summary>
    public class AppState : IAppState
    {
        private static readonly Lazy<AppState> _instance = new Lazy<AppState>(() => new AppState());
        
        private User _currentUser;

        /// <summary>
        /// Gets the singleton instance of AppState
        /// </summary>
        public static AppState Instance => _instance.Value;

        /// <summary>
        /// Private constructor to enforce singleton pattern
        /// </summary>
        private AppState()
        {
        }

        /// <summary>
        /// Gets or sets the current logged-in user
        /// </summary>
        public User CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value ?? throw new ArgumentNullException(nameof(value), "CurrentUser cannot be null");
        }
    }
} 