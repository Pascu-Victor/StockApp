namespace StockApp.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Data.SqlClient;
    using StockApp.Database;
    using StockApp.Models;

    public class AlertRepository
    {
        private readonly SqlConnection dbConnection = DatabaseHelper.GetConnection();

        public AlertRepository()
        {
            this.LoadAlerts();
        }

        public List<Alert> Alerts { get; } = [];

        public List<TriggeredAlert> TriggeredAlerts { get; private set; } = [];

        public void ClearTriggeredAlerts() => this.TriggeredAlerts.Clear();

        // Shared Method for SQL Command Execution
        private void ExecuteSql(string query, Action<SqlCommand> parameterize)
        {
            using var command = new SqlCommand(query, this.dbConnection);
            parameterize?.Invoke(command);
            command.ExecuteNonQuery();
        }

        // Shared Method for SQL Reader
        private List<T> ExecuteReader<T>(string query, Func<SqlDataReader, T> map)
        {
            using var command = new SqlCommand(query, this.dbConnection);
            using var reader = command.ExecuteReader();
            List<T> results = [];

            while (reader.Read())
            {
                results.Add(map(reader));
            }

            return results;
        }

        public void LoadAlerts()
        {
            string query = "SELECT * FROM ALERTS";
            this.Alerts.Clear();
            this.Alerts.AddRange(this.ExecuteReader(query, reader => new Alert
            {
                AlertId = reader.GetInt32(0),
                StockName = reader.GetString(1),
                Name = reader.GetString(2),
                LowerBound = reader.GetInt32(3),
                UpperBound = reader.GetInt32(4),
                ToggleOnOff = reader.GetBoolean(5),
            }));
        }

        public List<Alert> GetAllAlerts() => alerts;

        public Alert GetAlertById(int alertId)
        {
            return this.Alerts.FirstOrDefault(a => a.AlertId == alertId)
                ?? throw new KeyNotFoundException($"Alert with ID {alertId} not found.");
        }

        public Alert AddAlert(string stockName, string name, int upperBound, int lowerBound, bool toggleOnOff)
        {
            string insertQuery = @"
                INSERT INTO ALERTS 
                    (STOCK_NAME, NAME, LOWER_BOUND, UPPER_BOUND, TOGGLE) 
                VALUES 
                    (@StockName, @Name, @LowerBound, @UpperBound, @ToggleOnOff);
                
                SELECT SCOPE_IDENTITY();";

            int alertId = -1;

            this.ExecuteSql(insertQuery, command =>
            {
                command.Parameters.AddWithValue("@StockName", stockName);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@LowerBound", lowerBound);
                command.Parameters.AddWithValue("@UpperBound", upperBound);
                command.Parameters.AddWithValue("@ToggleOnOff", toggleOnOff);
                alertId = Convert.ToInt32(command.ExecuteScalar());
            });

            var alert = new Alert
            {
                AlertId = alertId,
                StockName = stockName,
                Name = name,
                LowerBound = lowerBound,
                UpperBound = upperBound,
                ToggleOnOff = toggleOnOff,
            };

            this.Alerts.Add(alert);
            return alert;
        }

        public void UpdateAlert(int alertId, string stockName, string name, decimal upperBound, decimal lowerBound, bool toggleOnOff)
        {
            string updateQuery = @"
                UPDATE ALERTS
                SET 
                    STOCK_NAME = @StockName, 
                    NAME = @Name, 
                    LOWER_BOUND = @LowerBound, 
                    UPPER_BOUND = @UpperBound, 
                    TOGGLE = @ToggleOnOff
                WHERE ALERT_ID = @AlertId";

            this.ExecuteSql(updateQuery, command =>
            {
                command.Parameters.AddWithValue("@AlertId", alertId);
                command.Parameters.AddWithValue("@StockName", stockName);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@LowerBound", lowerBound);
                command.Parameters.AddWithValue("@UpperBound", upperBound);
                command.Parameters.AddWithValue("@ToggleOnOff", toggleOnOff);
            });

            var existingAlert = this.Alerts.FirstOrDefault(a => a.AlertId == alertId);

            if (existingAlert == null)
            {
                return;
            }

            existingAlert.StockName = stockName;
            existingAlert.Name = name;
            existingAlert.LowerBound = lowerBound;
            existingAlert.UpperBound = upperBound;
            existingAlert.ToggleOnOff = toggleOnOff;
        }

        public void DeleteAlert(int alertId)
        {
            string deleteQuery = "DELETE FROM ALERTS WHERE ALERT_ID = @AlertId";

            this.ExecuteSql(deleteQuery, command =>
            {
                command.Parameters.AddWithValue("@AlertId", alertId);
            });

            this.Alerts.RemoveAll(a => a.AlertId == alertId);
        }

        public bool IsAlertTriggered(string stockName, decimal currentPrice)
        {
            var alert = this.Alerts.FirstOrDefault(a => a.StockName == stockName);
            return alert != null && alert.ToggleOnOff &&
                   (currentPrice >= alert.UpperBound || currentPrice <= alert.LowerBound);
        }

        public void TriggerAlert(string stockName, decimal currentPrice)
        {
            if (!this.IsAlertTriggered(stockName, currentPrice))
            {
                return;
            }

            var alert = this.Alerts.First(a => a.StockName == stockName);
            var message = $"⚠ Alert triggered for {stockName}: Price = {currentPrice}, Bounds: [{alert.LowerBound} - {alert.UpperBound}]";

            this.TriggeredAlerts.Add(new TriggeredAlert
            {
                StockName = stockName,
                Message = message,
            });
        }

    }
}