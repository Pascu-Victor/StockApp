namespace StockApp.Database
{
    using System;
    using System.Data;
    using System.IO;
    using Microsoft.Data.SqlClient;
    using StockApp.Exceptions;

    internal class DatabaseHelper
    {
        private const string CreateTablesScriptPath = "SqlScripts/CreateTables.sql";
        private const string ResetDatabaseScriptPath = "SqlScripts/ResetDatabase.sql";
        private const string CheckTablesExistScriptPath = "SqlScripts/CheckTablesExist.sql";

        private static DatabaseHelper? instance;

        private DatabaseHelper()
        {
            InitializeDatabase();
        }

        /// <summary>
        /// Gets the singleton instance of the <see cref="DatabaseHelper"/>.
        /// </summary>
        public static DatabaseHelper Instance => instance ??= new();

        /// <summary>
        /// Ensures the database is initialized by checking for required tables and creating them if missing.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="App.ConnectionString"/> is not set.
        /// </exception>
        /// <exception cref="DatabaseInitializationException">
        /// Thrown if a SQL error occurs or a required script is missing during initialization.
        /// </exception>
        public static void InitializeDatabase()
        {
            if (string.IsNullOrWhiteSpace(App.ConnectionString))
            {
                throw new InvalidOperationException("Connection string is not initialized.");
            }

            try
            {
                bool tablesExist = CheckIfTablesExist();
                if (!tablesExist)
                {
                    CreateDatabaseTables();
                }
            }
            catch (SqlScriptMissingException ex)
            {
                throw new DatabaseInitializationException("Missing required SQL script during initialization.", ex);
            }
            catch (SqlException ex)
            {
                throw new DatabaseInitializationException("SQL error during database initialization.", ex);
            }
        }

        /// <summary>
        /// Safely closes the given SQL connection if it is open.
        /// </summary>
        /// <param name="connection">The <see cref="SqlConnection"/> to close.</param>
        public static void CloseConnection(SqlConnection connection)
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// Opens and returns a new <see cref="SqlConnection"/> using <see cref="App.ConnectionString"/>.
        /// </summary>
        /// <returns>An open <see cref="SqlConnection"/>.</returns>
        /// <exception cref="DatabaseInitializationException">
        /// Thrown if the connection cannot be opened.
        /// </exception>
        public static SqlConnection GetConnection()
        {
            SqlConnection connection = new(App.ConnectionString);

            try
            {
                connection.Open();
                return connection;
            }
            catch (SqlException ex)
            {
                throw new DatabaseInitializationException("Failed to open SQL connection.", ex);
            }
        }

        private static bool CheckIfTablesExist()
        {
            try
            {
                // Open a new connection to check for table existence
                using SqlConnection connection = new(App.ConnectionString);
                connection.Open();

                string sql = LoadSqlScript(CheckTablesExistScriptPath);

                using SqlCommand command = new(sql, connection);
                return Convert.ToBoolean(command.ExecuteScalar() ?? false);
            }
            catch (FileNotFoundException ex)
            {
                throw new SqlScriptMissingException($"SQL script file '{CheckTablesExistScriptPath}' was not found.", ex);
            }
            catch (SqlException ex)
            {
                throw new DatabaseInitializationException("An error occurred while checking if database tables exist.", ex);
            }
        }

        private static void ResetDatabase()
        {
            const string masterConnection = @"
        Data Source=DESKTOP-2UI353C\SQLEXPRESS;
        Initial Catalog=StockApp_DB;
        Integrated Security=True;
        Trust Server Certificate=True";

            try
            {
                using SqlConnection connection = new(masterConnection);
                connection.Open();

                string sql = LoadSqlScript(ResetDatabaseScriptPath);

                using SqlCommand command = new(sql, connection);
                command.ExecuteNonQuery();
            }
            catch (FileNotFoundException ex)
            {
                throw new SqlScriptMissingException(ResetDatabaseScriptPath, ex);
            }
            catch (SqlException ex)
            {
                throw new DatabaseInitializationException("Failed to reset the database.", ex);
            }
        }

        private static void CreateDatabaseTables()
        {
            try
            {
                // Open a connection to create necessary tables
                using SqlConnection connection = new(App.ConnectionString);
                connection.Open();

                string sql = LoadSqlScript(CreateTablesScriptPath);

                using SqlCommand command = new(sql, connection);
                command.ExecuteNonQuery();
            }
            catch (FileNotFoundException ex)
            {
                throw new SqlScriptMissingException(CreateTablesScriptPath, ex);
            }
            catch (SqlException ex)
            {
                throw new DatabaseInitializationException("Failed to create database tables.", ex);
            }
        }

        private static string LoadSqlScript(string relativePath)
        {
            // Build the full path to the SQL script file
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            if (!File.Exists(fullPath))
            {
                throw new SqlScriptMissingException(fullPath);
            }

            // Read and return the SQL script contents
            return File.ReadAllText(fullPath);
        }
    }
}
