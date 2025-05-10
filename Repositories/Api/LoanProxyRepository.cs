using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using StockApp.Models;

namespace StockApp.Repositories.Api
{
    internal class LoanProxyRepository : ILoanRepository
    {
        private readonly HttpClient _httpClient;

        public LoanProxyRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<List<Loan>> GetLoansAsync()
        {
            try
            {
                Console.WriteLine($"Calling GET {_httpClient.BaseAddress}api/Loan");
                var response = await _httpClient.GetAsync("api/Loan");
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response: {(int)response.StatusCode} {response.ReasonPhrase} - {content}");
                }
                
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Loan>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLoansAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Loan> GetLoanByIdAsync(int loanID)
        {
            try
            {
                Console.WriteLine($"Calling GET {_httpClient.BaseAddress}api/Loan/{loanID}");
                var response = await _httpClient.GetAsync($"api/Loan/{loanID}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response: {(int)response.StatusCode} {response.ReasonPhrase} - {content}");
                }
                
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Loan>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLoanByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task AddLoanAsync(Loan loan)
        {
            try
            {
                Console.WriteLine($"Calling POST {_httpClient.BaseAddress}api/Loan");
                var response = await _httpClient.PostAsJsonAsync("api/Loan", loan);
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response: {(int)response.StatusCode} {response.ReasonPhrase} - {content}");
                }
                
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddLoanAsync: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateLoanAsync(Loan loan)
        {
            try
            {
                Console.WriteLine($"Calling PUT {_httpClient.BaseAddress}api/Loan/{loan.Id}");
                var response = await _httpClient.PutAsJsonAsync($"api/Loan/{loan.Id}", loan);
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response: {(int)response.StatusCode} {response.ReasonPhrase} - {content}");
                }
                
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateLoanAsync: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteLoanAsync(int loanID)
        {
            try
            {
                Console.WriteLine($"Calling DELETE {_httpClient.BaseAddress}api/Loan/{loanID}");
                var response = await _httpClient.DeleteAsync($"api/Loan/{loanID}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response: {(int)response.StatusCode} {response.ReasonPhrase} - {content}");
                }
                
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteLoanAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Loan>> GetUserLoansAsync(string userCNP)
        {
            try
            {
                Console.WriteLine($"Calling GET {_httpClient.BaseAddress}api/Loan?userCnp={userCNP}");
                var response = await _httpClient.GetAsync($"api/Loan?userCnp={userCNP}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response: {(int)response.StatusCode} {response.ReasonPhrase} - {content}");
                }
                
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Loan>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserLoansAsync: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateCreditScoreHistoryForUserAsync(string userCNP, int newScore)
        {
            try
            {
                Console.WriteLine($"Calling PATCH {_httpClient.BaseAddress}api/User/{userCNP}/creditScore?newScore={newScore}");
                var response = await _httpClient.PatchAsync($"api/User/{userCNP}/creditScore?newScore={newScore}", null);
                
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response: {(int)response.StatusCode} {response.ReasonPhrase} - {content}");
                }
                
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateCreditScoreHistoryForUserAsync: {ex.Message}");
                throw;
            }
        }
    }
}
