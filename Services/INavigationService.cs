using System;
using Microsoft.UI.Xaml.Controls;

namespace StockApp.Services
{
    public interface INavigationService
    {
        static abstract NavigationService Instance { get; }
        bool CanGoBack { get; }

        static abstract void Initialize(Frame frame);
        void GoBack();
        bool Navigate(Type pageType, object? parameter = null);
        void NavigateToArticleDetail(string articleId);
    }
}