using Common.Models;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StockAppWeb.Services
{
    public class LoanRequestProxyService(HttpClient httpClient) : IProxyService, ILoanRequestService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<List<LoanRequest>> GetLoanRequests()
        {
            return await _httpClient.GetFromJsonAsync<List<LoanRequest>>("api/LoanRequest") ??
                throw new InvalidOperationException("Failed to deserialize loan requests response.");
        }

        public async Task<List<LoanRequest>> GetUnsolvedLoanRequests()
        {
            return await _httpClient.GetFromJsonAsync<List<LoanRequest>>("api/LoanRequest/unsolved") ??
                throw new InvalidOperationException("Failed to deserialize unsolved loan requests response.");
        }

        public async Task<string> GiveSuggestion(LoanRequest loanRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/LoanRequest/suggestion", loanRequest);
            response.EnsureSuccessStatusCode();

            string suggestion = await response.Content.ReadAsStringAsync() ??
                throw new InvalidOperationException("Failed to deserialize suggestion response.");

            return suggestion;
        }

        public async Task SolveLoanRequest(int loanRequestId)
        {
            var response = await _httpClient.PostAsync($"api/LoanRequest/{loanRequestId}/solve", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteLoanRequest(int loanRequestId)
        {
            var response = await _httpClient.DeleteAsync($"api/LoanRequest/{loanRequestId}");
            response.EnsureSuccessStatusCode();
        }
    }
}