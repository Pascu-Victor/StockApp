using Common.Models;
using Common.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StockApp.Services
{
    public class TransactionLogProxy : IProxyService, ITransactionLogService
    {
        private readonly HttpClient _httpClient;

        public TransactionLogProxy(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<TransactionLogTransaction>> GetFilteredTransactions(TransactionFilterCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria), "Filter criteria cannot be null");
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/TransactionLog/filter", criteria);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<TransactionLogTransaction>>() ??
                    throw new InvalidOperationException("Failed to deserialize filtered transactions response.");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
            {
                // Handle bad request (400) responses
                throw new InvalidTransactionFilterCriteriaException("Invalid filter criteria");
            }
        }

        public List<TransactionLogTransaction> SortTransactions(
            List<TransactionLogTransaction> transactions,
            string sortType = "Date",
            bool ascending = true)
        {
            if (transactions == null)
            {
                throw new ArgumentNullException(nameof(transactions), "Transactions list cannot be null");
            }

            try
            {
                // We'll do client-side sorting like the original implementation
                return sortType switch
                {
                    "Date" => ascending
                        ? [.. transactions.OrderBy(t => t.Date)]
                        : [.. transactions.OrderByDescending(t => t.Date)],
                    "Stock Name" => ascending
                        ? [.. transactions.OrderBy(t => t.StockName)]
                        : [.. transactions.OrderByDescending(t => t.StockName)],
                    "Total Value" => ascending
                        ? [.. transactions.OrderBy(t => t.TotalValue)]
                        : [.. transactions.OrderByDescending(t => t.TotalValue)],
                    _ => throw new InvalidSortTypeException(sortType),
                };
            }
            catch (Exception ex) when (ex is not InvalidSortTypeException)
            {
                throw new InvalidOperationException("Error while sorting transactions", ex);
            }
        }

        public void ExportTransactions(
            List<TransactionLogTransaction> transactions,
            string filePath,
            string format)
        {
            if (transactions == null)
            {
                throw new ArgumentNullException(nameof(transactions), "Transactions list cannot be null");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(format))
            {
                throw new ArgumentException("Export format is required", nameof(format));
            }

            try
            {
                // We'll implement the export on the client side
                format = format.ToLower();
                switch (format)
                {
                    case "csv":
                        ExportToCsv(transactions, filePath);
                        break;
                    case "json":
                        ExportToJson(transactions, filePath);
                        break;
                    case "html":
                        ExportToHtml(transactions, filePath);
                        break;
                    default:
                        throw new ExportFormatNotSupportedException(format);
                }
            }
            catch (Exception ex) when (ex is not ExportFormatNotSupportedException)
            {
                throw new InvalidOperationException($"Error exporting transactions to {format}", ex);
            }
        }

        private static void ExportToCsv(List<TransactionLogTransaction> transactions, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            // Write header
            writer.WriteLine("Id,StockSymbol,StockName,Type,Amount,PricePerStock,TotalValue,Date,AuthorCNP");
            // Write data rows
            foreach (var transaction in transactions)
            {
                writer.WriteLine($"{transaction.Id},{transaction.StockSymbol},{transaction.StockName},{transaction.Type},"
                    + $"{transaction.Amount},{transaction.PricePerStock},{transaction.TotalValue},{transaction.Date},{transaction.AuthorCNP}");
            }
        }

        private static void ExportToJson(List<TransactionLogTransaction> transactions, string filePath)
        {
            var options = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(transactions, options);
            File.WriteAllText(filePath, jsonString);
        }

        private static void ExportToHtml(List<TransactionLogTransaction> transactions, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            writer.WriteLine("<!DOCTYPE html>");
            writer.WriteLine("<html>");
            writer.WriteLine("<head><title>Transaction Log</title>");
            writer.WriteLine("<style>");
            writer.WriteLine("table{border-collapse:collapse;width:100%;}");
            writer.WriteLine("th,td{border:1px solid #ddd;padding:8px;text-align:left;}");
            writer.WriteLine("th{background-color:#f2f2f2;}");
            writer.WriteLine("</style>");
            writer.WriteLine("</head>");
            writer.WriteLine("<body>");
            writer.WriteLine("<h1>Transaction Log</h1>");
            writer.WriteLine("<table>");
            writer.WriteLine("<tr><th>ID</th><th>Stock Symbol</th><th>Stock Name</th><th>Type</th><th>Amount</th>"
                + "<th>Price Per Stock</th><th>Total Value</th><th>Date</th><th>Author CNP</th></tr>");
            foreach (var transaction in transactions)
            {
                writer.WriteLine($"<tr><td>{transaction.Id}</td><td>{transaction.StockSymbol}</td><td>{transaction.StockName}</td>"
                    + $"<td>{transaction.Type}</td><td>{transaction.Amount}</td><td>{transaction.PricePerStock}</td>"
                    + $"<td>{transaction.TotalValue}</td><td>{transaction.Date}</td><td>{transaction.AuthorCNP}</td></tr>");
            }
            writer.WriteLine("</table>");
            writer.WriteLine("</body>");
            writer.WriteLine("</html>");
        }
    }

    public class InvalidSortTypeException : Exception
    {
        public InvalidSortTypeException(string sortType)
            : base($"Invalid sort type: {sortType}. Valid types are 'Date', 'Stock Name', and 'Total Value'.")
        {
        }
    }

    public class ExportFormatNotSupportedException : Exception
    {
        public ExportFormatNotSupportedException(string format)
            : base($"Export format '{format}' is not supported. Supported formats are 'csv', 'json', and 'html'.")
        {
        }
    }

    public class InvalidTransactionFilterCriteriaException : Exception
    {
        public InvalidTransactionFilterCriteriaException(string message)
            : base(message)
        {
        }
    }

    internal class SortTransactionsRequestDto
    {
        public List<TransactionLogTransaction> Transactions { get; set; } = [];
        public string SortType { get; set; } = "Date";
        public bool Ascending { get; set; } = true;
    }

    internal class ExportTransactionsRequestDto
    {
        public TransactionFilterCriteria Criteria { get; set; } = new TransactionFilterCriteria();
        public string Format { get; set; } = "csv";
    }
}