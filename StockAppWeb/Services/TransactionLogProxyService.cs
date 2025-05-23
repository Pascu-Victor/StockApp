using Common.Models;
using Common.Services;
using System.Net.Http.Json;

namespace StockAppWeb.Services
{
    public class TransactionLogProxyService : ITransactionLogService
    {
        private readonly HttpClient _httpClient;

        public TransactionLogProxyService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<TransactionLogTransaction>> GetFilteredTransactions(TransactionFilterCriteria criteria)
        {
            var response = await _httpClient.PostAsJsonAsync("api/transactionlog/filter", criteria);
            response.EnsureSuccessStatusCode();
            var transactions = await response.Content.ReadFromJsonAsync<IEnumerable<TransactionLogTransaction>>() ?? Array.Empty<TransactionLogTransaction>();
            return transactions.ToList();
        }

        public List<TransactionLogTransaction> SortTransactions(List<TransactionLogTransaction> transactions, string sortType = "Date", bool ascending = true)
        {
            return sortType.ToLower() switch
            {
                "stockname" => ascending
                    ? transactions.OrderBy(t => t.StockName).ToList()
                    : transactions.OrderByDescending(t => t.StockName).ToList(),
                "type" => ascending
                    ? transactions.OrderBy(t => t.Type).ToList()
                    : transactions.OrderByDescending(t => t.Type).ToList(),
                "amount" => ascending
                    ? transactions.OrderBy(t => t.Amount).ToList()
                    : transactions.OrderByDescending(t => t.Amount).ToList(),
                "totalvalue" => ascending
                    ? transactions.OrderBy(t => t.TotalValue).ToList()
                    : transactions.OrderByDescending(t => t.TotalValue).ToList(),
                "date" => ascending
                    ? transactions.OrderBy(t => t.Date).ToList()
                    : transactions.OrderByDescending(t => t.Date).ToList(),
                "author" => ascending
                    ? transactions.OrderBy(t => t.Author).ToList()
                    : transactions.OrderByDescending(t => t.Author).ToList(),
                _ => transactions
            };
        }

        public void ExportTransactions(List<TransactionLogTransaction> transactions, string filePath, string format)
        {
            var url = $"api/transactionlog/export/{format}?filePath={filePath}";
            var response = _httpClient.PostAsJsonAsync(url, transactions).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
        }
    }
} 