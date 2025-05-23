using Common.Models;
using Common.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StockApp.Services
{
    public class MessagesProxyService(HttpClient httpClient) : IProxyService, IMessagesService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task GiveMessageToUserAsync(string userCnp, string type, string messageText)
        {
            var request = new { Type = type, MessageText = messageText };
            await _httpClient.PostAsJsonAsync($"api/Messages/User/{userCnp}/give", request);
        }

        public async Task<List<Message>> GetMessagesForUserAsync(string userCnp)
        {
            // If userCnp is provided, get messages for the specified user (admin only)
            if (!string.IsNullOrEmpty(userCnp))
            {
                return await _httpClient.GetFromJsonAsync<List<Message>>($"api/Messages/user/{userCnp}") ??
                    throw new InvalidOperationException("Failed to deserialize messages for user response.");
            }

            // If no userCnp is provided, get messages for the current user
            return await _httpClient.GetFromJsonAsync<List<Message>>("api/Messages/user") ??
                throw new InvalidOperationException("Failed to deserialize messages response.");
        }
    }
}