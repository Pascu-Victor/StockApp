using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace StockApp.Service
{
    public class NavigationService
    {
        private static readonly Lazy<NavigationService> _instance = new Lazy<NavigationService>(() => new NavigationService());

        public static NavigationService Instance => _instance.Value;

        private Frame _frame;

        // Private constructor to enforce singleton pattern
        private NavigationService()
        {
        }

        public void Initialize(Frame frame)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public bool Navigate(Type pageType, object parameter = null)
        {
            if (_frame == null)
                throw new InvalidOperationException("NavigationService not initialized. Call Initialize first.");

            return _frame.Navigate(pageType ?? throw new ArgumentNullException(nameof(pageType)), parameter);
        }

        public void GoBack()
        {
            if (_frame == null)
                throw new InvalidOperationException("NavigationService not initialized. Call Initialize first.");

            if (_frame.CanGoBack)
                _frame.GoBack();
        }

        public bool CanGoBack => _frame?.CanGoBack ?? false;
    }
}