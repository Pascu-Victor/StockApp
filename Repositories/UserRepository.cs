namespace StockApp.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using StockApp.Database;
    using StockApp.Exceptions;
    using StockApp.Models;

    public class UserRepository(string? cnp = null) : IUserRepository
    {
        /// <summary>
        /// Gets or sets the CNP (unique identifier) of the current user.
        /// </summary>
        public string CurrentUserCNP { get; set; } = !string.IsNullOrWhiteSpace(cnp)
                ? cnp
                : App.Configuration["DefaultUserCNP"]
                    ?? throw new InvalidOperationException("DefaultUserCNP is not set in appsettings.json");

        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        /// <param name="user">The user entity to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="UserRepositoryException">Thrown when there is an error creating the user.</exception>
        public async Task CreateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }

            if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName))
            {
                throw new ArgumentException("First and last names cannot be empty");
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                var command = new SqlCommand(@"
                    INSERT INTO [USER] (
                        CNP, NAME, DESCRIPTION, IS_HIDDEN, IS_ADMIN, PROFILE_PICTURE, GEM_BALANCE,
                        FirstName, LastName, Email, PhoneNumber, HashedPassword, NumberOfOffenses,
                        RiskScore, Roi, CreditScore, Birthday, ZodiacSign, ZodiacAttribute,
                        NumberOfBillSharesPaid, Income, Balance
                    )
                    VALUES (
                        @cnp, @name, @description, @isHidden, @isAdmin, @profilePicture, @gemBalance,
                        @firstName, @lastName, @email, @phoneNumber, @hashedPassword, @numberOfOffenses,
                        @riskScore, @roi, @creditScore, @birthday, @zodiacSign, @zodiacAttribute,
                        @numberOfBillSharesPaid, @income, @balance
                    )", connection);

                command.Parameters.AddWithValue("@cnp", user.CNP);
                command.Parameters.AddWithValue("@name", user.Username);
                command.Parameters.AddWithValue("@description", user.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@isHidden", user.IsHidden);
                command.Parameters.AddWithValue("@isAdmin", user.IsModerator);
                command.Parameters.AddWithValue("@profilePicture", user.Image ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@gemBalance", user.GemBalance);
                command.Parameters.AddWithValue("@firstName", user.FirstName);
                command.Parameters.AddWithValue("@lastName", user.LastName);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@hashedPassword", user.HashedPassword);
                command.Parameters.AddWithValue("@numberOfOffenses", user.NumberOfOffenses);
                command.Parameters.AddWithValue("@riskScore", user.RiskScore);
                command.Parameters.AddWithValue("@roi", user.ROI);
                command.Parameters.AddWithValue("@creditScore", user.CreditScore);
                command.Parameters.AddWithValue("@birthday", user.Birthday.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@zodiacSign", user.ZodiacSign);
                command.Parameters.AddWithValue("@zodiacAttribute", user.ZodiacAttribute);
                command.Parameters.AddWithValue("@numberOfBillSharesPaid", user.NumberOfBillSharesPaid);
                command.Parameters.AddWithValue("@income", user.Income);
                command.Parameters.AddWithValue("@balance", user.Balance);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
            }
            catch (SqlException ex)
            {
                throw new UserRepositoryException("Failed to create user in the database.", ex);
            }
        }

        /// <summary>
        /// Retrieves a user by their CNP.
        /// </summary>
        /// <param name="userCNP">The CNP of the user to retrieve.</param>
        /// <returns>The user matching the specified CNP.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no user with the specified CNP exists.</exception>
        /// <exception cref="UserRepositoryException">Thrown when there is an error retrieving the user.</exception>
        public async Task<User> GetUserByCnpAsync(string userCNP)
        {
            if (string.IsNullOrWhiteSpace(userCNP))
            {
                throw new ArgumentException("CNP cannot be empty", nameof(userCNP));
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                var command = new SqlCommand(@"
                    SELECT * FROM [USER] 
                    WHERE CNP = @cnp", connection);
                command.Parameters.AddWithValue("@cnp", userCNP);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return this.CreateUserFromDataReader(reader);
                }

                throw new KeyNotFoundException($"No user found with CNP: {userCNP}");
            }
            catch (SqlException ex)
            {
                throw new UserRepositoryException("Failed to retrieve user from the database.", ex);
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                var command = new SqlCommand("SELECT * FROM [USER] WHERE NAME = @name", connection);
                command.Parameters.AddWithValue("@name", username);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return this.CreateUserFromDataReader(reader);
                }

                throw new KeyNotFoundException($"No user found with username: {username}");
            }
            catch (SqlException ex)
            {
                throw new UserRepositoryException("Failed to retrieve user from the database.", ex);
            }
        }

        /// <summary>
        /// Updates an existing user's details in the database.
        /// </summary>
        /// <param name="user">The user entity with updated information.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="UserRepositoryException">Thrown when there is an error updating the user.</exception>
        public async Task UpdateUserAsync(User user)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                var command = new SqlCommand(@"
                    UPDATE [USER] SET 
                        NAME = @name, 
                        DESCRIPTION = @description, 
                        IS_HIDDEN = @isHidden, 
                        IS_ADMIN = @isAdmin, 
                        PROFILE_PICTURE = @profilePicture, 
                        GEM_BALANCE = @gemBalance,
                        FirstName = @firstName,
                        LastName = @lastName,
                        Email = @email,
                        PhoneNumber = @phoneNumber,
                        HashedPassword = @hashedPassword,
                        NumberOfOffenses = @numberOfOffenses,
                        RiskScore = @riskScore,
                        Roi = @roi,
                        CreditScore = @creditScore,
                        Birthday = @birthday,
                        ZodiacSign = @zodiacSign,
                        ZodiacAttribute = @zodiacAttribute,
                        NumberOfBillSharesPaid = @numberOfBillSharesPaid,
                        Income = @income,
                        Balance = @balance
                    WHERE CNP = @cnp", connection);

                command.Parameters.AddWithValue("@cnp", user.CNP);
                command.Parameters.AddWithValue("@name", user.Username);
                command.Parameters.AddWithValue("@description", user.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@isHidden", user.IsHidden);
                command.Parameters.AddWithValue("@isAdmin", user.IsModerator);
                command.Parameters.AddWithValue("@profilePicture", user.Image ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@gemBalance", user.GemBalance);
                command.Parameters.AddWithValue("@firstName", user.FirstName);
                command.Parameters.AddWithValue("@lastName", user.LastName);
                command.Parameters.AddWithValue("@email", user.Email);
                command.Parameters.AddWithValue("@phoneNumber", user.PhoneNumber ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@hashedPassword", user.HashedPassword);
                command.Parameters.AddWithValue("@numberOfOffenses", user.NumberOfOffenses);
                command.Parameters.AddWithValue("@riskScore", user.RiskScore);
                command.Parameters.AddWithValue("@roi", user.ROI);
                command.Parameters.AddWithValue("@creditScore", user.CreditScore);
                command.Parameters.AddWithValue("@birthday", user.Birthday.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@zodiacSign", user.ZodiacSign);
                command.Parameters.AddWithValue("@zodiacAttribute", user.ZodiacAttribute);
                command.Parameters.AddWithValue("@numberOfBillSharesPaid", user.NumberOfBillSharesPaid);
                command.Parameters.AddWithValue("@income", user.Income);
                command.Parameters.AddWithValue("@balance", user.Balance);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
            }
            catch (SqlException ex)
            {
                throw new UserRepositoryException("Failed to update user in the database.", ex);
            }
        }

        /// <summary>
        /// Deletes a user from the database by their CNP.
        /// </summary>
        /// <param name="userCNP">The CNP of the user to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="UserRepositoryException">Thrown when there is an error deleting the user.</exception>
        public async Task DeleteUserAsync(string userCNP)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                var command = new SqlCommand("DELETE FROM [USER] WHERE CNP = @cnp", connection);
                command.Parameters.AddWithValue("@cnp", userCNP);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                await connection.CloseAsync();
            }
            catch (SqlException ex)
            {
                throw new UserRepositoryException("Failed to delete user from the database.", ex);
            }
        }

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>A list of all user entities.</returns>
        /// <exception cref="UserRepositoryException">Thrown when there is an error retrieving the users.</exception>
        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                var users = new List<User>();
                using var connection = DatabaseHelper.GetConnection();
                var command = new SqlCommand("SELECT * FROM [USER]", connection);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    users.Add(this.CreateUserFromDataReader(reader));
                }

                await connection.CloseAsync();
                return users;
            }
            catch (SqlException ex)
            {
                throw new UserRepositoryException("Failed to retrieve users from the database.", ex);
            }
        }

        public async Task PenalizeUserAsync(string cnp, int penaltyAmount)
        {
            if (string.IsNullOrWhiteSpace(cnp))
            {
                throw new ArgumentException("CNP cannot be empty", nameof(cnp));
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                var command = new SqlCommand(@"
                    UPDATE [USER] 
                    SET CreditScore = CreditScore - @Amount 
                    WHERE CNP = @CNP", connection);

                command.Parameters.AddWithValue("@CNP", cnp);
                command.Parameters.AddWithValue("@Amount", penaltyAmount);

                await connection.OpenAsync();
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException($"No user found with CNP: {cnp}");
                }

                await connection.CloseAsync();
            }
            catch (SqlException ex)
            {
                throw new UserRepositoryException("Failed to penalize user in the database.", ex);
            }
        }

        public async Task IncrementOffensesCountAsync(string cnp)
        {
            if (string.IsNullOrWhiteSpace(cnp))
            {
                throw new ArgumentException("CNP cannot be empty", nameof(cnp));
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                var command = new SqlCommand(@"
                    UPDATE [USER] 
                    SET NumberOfOffenses = ISNULL(NumberOfOffenses, 0) + 1 
                    WHERE CNP = @CNP", connection);

                command.Parameters.AddWithValue("@CNP", cnp);

                await connection.OpenAsync();
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException($"No user found with CNP: {cnp}");
                }

                await connection.CloseAsync();
            }
            catch (SqlException ex)
            {
                throw new UserRepositoryException("Failed to increment offenses count in the database.", ex);
            }
        }

        public async Task UpdateUserCreditScoreAsync(string cnp, int creditScore)
        {
            if (string.IsNullOrWhiteSpace(cnp))
            {
                throw new ArgumentException("CNP cannot be empty", nameof(cnp));
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                var command = new SqlCommand(@"
                    UPDATE [USER] 
                    SET CreditScore = @CreditScore 
                    WHERE CNP = @CNP", connection);

                command.Parameters.AddWithValue("@CNP", cnp);
                command.Parameters.AddWithValue("@CreditScore", creditScore);

                await connection.OpenAsync();
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException($"No user found with CNP: {cnp}");
                }

                await connection.CloseAsync();
            }
            catch (SqlException ex)
            {
                throw new UserRepositoryException("Failed to update credit score in the database.", ex);
            }
        }

        public async Task UpdateUserROIAsync(string cnp, decimal roi)
        {
            if (string.IsNullOrWhiteSpace(cnp))
            {
                throw new ArgumentException("CNP cannot be empty", nameof(cnp));
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                var command = new SqlCommand(@"
                    UPDATE [USER] 
                    SET Roi = @Roi 
                    WHERE CNP = @CNP", connection);

                command.Parameters.AddWithValue("@CNP", cnp);
                command.Parameters.AddWithValue("@Roi", roi);

                await connection.OpenAsync();
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException($"No user found with CNP: {cnp}");
                }

                await connection.CloseAsync();
            }
            catch (SqlException ex)
            {
                throw new UserRepositoryException("Failed to update ROI in the database.", ex);
            }
        }

        public async Task UpdateUserRiskScoreAsync(string cnp, int riskScore)
        {
            if (string.IsNullOrWhiteSpace(cnp))
            {
                throw new ArgumentException("CNP cannot be empty", nameof(cnp));
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                var command = new SqlCommand(@"
                    UPDATE [USER] 
                    SET RiskScore = @RiskScore 
                    WHERE CNP = @CNP", connection);

                command.Parameters.AddWithValue("@CNP", cnp);
                command.Parameters.AddWithValue("@RiskScore", riskScore);

                await connection.OpenAsync();
                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new KeyNotFoundException($"No user found with CNP: {cnp}");
                }

                await connection.CloseAsync();
            }
            catch (SqlException ex)
            {
                throw new UserRepositoryException("Failed to update risk score in the database.", ex);
            }
        }

        private User CreateUserFromDataReader(SqlDataReader reader)
        {
            return new User
            {
                CNP = reader["CNP"].ToString(),
                Username = reader["NAME"].ToString(),
                Description = reader["DESCRIPTION"] as string,
                IsHidden = (bool)reader["IS_HIDDEN"],
                IsModerator = (bool)reader["IS_ADMIN"],
                Image = reader["PROFILE_PICTURE"] as string,
                GemBalance = (int)reader["GEM_BALANCE"],
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                Email = reader["Email"].ToString(),
                PhoneNumber = reader["PhoneNumber"] is DBNull ? string.Empty : reader["PhoneNumber"].ToString(),
                HashedPassword = reader["HashedPassword"].ToString(),
                NumberOfOffenses = reader["NumberOfOffenses"] is DBNull ? 0 : Convert.ToInt32(reader["NumberOfOffenses"]),
                RiskScore = reader["RiskScore"] is DBNull ? 0 : Convert.ToInt32(reader["RiskScore"]),
                ROI = reader["Roi"] is DBNull ? 0m : Convert.ToDecimal(reader["Roi"]),
                CreditScore = reader["CreditScore"] is DBNull ? 0 : Convert.ToInt32(reader["CreditScore"]),
                Birthday = reader["Birthday"] is DBNull ? default : Convert.ToDateTime(reader["Birthday"]),
                ZodiacSign = reader["ZodiacSign"].ToString(),
                ZodiacAttribute = reader["ZodiacAttribute"].ToString(),
                NumberOfBillSharesPaid = reader["NumberOfBillSharesPaid"] is DBNull ? 0 : Convert.ToInt32(reader["NumberOfBillSharesPaid"]),
                Income = reader["Income"] is DBNull ? 0 : Convert.ToInt32(reader["Income"]),
                Balance = reader["Balance"] is DBNull ? 0m : Convert.ToDecimal(reader["Balance"])
            };
        }
    }
}