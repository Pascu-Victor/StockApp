using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using StockApp.Models;
using StockApp.Exceptions;

namespace StockApp.Services
{
    public class ProfileApiService : IProfileApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "api/profile";

        public ProfileApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<Profile> GetProfileByCnpAsync(string cnp)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Profile>($"{BaseUrl}/{cnp}") 
                    ?? throw new ProfileNotFoundException($"Profile not found for CNP: {cnp}");
            }
            catch (Exception ex) when (ex is not ProfileNotFoundException)
            {
                throw new ProfilePersistenceException("Error retrieving profile", ex);
            }
        }

        public async Task<Profile> GetCurrentProfileAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Profile>($"{BaseUrl}/current") 
                    ?? throw new ProfileNotFoundException("Current profile not found");
            }
            catch (Exception ex) when (ex is not ProfileNotFoundException)
            {
                throw new ProfilePersistenceException("Error retrieving current profile", ex);
            }
        }

        public async Task<string> GenerateUsernameAsync()
        {
            try
            {
                return await _httpClient.GetStringAsync($"{BaseUrl}/generate-username");
            }
            catch (Exception ex)
            {
                throw new ProfilePersistenceException("Error generating username", ex);
            }
        }

        public async Task UpdateProfileAsync(string cnp, string newUsername, string newImage, string newDescription, bool newHidden)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{cnp}", new
                {
                    Username = newUsername,
                    Image = newImage,
                    Description = newDescription,
                    IsHidden = newHidden
                });

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new ProfilePersistenceException("Error updating profile", ex);
            }
        }

        public async Task UpdateIsAdminAsync(string cnp, bool isAdmin)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{cnp}/admin", new { IsAdmin = isAdmin });
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new ProfilePersistenceException("Error updating admin status", ex);
            }
        }

        public async Task<List<Stock>> GetUserStocksAsync(string cnp)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Stock>>($"{BaseUrl}/{cnp}/stocks") 
                    ?? new List<Stock>();
            }
            catch (Exception ex)
            {
                throw new ProfilePersistenceException("Error retrieving user stocks", ex);
            }
        }
    }
} 