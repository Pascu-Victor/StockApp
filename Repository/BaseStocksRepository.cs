﻿namespace StockApp.Repository
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;
    using StockApp.Database;
    using StockApp.Models;

    internal class BaseStocksRepository
    {
        private readonly List<BaseStock> stocks = [];
        private readonly SqlConnection dbConnection = DatabaseHelper.GetConnection();

        public BaseStocksRepository()
        {
            this.LoadStocks();
        }

        public void AddStock(BaseStock stock, int initialPrice = 100)
        {
            // Start a transaction to ensure both operations complete successfully
            using var transaction = this.dbConnection.BeginTransaction();

            try
            {
                string checkQuery = "SELECT COUNT(*) FROM STOCK WHERE STOCK_NAME = @StockName";
                using (SqlCommand checkCommand = new (checkQuery, this.dbConnection, transaction))
                {
                    checkCommand.Parameters.AddWithValue("@StockName", stock.Name);
                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                    if (count > 0)
                    {
                        throw new Exception("A stock with this name already exists!");
                    }
                }

                // Insert the stock
                string stockQuery = "INSERT INTO STOCK (STOCK_NAME, STOCK_SYMBOL, AUTHOR_CNP) VALUES (@StockName, @StockSymbol, @AuthorCNP)";
                using (SqlCommand stockCommand = new (stockQuery, this.dbConnection, transaction))
                {
                    stockCommand.Parameters.AddWithValue("@StockName", stock.Name);
                    stockCommand.Parameters.AddWithValue("@StockSymbol", stock.Symbol);
                    stockCommand.Parameters.AddWithValue("@AuthorCNP", stock.AuthorCNP);
                    stockCommand.ExecuteNonQuery();
                }

                // Insert the initial stock value
                string valueQuery = "INSERT INTO STOCK_VALUE (STOCK_NAME, PRICE) VALUES (@StockName, @Price)";
                using (SqlCommand valueCommand = new (valueQuery, this.dbConnection, transaction))
                {
                    valueCommand.Parameters.AddWithValue("@StockName", stock.Name);
                    valueCommand.Parameters.AddWithValue("@Price", initialPrice);
                    valueCommand.ExecuteNonQuery();
                }

                // Commit the transaction
                transaction.Commit();
                this.stocks.Add(stock);
            }
            catch (Exception ex)
            {
                // Roll back transaction if something goes wrong
                transaction.Rollback();
                throw new Exception($"Failed to add stock: {ex.Message}");
            }
        }

        public void LoadStocks()
        {
            string query = "SELECT STOCK_NAME, STOCK_SYMBOL, AUTHOR_CNP FROM STOCK";

            using var command = new SqlCommand(query, this.dbConnection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var stockName = reader["STOCK_NAME"]?.ToString() ?? string.Empty;
                var stockSymbol = reader["STOCK_SYMBOL"]?.ToString() ?? string.Empty;
                var authorCnp = reader["AUTHOR_CNP"]?.ToString() ?? string.Empty;
                var stock = new BaseStock(stockName, stockSymbol, authorCnp);

                this.stocks.Add(stock);
            }
        }

        public List<BaseStock> GetAllStocks()
        {
            return this.stocks;
        }
    }
}