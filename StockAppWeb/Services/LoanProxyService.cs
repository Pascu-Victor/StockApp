using Common.Models;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StockAppWeb.Services
{
    public class LoanProxyService(HttpClient httpClient) : IProxyService, ILoanService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<List<Loan>> GetLoansAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Loan>>("api/Loan") ??
                throw new InvalidOperationException("Failed to deserialize loans response.");
        }

        public async Task<List<Loan>> GetUserLoansAsync(string userCNP)
        {
            if (string.IsNullOrEmpty(userCNP))
            {
                // Get current user's loans
                return await _httpClient.GetFromJsonAsync<List<Loan>>("api/Loan/user") ??
                    throw new InvalidOperationException("Failed to deserialize user loans response.");
            }
            else
            {
                // Get loans for a specific user (admin only)
                return await _httpClient.GetFromJsonAsync<List<Loan>>($"api/Loan/user/{userCNP}") ??
                    throw new InvalidOperationException("Failed to deserialize user loans response.");
            }
        }

        public async Task AddLoanAsync(LoanRequest loanRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Loan", loanRequest);
            response.EnsureSuccessStatusCode();
        }

        public async Task CheckLoansAsync()
        {
            var response = await _httpClient.PostAsync("api/Loan/check", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task IncrementMonthlyPaymentsCompletedAsync(int loanID, decimal penalty)
        {
            var payment = new PaymentDto { Penalty = penalty };
            var response = await _httpClient.PostAsJsonAsync($"api/Loan/{loanID}/increment-payment", payment);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateHistoryForUserAsync(string userCNP, int newScore)
        {
            var response = await _httpClient.PostAsync($"api/Loan/update-history?userCnp={userCNP}&newScore={newScore}", null);
            response.EnsureSuccessStatusCode();
        }
    }
}