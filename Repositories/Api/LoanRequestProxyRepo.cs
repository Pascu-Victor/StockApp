using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Repositories.Api
{
    public class LoanRequestProxyRepo : ILoanRequestRepository
    {
        private readonly HttpClient _httpClient;

        public LoanRequestProxyRepo(HttpClient httpClient)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<LoanRequest>> GetLoanRequestsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/LoanRequest");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<LoanRequest>>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error calling GetLoanRequestsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<LoanRequest>> GetUnsolvedLoanRequestsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/LoanRequest/unsolved");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<LoanRequest>>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error calling GetUnsolvedLoanRequestsAsync: {ex.Message}");
                throw;
            }
        }

        public async Task SolveLoanRequestAsync(LoanRequest loanRequest)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"api/LoanRequest/{loanRequest.Id}/solve", null);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error calling SolveLoanRequestAsync: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteLoanRequestAsync(LoanRequest loanRequest)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/LoanRequest/{loanRequest.Id}");
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error calling DeleteLoanRequestAsync: {ex.Message}");
                throw;
            }
        }
    }
}
