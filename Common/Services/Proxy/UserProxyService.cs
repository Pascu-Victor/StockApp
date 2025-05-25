using Common.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Common.Services.Proxy
{
    public class UserProxyService(HttpClient httpClient, IOptions<JsonOptions> jsonOptions) : IProxyService, IUserService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions.Value.SerializerOptions ?? throw new ArgumentNullException(nameof(jsonOptions), "JsonSerializerOptions cannot be null.");

        public async Task CreateUser(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("api/User", user, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task<User> GetCurrentUserAsync(string? userCNP = null) // userCNP is not used by the API endpoint for current user
        {
            // If userCNP is provided, it implies fetching a specific user, otherwise the current one.
            // However, the UserController has GetCurrentUser for the authenticated user (no CNP in path)
            // and GetUserByCnp for a specific user.
            // The IUserService interface's GetCurrentUserAsync(string? userCNP = null) is a bit ambiguous here.
            // Assuming if userCNP is null, we get the current authenticated user.
            // If userCNP is not null, we should call the specific user endpoint.
            // For now, sticking to the "current" user endpoint as per method name.
            // A dedicated GetUserByCnpAsync will handle the other case.
            var response = await _httpClient.GetAsync("api/User/current");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>(_jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize user.");
        }

        public async Task<int> GetCurrentUserGemsAsync(string? userCNP = null)
        {
            // This seems to map to StoreController's GetUserGemBalanceAsync
            // Assuming the API endpoint is "api/Store/user-gem-balance"
            // The userCNP parameter in IUserService.GetCurrentUserGemsAsync might be problematic if the API
            // always derives the user from the token for this specific call.
            // The StoreController's GetUserGemBalance uses the authenticated user's CNP.
            // If userCNP is provided and different, this proxy method might not behave as expected without API changes.
            // For now, we assume the API uses the authenticated user.
            var response = await _httpClient.GetAsync("api/Store/user-gem-balance");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>(_jsonOptions);
        }

        public async Task<User> GetUserByCnpAsync(string cnp)
        {
            if (string.IsNullOrEmpty(cnp))
            {
                throw new ArgumentNullException(nameof(cnp));
            }

            var response = await _httpClient.GetAsync($"api/User/{cnp}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>(_jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize user.");
        }

        public async Task<List<User>> GetUsers()
        {
            var response = await _httpClient.GetAsync("api/User");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<User>>(_jsonOptions) ?? [];
        }

        public async Task UpdateIsAdminAsync(bool newIsAdmin, string? userCNP = null)
        {
            if (string.IsNullOrEmpty(userCNP))
            {
                throw new ArgumentNullException(nameof(userCNP), "User CNP must be provided to update admin status.");
            }

            var payload = new { IsAdmin = newIsAdmin };
            var response = await _httpClient.PutAsJsonAsync($"api/User/{userCNP}/admin-status", payload, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateUserAsync(User user, string? userCNP = null)
        {
            ArgumentNullException.ThrowIfNull(user);

            var payload = new { 
                UserName = user.UserName, 
                Image = user.Image, 
                Description = user.Description, 
                IsHidden = user.IsHidden,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            HttpResponseMessage response;
            
            if (string.IsNullOrEmpty(userCNP))
            {
                // Update current user
                response = await _httpClient.PutAsJsonAsync("api/User/current", payload, _jsonOptions);
            }
            else
            {
                // Update user by CNP (typically admin action)
                response = await _httpClient.PutAsJsonAsync($"api/User/{userCNP}", payload, _jsonOptions);
            }

            response.EnsureSuccessStatusCode();
        }

        public async Task<int> AddDefaultRoleToAllUsersAsync()
        {
            // This operation is admin-only and calls the endpoint we created
            var response = await _httpClient.PostAsync("api/User/add-default-role", null);
            response.EnsureSuccessStatusCode();

            // Parse the response - it returns a message with the count of updated users
            var result = await response.Content.ReadFromJsonAsync<DefaultRoleResponse>(_jsonOptions);
            if (result == null)
            {
                return 0;
            }

            // Extract the number from the message (e.g., "Successfully added the 'User' role to 5 users")
            string numberPart = result.Message.Split(' ').FirstOrDefault(part => int.TryParse(part, out _)) ?? "0";
            return int.TryParse(numberPart, out int count) ? count : 0;
        }

        private class DefaultRoleResponse
        {
            public string Message { get; set; } = string.Empty;
        }
    }
}
