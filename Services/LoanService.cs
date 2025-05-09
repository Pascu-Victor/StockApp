﻿namespace StockApp.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StockApp.Models;
    using StockApp.Repositories;

    public class LoanService : ILoanService
    {
        private readonly ILoanRepository loanRepository;
        private readonly IUserRepository userRepository;

        public LoanService(ILoanRepository loanRepository, IUserRepository userRepository)
        {
            this.loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<List<Loan>> GetLoansAsync()
        {
            return await this.loanRepository.GetLoansAsync();
        }

        public async Task<List<Loan>> GetUserLoansAsync(string userCNP)
        {
            return await this.loanRepository.GetUserLoansAsync(userCNP);
        }

        public async Task AddLoanAsync(LoanRequest loanRequest)
        {
            User user = await userRepository.GetByCnpAsync(loanRequest.UserCnp) ?? throw new Exception("User not found");

            decimal interestRate = (decimal)user.RiskScore / user.CreditScore * 100;
            int noMonths = ((loanRequest.RepaymentDate.Year - loanRequest.ApplicationDate.Year) * 12) + loanRequest.RepaymentDate.Month - loanRequest.ApplicationDate.Month;
            decimal monthlyPaymentAmount = (decimal)loanRequest.Amount * (1 + (interestRate / 100)) / noMonths;
            int monthlyPaymentsCompleted = 0;
            int repaidAmount = 0;
            decimal penalty = 0;

            Loan loan = new Loan(
                loanRequest.UserCnp,
                loanRequest.Amount,
                loanRequest.ApplicationDate,
                loanRequest.RepaymentDate,
                interestRate,
                noMonths,
                monthlyPaymentAmount,
                monthlyPaymentsCompleted,
                repaidAmount,
                penalty,
                "active");

            await this.loanRepository.AddLoanAsync(loan);
        }

        public async Task CheckLoansAsync()
        {
            List<Loan> loanList = await this.loanRepository.GetLoansAsync();
            foreach (Loan loan in loanList)
            {
                int numberOfMonthsPassed = ((DateTime.Today.Year - loan.ApplicationDate.Year) * 12) + DateTime.Today.Month - loan.ApplicationDate.Month;
                User user = await this.userRepository.GetByCnpAsync(loan.UserCnp) ?? throw new Exception("User not found");
                if (loan.MonthlyPaymentsCompleted >= loan.NumberOfMonths)
                {
                    loan.Status = "completed";
                    int newUserCreditScore = ILoanService.ComputeNewCreditScore(user, loan);

                    user.CreditScore = newUserCreditScore;
                    await this.userRepository.UpdateAsync(user.Id, user);
                }

                if (numberOfMonthsPassed > loan.MonthlyPaymentsCompleted)
                {
                    int numberOfOverdueDays = (DateTime.Today - loan.ApplicationDate.AddMonths(loan.MonthlyPaymentsCompleted)).Days;
                    decimal penalty = (decimal)(0.1 * numberOfOverdueDays);
                    loan.Penalty = penalty;
                }
                else
                {
                    loan.Penalty = 0;
                }

                if (DateTime.Today > loan.RepaymentDate && loan.Status == "active")
                {
                    loan.Status = "overdue";
                    int newUserCreditScore = ILoanService.ComputeNewCreditScore(user, loan);

                    user.CreditScore = newUserCreditScore;
                    await this.userRepository.UpdateAsync(user.Id, user);
                    await this.UpdateHistoryForUserAsync(loan.UserCnp, newUserCreditScore);
                }
                else if (loan.Status == "overdue")
                {
                    if (loan.MonthlyPaymentsCompleted >= loan.NumberOfMonths)
                    {
                        loan.Status = "completed";
                        int newUserCreditScore = ILoanService.ComputeNewCreditScore(user, loan);

                        user.CreditScore = newUserCreditScore;
                        await this.userRepository.UpdateAsync(user.Id, user);
                        await this.UpdateHistoryForUserAsync(loan.UserCnp, newUserCreditScore);
                    }
                }

                if (loan.Status == "completed")
                {
                    await this.loanRepository.DeleteLoanAsync(loan.Id);
                }
                else
                {
                    await this.loanRepository.UpdateLoanAsync(loan);
                }
            }
        }

        public async Task UpdateHistoryForUserAsync(string userCNP, int newScore)
        {
            await this.loanRepository.UpdateCreditScoreHistoryForUserAsync(userCNP, newScore);
        }

        public async Task IncrementMonthlyPaymentsCompletedAsync(int loanID, decimal penalty)
        {
            Loan loan = await this.loanRepository.GetLoanByIdAsync(loanID);
            loan.MonthlyPaymentsCompleted++;
            loan.RepaidAmount += loan.MonthlyPaymentAmount + penalty;
            await this.loanRepository.UpdateLoanAsync(loan);
        }
    }
}
