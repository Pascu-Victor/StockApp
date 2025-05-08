using Src.Data;
using Src.Helpers;
using Src.Model;
using StockApp.Models;
using StockApp.Repositories;
using StockApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ChatReportService : IChatReportService
{
    private readonly IChatReportRepository chatReportRepository;

    public ChatReportService(IChatReportRepository chatReportRepository)
    {
        this.chatReportRepository = chatReportRepository;
    }

    public async Task DoNotPunishUser(ChatReport chatReportToBeSolved)
    {
        await this.chatReportRepository.DeleteChatReportAsync(chatReportToBeSolved.Id);
    }

    public async Task PunishUser(ChatReport chatReportToBeSolved)
    {
        UserRepository userRepo = new UserRepository();
        DatabaseConnection dbConn = new DatabaseConnection();

        User reportedUser = await userRepo.GetUserByCnpAsync(chatReportToBeSolved.ReportedUserCnp);

        int noOffenses = reportedUser.NumberOfOffenses;
        const int MINIMUM = 3;
        const int FLAT_PENALTY = 15;

        int amount = noOffenses >= MINIMUM
            ? FLAT_PENALTY * noOffenses
            : FLAT_PENALTY;

        await userRepo.PenalizeUserAsync(chatReportToBeSolved.ReportedUserCnp, amount);

        int updatedScore = (await userRepo.GetUserByCnpAsync(chatReportToBeSolved.ReportedUserCnp)).CreditScore - amount;
        await this.chatReportRepository.UpdateScoreHistoryForUserAsync(chatReportToBeSolved.ReportedUserCnp, updatedScore);

        await userRepo.IncrementOffensesCountAsync(chatReportToBeSolved.ReportedUserCnp);
        await this.chatReportRepository.DeleteChatReportAsync(chatReportToBeSolved.Id);

        var tipsService = new TipsService(new TipsRepository(dbConn));
        tipsService.GiveTipToUser(chatReportToBeSolved.ReportedUserCnp);

        int tipCount = await this.chatReportRepository.GetNumberOfGivenTipsForUserAsync(chatReportToBeSolved.ReportedUserCnp);
        if (tipCount % 3 == 0)
        {
            var msgService = new MessagesService(new MessagesRepository(dbConn));
            msgService.GiveMessageToUser(chatReportToBeSolved.ReportedUserCnp);
        }

        await this.chatReportRepository.UpdateActivityLogAsync(chatReportToBeSolved.ReportedUserCnp, amount);
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
