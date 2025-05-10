namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;
    using StockApp.Repositories;

    public class TipsService : ITipsService
    {
        private ITipsRepository tipsRepository;
        private IUserRepository userRepository;

        public TipsService(ITipsRepository tipsRepository, IUserRepository userRepository)
        {
            this.tipsRepository = tipsRepository;
            this.userRepository = userRepository;
        }

        public async Task GiveTipToUser(string userCNP)
        {

            try
            {
                User user = await this.userRepository.GetByCnpAsync(userCNP) ?? throw new Exception("User not found");
                if (user.CreditScore < 300)
                {
                    await this.tipsRepository.GiveLowBracketTipAsync(userCNP);
                }
                else if (user.CreditScore < 550)
                {
                    await this.tipsRepository.GiveMediumBracketTipAsync(userCNP);
                }
                else if (user.CreditScore > 549)
                {
                    await this.tipsRepository.GiveHighBracketTipAsync(userCNP);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{exception.Message},User is not found");
            }
        }

        public async Task<List<Tip>> GetTipsForGivenUser(string userCnp)
        {
            return await this.tipsRepository.GetTipsForGivenUserAsync(userCnp);
        }
    }
}
