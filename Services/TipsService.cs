
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Src.Model;
using StockApp.Models;
using StockApp.Repositories;
using StockApp.Repositories.Api; // <-- for TipsProxyRepo

namespace StockApp.Services
{
    public class TipsService : ITipsService
    {
        private readonly TipsProxyRepo _tipsProxyRepo;

        public TipsService(TipsProxyRepo tipsProxyRepo)
        {
            _tipsProxyRepo = tipsProxyRepo;

        }

        public async Task GiveTipToUserAsync(string userCNP)
        {
            UserRepository userRepository = new UserRepository();
            try
            {

                int creditScore = userRepository.GetUserByCnpAsync(userCNP).Result.CreditScore;




                if (creditScore < 300)
                {
                    await _tipsProxyRepo.GiveLowBracketTipAsync(userCNP);
                }
                else if (creditScore < 550)
                {
                    await _tipsProxyRepo.GiveMediumBracketTipAsync(userCNP);
                }
                else
                {
                    await _tipsProxyRepo.GiveHighBracketTipAsync(userCNP);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GiveTipToUserAsync: {ex.Message}");
            }
        }

        public async Task<List<Tip>> GetTipsForGivenUserAsync(string userCnp)
        {
            try
            {
                return await _tipsProxyRepo.GetTipsForGivenUserAsync(userCnp);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTipsForGivenUserAsync: {ex.Message}");
                return new List<Tip>();
            }
        }
    }
}