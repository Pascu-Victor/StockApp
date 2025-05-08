namespace StockApp.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using StockApp.Models;
    using StockApp.Services;

    public class UserViewModel : INotifyPropertyChanged
    {
        private IUserService userService;

        public ObservableCollection<User> Users { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public UserViewModel(IUserService userServices)
        {
            this.userService = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task LoadUsers()
        {
            try
            {
                var users = await this.userService.GetUsers();
                foreach (var user in users)
                {
                    this.Users.Add(user);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error: {exception.Message}");
            }
        }
    }
}