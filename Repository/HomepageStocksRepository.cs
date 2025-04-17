using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using StockApp.Database;
using StockApp.Model;

namespace StockApp.Repository
{
    internal class HomepageStocksRepository
    {
        private SqlConnection dbConnection = DatabaseHelper.Instance.GetConnection();
        private string userCnp;

        public string getCnp()
        {
            return this.userCnp;
        }

        public List<int> GetStockHistory(string stockName)
        {
            using (SqlCommand getStock = new SqlCommand("SELECT PRICE FROM STOCK_VALUE WHERE STOCK_NAME = @name ORDER BY STOCK_VALUE_ID", dbConnection))
            {
                getStock.Parameters.AddWithValue("@name", stockName);

                using (SqlDataReader reader = getStock.ExecuteReader())
                {
                    List<int> stock_values = new List<int>();
                    while (reader.Read())
                    {
                        stock_values.Add(Convert.ToInt32(reader["PRICE"]));
                    }
                    return stock_values;
                }
            }
        }
        public HomepageStocksRepository()
        {
            this.userCnp = GetUserCnp();
            Console.WriteLine("User CNP: " + userCnp);
            Console.WriteLine("IsGuestUser: " + IsGuestUser(userCnp));
            LoadStocks();
        }
        public bool IsGuestUser(string userCNP)
        {
            string query = "SELECT COUNT(*) FROM [USER] WHERE CNP = @UserCNP";
            using (var command = new SqlCommand(query, dbConnection))
            {
                command.Parameters.AddWithValue("@UserCNP", userCNP);
                int count = (int)command.ExecuteScalar();
                Console.WriteLine("Count: " + count);
                return count == 0;
            }
        }

        public string GetUserCnp()
        {

            string query = "SELECT TOP 1 CNP FROM HARDCODED_CNPS ORDER BY CNP DESC";

            using (var command = new SqlCommand(query, dbConnection))
            {
                command.ExecuteNonQuery();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader["CNP"].ToString();
                    }
                }
            }
            throw new InvalidOperationException("No CNP found in the database");
        }

        public List<HomepageStock> LoadStocks()
        {
            List<HomepageStock> stocks = new List<HomepageStock>();
            Dictionary<string, List<int>> allStockHistories = new Dictionary<string, List<int>>();

            // First, get all stock histories in one go
            string historyQuery = "SELECT STOCK_NAME, PRICE FROM STOCK_VALUE ORDER BY STOCK_NAME, STOCK_VALUE_ID";
            using (var historyCommand = new SqlCommand(historyQuery, dbConnection))
            {
                using (var historyReader = historyCommand.ExecuteReader())
                {
                    string currentStock = null;
                    List<int> currentPrices = null;

                    while (historyReader.Read())
                    {
                        string stockName = historyReader["STOCK_NAME"].ToString();
                        int price = Convert.ToInt32(historyReader["PRICE"]);

                        if (currentStock != stockName)
                        {
                            if (currentStock != null)
                            {
                                allStockHistories[currentStock] = currentPrices;
                            }
                            currentStock = stockName;
                            currentPrices = new List<int>();
                        }

                        currentPrices.Add(price);
                    }

                    // Add the last stock
                    if (currentStock != null)
                    {
                        allStockHistories[currentStock] = currentPrices;
                    }
                }
            }

            // Now get all stocks info
            string stocksQuery = @"
                                    SELECT 
                                        s.STOCK_NAME, 
                                        s.STOCK_SYMBOL,
                                        MAX(COALESCE(f.IS_FAVORITE, 0)) AS IS_FAVORITE
                                    FROM STOCK s
                                    LEFT JOIN FAVORITE_STOCK f ON s.STOCK_NAME = f.STOCK_NAME AND f.USER_CNP = @UserCNP
                                    GROUP BY s.STOCK_NAME, s.STOCK_SYMBOL";

            using (var command = new SqlCommand(stocksQuery, dbConnection))
            {
                command.Parameters.AddWithValue("@UserCNP", userCnp);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var stockName = reader["STOCK_NAME"]?.ToString();
                        var stockSymbol = reader["STOCK_SYMBOL"]?.ToString();
                        var isFavorite = Convert.ToInt32(reader["IS_FAVORITE"]) == 1;

                        // Get stock history from dict
                        List<int> stockHistory = allStockHistories.ContainsKey(stockName)
                            ? allStockHistories[stockName]
                            : new List<int>();

                        // Calculate price and change percentage
                        int currentPrice = 0;
                        string changePercentage = "0%";

                        if (stockHistory.Count > 0)
                        {
                            currentPrice = stockHistory.Last();

                            if (stockHistory.Count > 1)
                            {
                                int previousPrice = stockHistory[stockHistory.Count - 2];
                                if (previousPrice > 0)
                                {
                                    int increasePerc = ((currentPrice - previousPrice) * 100) / previousPrice;
                                    changePercentage = (increasePerc >= 0 ? "+" : "") + increasePerc.ToString() + "%";
                                }
                            }
                        }

                        var stock = new HomepageStock
                        {
                            Symbol = stockSymbol,
                            Name = stockName,
                            Price = currentPrice,
                            Change = changePercentage,
                            isFavorite = isFavorite
                        };

                        stocks.Add(stock);
                    }
                }
            }

            return stocks;
        }

        public void AddToFavorites(HomepageStock stock)
        {
            var query = "INSERT INTO FAVORITE_STOCK (USER_CNP, STOCK_NAME, IS_FAVORITE) VALUES (@UserCNP, @Name, 1)";
            using (var command = new SqlCommand(query, dbConnection))
            {
                command.Parameters.AddWithValue("@UserCNP", userCnp);
                command.Parameters.AddWithValue("@Name", stock.Name);
                command.ExecuteNonQuery();
            }
        }

        public void RemoveFromFavorites(HomepageStock stock)
        {
            var query = "DELETE FROM FAVORITE_STOCK WHERE USER_CNP = @UserCNP AND STOCK_NAME = @Name";
            using (var command = new SqlCommand(query, dbConnection))
            {
                command.Parameters.AddWithValue("@UserCNP", userCnp);
                command.Parameters.AddWithValue("@Name", stock.Name);
                command.ExecuteNonQuery();
            }
        }

        public void CreateUserProfile()
        {

            string currentCNP = userCnp;
            List<String> names = new List<string>
            {
                "storm", "shadow", "blaze", "nova", "ember", "frost", "zephyr", "luna", "onyx", "raven",
                "viper", "echo", "skye", "falcon", "titan", "phoenix", "cobra", "ghost", "venom", "dusk",
                "wraith", "flare", "night", "rogue", "drift", "glitch", "shade", "pulse", "crimson",
                "hazard", "orbit", "quake", "rune", "saber", "thorn", "vortex", "zodiac", "howl", "jett"
            };
            Random random = new Random();
            string name = names[random.Next(names.Count)];
            int number = random.Next(1000, 10000);
            string randomUsername = $"{name}_{number}";

            string userQuery = "INSERT INTO [USER] (CNP, NAME, DESCRIPTION, IS_HIDDEN, IS_ADMIN, PROFILE_PICTURE, GEM_BALANCE) VALUES(@CNP, @Name, @Description, @IsHidden, @IsAdmin, @ProfilePicture, @GemBalance)";

            using (var command = new SqlCommand(userQuery, dbConnection))
            {
                command.Parameters.AddWithValue("@CNP", currentCNP);
                command.Parameters.AddWithValue("@Name", randomUsername);
                command.Parameters.AddWithValue("@Description", "Default User Description");
                command.Parameters.AddWithValue("@IsHidden", false);
                command.Parameters.AddWithValue("@IsAdmin", false);
                command.Parameters.AddWithValue("@ProfilePicture", "https://cdn.discordapp.com/attachments/1309495559085756436/1358378808440389854/defaultProfilePicture.png?ex=67f3a059&is=67f24ed9&hm=674641524bcc24a5fadfde6b087bf550b147c9ec9d81f81e4b0447f69624cb55&");
                command.Parameters.AddWithValue("@GemBalance", 0);
                command.ExecuteNonQuery();
            }
        }

    }
}
