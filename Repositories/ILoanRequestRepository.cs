﻿namespace StockApp.Repositories
{
    using System;
    using System.Collections.Generic;
    using Src.Model;

    public interface ILoanRequestRepository
    {
        public List<LoanRequest> GetLoanRequests();

        public List<LoanRequest> GetUnsolvedLoanRequests();

        public void SolveLoanRequest(int loanRequestID);

        public void DeleteLoanRequest(int loanRequestID);
    }
}
