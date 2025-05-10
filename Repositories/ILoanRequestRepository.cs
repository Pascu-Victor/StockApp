namespace StockApp.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;

    public interface ILoanRequestRepository
    {
        Task<List<LoanRequest>> GetLoanRequestsAsync();

        Task<List<LoanRequest>> GetUnsolvedLoanRequestsAsync();

        Task SolveLoanRequestAsync(LoanRequest loanRequest);

        Task DeleteLoanRequestAsync(LoanRequest loanRequest);
    }
}
