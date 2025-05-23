using Common.Models;
using Common.Services;

namespace StockAppWeb.Services
{
    public class StoreProxyService(HttpClient httpClient) : IProxyService, IStoreService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<string> BuyGems(GemDeal deal, string selectedAccountId, string? userCNP = null)
        {
            // The controller expects a GemDealDto which includes Title, GemAmount, Price, and SelectedAccountId.
            // userCNP is derived from claims on the server.
            var dealDto = new { deal.Title, deal.GemAmount, deal.Price, SelectedAccountId = selectedAccountId };
            var response = await _httpClient.PostAsJsonAsync("api/Store/buy-gems", dealDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<int> GetUserGemBalanceAsync(string? userCNP = null)
        {
            // userCNP is derived from claims on the server for this controller method.
            return await _httpClient.GetFromJsonAsync<int>("api/Store/user-gem-balance");
        }

        public async Task<string> SellGems(int gemAmount, string selectedAccountId, string? userCNP = null)
        {
            // The controller expects a SellGemsDto which includes GemAmount and SelectedAccountId.
            // userCNP is derived from claims on the server.
            var sellGemsDto = new { GemAmount = gemAmount, SelectedAccountId = selectedAccountId };
            var response = await _httpClient.PostAsJsonAsync("api/Store/sell-gems", sellGemsDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task UpdateUserGemBalanceAsync(int newBalance, string? userCNP = null)
        {
            // The controller expects an UpdateGemBalanceDto which includes UserCnp and NewBalance.
            // This method is Admin only, so userCNP must be provided if it's for another user.
            // If userCNP is null, it implies updating the current admin's balance, which might not be the intent.
            // For now, assume userCNP will be provided correctly by the caller for admin operations.
            var updateDto = new { UserCnp = userCNP, NewBalance = newBalance };
            var response = await _httpClient.PutAsJsonAsync("api/Store/user-gem-balance", updateDto);
            response.EnsureSuccessStatusCode();
        }
    }
}