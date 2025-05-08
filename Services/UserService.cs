namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;
    using StockApp.Repositories;

    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public User GetUserByCnp(string cnp)
        {
            if (string.IsNullOrWhiteSpace(cnp))
            {
                throw new ArgumentException("CNP cannot be empty");
            }

            return this.userRepository.GetUserByCnpAsync(cnp).Result;
        }

        public Task<List<User>> GetUsers()
        {
            return this.userRepository.GetAllUsersAsync();
        }

        public async Task CreateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            await this.userRepository.CreateUserAsync(user);
        }
    }
}