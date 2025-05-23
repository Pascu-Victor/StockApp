using Common.Models;
using Common.Services;

namespace StockAppWeb.Services
{
    public class ChatReportProxyService(HttpClient httpClient) : IProxyService, IChatReportService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<List<ChatReport>> GetAllChatReportsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<ChatReport>>("api/ChatReport") ??
                throw new InvalidOperationException("Failed to deserialize chat reports response.");
        }

        public async Task<ChatReport?> GetChatReportByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ChatReport>($"api/ChatReport/{id}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task AddChatReportAsync(ChatReport report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report), "Chat report cannot be null");
            }

            var response = await _httpClient.PostAsJsonAsync("api/ChatReport", report);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteChatReportAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/ChatReport/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<int> GetNumberOfGivenTipsForUserAsync(string? userCnp = null)
        {
            string endpoint = string.IsNullOrEmpty(userCnp)
                ? "api/ChatReport/user-tips/current"
                : $"api/ChatReport/user-tips/{userCnp}";

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task UpdateActivityLogAsync(int amount, string? userCnp = null)
        {
            var updateDto = new ActivityLogUpdateDto { Amount = amount };
            var response = await _httpClient.PostAsJsonAsync("api/ChatReport/activity-log", updateDto);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateScoreHistoryForUserAsync(int newScore, string? userCnp = null)
        {
            var updateDto = new ScoreHistoryUpdateDto { NewScore = newScore };
            var response = await _httpClient.PostAsJsonAsync("api/ChatReport/score-history", updateDto);
            response.EnsureSuccessStatusCode();
        }

        // Implementing the new interface methods
        public async Task PunishUser(ChatReport chatReportToBeSolved)
        {
            if (chatReportToBeSolved == null)
            {
                throw new ArgumentNullException(nameof(chatReportToBeSolved), "Chat report cannot be null");
            }

            var punishmentDto = new PunishmentMessageDto
            {
                ShouldPunish = true,
                MessageContent = "Your message was reported and found to be in violation of our community standards."
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"api/ChatReport/punish-with-message/{chatReportToBeSolved.Id}",
                punishmentDto);

            response.EnsureSuccessStatusCode();
        }

        public async Task DoNotPunishUser(ChatReport chatReportToBeSolved)
        {
            if (chatReportToBeSolved == null)
            {
                throw new ArgumentNullException(nameof(chatReportToBeSolved), "Chat report cannot be null");
            }

            var punishmentDto = new PunishmentMessageDto
            {
                ShouldPunish = false,
                MessageContent = string.Empty // No message needed since we're not punishing
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"api/ChatReport/punish-with-message/{chatReportToBeSolved.Id}",
                punishmentDto);

            response.EnsureSuccessStatusCode();
        }

        public async Task<bool> IsMessageOffensive(string messageToBeChecked)
        {
            if (string.IsNullOrEmpty(messageToBeChecked))
            {
                return false;
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/ChatReport/check-message",
                    new { Message = messageToBeChecked });

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<bool>();
            }
            catch
            {
                // If the API call fails, assume the message is not offensive
                return false;
            }
        }

        // Send a message to a user
        public async Task SendMessageToUser(string userCnp, string messageContent, string messageType = "System")
        {
            if (string.IsNullOrEmpty(userCnp))
            {
                throw new ArgumentException("User CNP cannot be null or empty", nameof(userCnp));
            }

            if (string.IsNullOrEmpty(messageContent))
            {
                throw new ArgumentException("Message content cannot be null or empty", nameof(messageContent));
            }

            var messageDto = new SendMessageDto
            {
                UserCnp = userCnp,
                MessageType = messageType,
                MessageContent = messageContent
            };

            var response = await _httpClient.PostAsJsonAsync("api/ChatReport/send-message", messageDto);
            response.EnsureSuccessStatusCode();
        }
    }

    // DTOs needed for API calls
    internal class ActivityLogUpdateDto
    {
        public int Amount { get; set; }
    }

    internal class ScoreHistoryUpdateDto
    {
        public int NewScore { get; set; }
    }

    internal class PunishmentMessageDto
    {
        public bool ShouldPunish { get; set; } = true;
        public string MessageContent { get; set; } = string.Empty;
    }

    internal class SendMessageDto
    {
        public string UserCnp { get; set; } = string.Empty;
        public string MessageType { get; set; } = "System";
        public string MessageContent { get; set; } = string.Empty;
    }
}