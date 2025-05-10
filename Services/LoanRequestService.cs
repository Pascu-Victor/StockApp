namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Src.Data;
    using Src.Model;
    using StockApp.Models;
    using StockApp.Repositories;

    public class LoanRequestService : ILoanRequestService
    {
        private readonly ILoanRequestRepository loanRequestRepository;
        private readonly IUserRepository userRepository;

        public LoanRequestService(ILoanRequestRepository loanRequestRepository, IUserRepository userRepository)
        {
            this.userRepository = userRepository;
            this.loanRequestRepository = loanRequestRepository;
        }

        public async Task<string> GiveSuggestion(LoanRequest loanRequest)
        {

            User user = await this.userRepository.GetByCnpAsync(loanRequest.UserCnp) ?? throw new Exception("User not found");

            string givenSuggestion = string.Empty;

            if (loanRequest.Amount > user.Income * 10)
            {
                givenSuggestion = "Amount requested is too high for user income";
            }

            if (user.CreditScore < 300)
            {
                if (givenSuggestion.Length > 0)
                {
                    givenSuggestion += ", ";
                }

                givenSuggestion += "Credit score is too low";
            }

            if (user.RiskScore > 70)
            {
                if (givenSuggestion.Length > 0)
                {
                    givenSuggestion += ", ";
                }

                givenSuggestion += "User risk score is too high";
            }

            if (givenSuggestion.Length > 0)
            {
                givenSuggestion = "User does not qualify for loan: " + givenSuggestion;
            }

            return givenSuggestion;
        }

        public async Task SolveLoanRequest(LoanRequest loanRequest)
        {
            await loanRequestRepository.SolveLoanRequestAsync(loanRequest);
        }

        public  async Task DeleteLoanRequest(LoanRequest loanRequest)
        {
            await loanRequestRepository.DeleteLoanRequestAsync(loanRequest);
        }

        public async Task<List<LoanRequest>> GetLoanRequests()
        {
            return await loanRequestRepository.GetLoanRequestsAsync();
        }

        public async Task<List<LoanRequest>> GetUnsolvedLoanRequests()
        {
            return await loanRequestRepository.GetUnsolvedLoanRequestsAsync();
        }
    }
}