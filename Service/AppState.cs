﻿using System;

namespace StockApp.Service
{
    public class AppState
    {
        private static readonly Lazy<AppState> _instance = new Lazy<AppState>(() => new AppState());

        public static AppState Instance => _instance.Value;

        public Model.User CurrentUser 
        { 
            get => _currentUser;
            set => _currentUser = value ?? throw new ArgumentNullException(nameof(value));
        }

        private Model.User _currentUser;

        private AppState()
        {
            _currentUser = new Model.User("1234567890123", "Caramel", "asdf", false, "imagine", false);
        }
    }
}