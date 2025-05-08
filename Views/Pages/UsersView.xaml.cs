namespace StockApp.Views.Pages
{
    using System;
    using System.Collections.Generic;
    using Microsoft.UI.Xaml.Controls;
    using StockApp.Models;
    using StockApp.Services;
    using StockApp.Views.Components;

    public sealed partial class UsersView : Page
    {
        private readonly IUserService userService;
        private readonly Func<UserInfoComponent> userComponentFactory;

        public UsersView(IUserService userService, Func<UserInfoComponent> userComponentFactory)
        {
            this.InitializeComponent();
            this.userService = userService;
            this.userComponentFactory = userComponentFactory;
            this.LoadUsers();
        }

        private async void LoadUsers()
        {
            this.UsersContainer.Items.Clear();

            try
            {
                List<User> users = await this.userService.GetUsers();
                foreach (var user in users)
                {
                    var userComponent = this.userComponentFactory();
                    userComponent.SetUserData(user);
                    this.UsersContainer.Items.Add(userComponent);
                }
            }
            catch (Exception)
            {
                this.UsersContainer.Items.Add("There are no users to display.");
            }
        }
    }
}
