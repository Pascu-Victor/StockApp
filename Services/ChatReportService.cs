using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Src.Helpers;
using Src.Model;
using StockApp.Models;
using StockApp.Repositories;
using StockApp.Services;

public class ChatReportService(IChatReportRepository chatReportRepository, IUserRepository userRepository, ITipsService tipsService, IMessagesService messageService) : IChatReportService
{
    private readonly IChatReportRepository chatReportRepository = chatReportRepository ?? throw new System.ArgumentNullException(nameof(chatReportRepository));
    private readonly IUserRepository userRepository = userRepository ?? throw new System.ArgumentNullException(nameof(userRepository));
    private readonly ITipsService tipsService = tipsService ?? throw new System.ArgumentNullException(nameof(tipsService));
    private readonly IMessagesService messageService = messageService ?? throw new System.ArgumentNullException(nameof(messageService));

    public async Task DoNotPunishUser(ChatReport chatReportToBeSolved)
    {
        await this.chatReportRepository.DeleteChatReportAsync(chatReportToBeSolved.Id);
    }

    public async Task PunishUser(ChatReport chatReportToBeSolved)
    {
        if (chatReportToBeSolved == null)
        {
            throw new ArgumentNullException(nameof(chatReportToBeSolved), "Chat report cannot be null");
        }

        if (string.IsNullOrEmpty(chatReportToBeSolved.ReportedUserCnp))
        {
            throw new ArgumentException("Reported user CNP cannot be null or empty", nameof(chatReportToBeSolved));
        }

        User reportedUser = await userRepository.GetByCnpAsync(chatReportToBeSolved.ReportedUserCnp) ?? throw new Exception("User not found");

        int noOffenses = reportedUser.NumberOfOffenses;
        const int MINIMUM = 3;
        const int FLAT_PENALTY = 15;

        int amount = noOffenses >= MINIMUM
            ? FLAT_PENALTY * noOffenses
            : FLAT_PENALTY;

        // Use the PunishUserAsync method if available, otherwise fall back to regular update
        bool success = false;
        try
        {
            // Try to use the specialized punishment method first
            var repository = userRepository as dynamic;
            success = await repository.PunishUserAsync(reportedUser.Id, amount);
        }
        catch (Exception)
        {
            // Fall back to standard update when specialized method isn't available
            reportedUser.GemBalance -= amount;
            reportedUser.NumberOfOffenses++;
            success = await userRepository.UpdateAsync(reportedUser.Id, reportedUser);
        }

        if (!success)
        {
            throw new Exception("Failed to update user for punishment");
        }

        int updatedScore = reportedUser.CreditScore - amount;
        try
        {
            await this.chatReportRepository.UpdateScoreHistoryForUserAsync(chatReportToBeSolved.ReportedUserCnp, updatedScore);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating score history: {ex.Message}");
        }

        try 
        {
            await this.chatReportRepository.DeleteChatReportAsync(chatReportToBeSolved.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting chat report: {ex.Message}");
            // Continue with other operations even if deletion fails
        }

        // Cache the CNP before proceeding with other operations
        string reportedUserCnp = chatReportToBeSolved.ReportedUserCnp;
        int penaltyAmount = amount;

        try
        {
            await tipsService.GiveTipToUser(reportedUserCnp);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error giving tip to user: {ex.Message}");
        }

        try
        {
            int tipCount = await this.chatReportRepository.GetNumberOfGivenTipsForUserAsync(reportedUserCnp);
            if (tipCount % 3 == 0)
            {
                await Task.Run(() => messageService.GiveMessageToUser(reportedUserCnp));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing tip count: {ex.Message}");
        }

        try
        {
            await this.chatReportRepository.UpdateActivityLogAsync(reportedUserCnp, penaltyAmount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating activity log: {ex.Message}");
        }
    }

    public async Task<bool> IsMessageOffensive(string messageToBeChecked)
    {
        return await ProfanityChecker.IsMessageOffensive(messageToBeChecked);
    }

    public async Task<List<ChatReport>> GetChatReports()
    {
        return await this.chatReportRepository.GetAllChatReportsAsync();
    }

    public async Task DeleteChatReport(int id)
    {
        await this.chatReportRepository.DeleteChatReportAsync(id);
    }

    public async Task UpdateHistoryForUser(string userCNP, int newScore)
    {
        await this.chatReportRepository.UpdateScoreHistoryForUserAsync(userCNP, newScore);
    }

}
