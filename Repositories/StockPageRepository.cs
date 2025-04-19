﻿namespace StockApp.Repositories
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;
    using StockApp.Database;
    using StockApp.Models;

    /// <summary>
    /// Repository for managing stock page data, including user and stock information.
    /// </summary>
    public class StockPageRepository : IStockPageRepository
    {
        private readonly string userCnp;
        private readonly SqlConnection connection;

        /// <summary>
        /// Gets the user associated with the stock page.
        /// </summary>
        public User? User { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current user is a guest.
        /// </summary>
        public bool IsGuest { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StockPageRepository"/> class.
        /// </summary>
        public StockPageRepository()
        {
            this.connection = DatabaseHelper.GetConnection();
            InitializeUser();
        }

        public void UpdateUserGems(int newGemBalance)
        {
            using var command = new SqlCommand("UPDATE [USER] SET GEM_BALANCE = @gems WHERE CNP = @cnp", this.connection);
            command.Parameters.AddWithValue("@gems", newGemBalance);
            command.Parameters.AddWithValue("@cnp", this.userCnp);
            command.ExecuteNonQuery();

            if (this.User != null)
            {
                this.User.GemBalance = newGemBalance;
            }
        }

        public void AddOrUpdateUserStock(string stockName, int quantity)
        {
            const string query = @"
                        IF EXISTS (SELECT 1 FROM USER_STOCK WHERE USER_CNP = @cnp AND STOCK_NAME = @name)
                        BEGIN
                            UPDATE USER_STOCK SET QUANTITY = QUANTITY + @quantity WHERE USER_CNP = @cnp AND STOCK_NAME = @name
                        END
                        ELSE
                        BEGIN
                            INSERT INTO USER_STOCK (USER_CNP, STOCK_NAME, QUANTITY) VALUES (@cnp, @name, @quantity)
                        END";

            using var command = new SqlCommand(query, this.connection);
            command.Parameters.AddWithValue("@cnp", this.userCnp);
            command.Parameters.AddWithValue("@name", stockName);
            command.Parameters.AddWithValue("@quantity", quantity);
            command.ExecuteNonQuery();
        }

        public void AddStockValue(string stockName, int price)
        {
            using var command = new SqlCommand("INSERT INTO STOCK_VALUE (STOCK_NAME, PRICE) VALUES (@name, @price)", this.connection);
            command.Parameters.AddWithValue("@name", stockName);
            command.Parameters.AddWithValue("@price", price);
            command.ExecuteNonQuery();
        }

        public Stock GetStock(string stockName)
        {
            const string query = @"
                SELECT s.*, sv.*, us.QUANTITY 
                FROM STOCK s
                INNER JOIN STOCK_VALUE sv ON s.STOCK_NAME = sv.STOCK_NAME
                LEFT JOIN USER_STOCK us ON s.STOCK_NAME = us.STOCK_NAME AND us.USER_CNP = @cnp
                WHERE s.STOCK_NAME = @name";

            using var command = new SqlCommand(query, this.connection);
            command.Parameters.AddWithValue("@name", stockName);
            command.Parameters.AddWithValue("@cnp", this.userCnp);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Stock(
                    name: reader["STOCK_NAME"].ToString() ?? throw new Exception("Stock name not found."),
                    symbol: reader["STOCK_SYMBOL"].ToString() ?? throw new Exception("Stock symbol not found."),
                    authorCNP: reader["AUTHOR_CNP"].ToString() ?? throw new Exception("Author CNP not found."),
                    price: Convert.ToInt32(reader["PRICE"]),
                    quantity: reader["QUANTITY"] != DBNull.Value ? Convert.ToInt32(reader["QUANTITY"]) : 0);
            }

            throw new InvalidOperationException($"Stock with name '{stockName}' not found.");
        }

        public List<int> GetStockHistory(string stockName)
        {
            using var command = new SqlCommand("SELECT PRICE FROM STOCK_VALUE WHERE STOCK_NAME = @name", this.connection);
            command.Parameters.AddWithValue("@name", stockName);

            using var reader = command.ExecuteReader();
            var stockValues = new List<int>();
            while (reader.Read())
            {
                stockValues.Add(Convert.ToInt32(reader["PRICE"]));
            }
            return stockValues;
        }

        public int GetOwnedStocks(string stockName)
        {
            using var command = new SqlCommand("SELECT QUANTITY FROM USER_STOCK WHERE USER_CNP = @cnp AND STOCK_NAME = @name", this.connection);
            command.Parameters.AddWithValue("@cnp", this.userCnp);
            command.Parameters.AddWithValue("@name", stockName);

            using var reader = command.ExecuteReader();
            return reader.Read() ? Convert.ToInt32(reader["QUANTITY"]) : 0;
        }

        public bool GetFavorite(string stockName)
        {
            using var command = new SqlCommand("SELECT 1 FROM FAVORITE_STOCK WHERE USER_CNP = @cnp AND STOCK_NAME = @name", this.connection);
            command.Parameters.AddWithValue("@cnp", this.userCnp);
            command.Parameters.AddWithValue("@name", stockName);

            using var reader = command.ExecuteReader();
            return reader.Read();
        }

        public void ToggleFavorite(string stockName, bool state)
        {
            if (state)
            {
                using var command = new SqlCommand("INSERT INTO FAVORITE_STOCK (USER_CNP, STOCK_NAME) VALUES (@cnp, @name)", this.connection);
                command.Parameters.AddWithValue("@cnp", this.userCnp);
                command.Parameters.AddWithValue("@name", stockName);
                command.ExecuteNonQuery();
            }
            else
            {
                using var command = new SqlCommand("DELETE FROM FAVORITE_STOCK WHERE USER_CNP = @cnp AND STOCK_NAME = @name", this.connection);
                command.Parameters.AddWithValue("@cnp", this.userCnp);
                command.Parameters.AddWithValue("@name", stockName);
                command.ExecuteNonQuery();
            }
        }

        private void InitializeUser()
        {
            using var command = new SqlCommand("SELECT * FROM [USER] WHERE CNP = @cnp", this.connection);
            command.Parameters.AddWithValue("@cnp", this.userCnp);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                this.User = new User(
                    reader["CNP"].ToString(),
                    reader["NAME"].ToString(),
                    reader["DESCRIPTION"].ToString(),
                    Convert.ToBoolean(reader["IS_ADMIN"]),
                    reader["PROFILE_PICTURE"].ToString(),
                    Convert.ToBoolean(reader["IS_HIDDEN"]),
                    Convert.ToInt32(reader["GEM_BALANCE"]));
                this.IsGuest = false;
            }
            else
            {
                this.User = null; // Explicitly set to null to satisfy CS8618
                this.IsGuest = true;
            }
        }
    }
}
