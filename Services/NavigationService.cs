namespace StockApp.Services
{
    using System;
    using Microsoft.UI.Xaml.Controls;

    public class NavigationService : INavigationService
    {
        private static readonly Lazy<NavigationService> instance = new(() => new NavigationService());

        public static NavigationService Instance => instance.Value;

        private static Frame RootFrame;

        // Private constructor to enforce singleton pattern
        private NavigationService()
        {
        }

        public static void Initialize(Frame frame)
        {
            RootFrame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public bool Navigate(Type pageType, object? parameter = null)
        {
            if (RootFrame == null)
            {
                throw new InvalidOperationException("NavigationService not initialized. Call Initialize first.");
            }

            return RootFrame.Navigate(pageType, parameter);
        }

        public void GoBack()
        {
            if (RootFrame == null)
            {
                throw new InvalidOperationException("NavigationService not initialized. Call Initialize first.");
            }

            if (RootFrame.CanGoBack)
            {
                RootFrame.GoBack();
            }
        }

        public bool CanGoBack => RootFrame?.CanGoBack ?? false;

        public void NavigateToArticleDetail(string articleId)
        {
            if (RootFrame == null)
            {
                throw new InvalidOperationException("NavigationService not initialized. Call Initialize first.");
            }

            Navigate(typeof(StockApp.Views.NewsArticleView), articleId);
        }
    }
}