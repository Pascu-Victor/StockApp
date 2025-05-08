namespace BankApi.Models
{
    using System;

    public class Loan
    {
        public int Id { get; set; }

        public string UserCnp { get; set; }

        public float LoanAmount { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime RepaymentDate { get; set; }

        public float InterestRate { get; set; }

        public int NumberOfMonths { get; set; }

        public float MonthlyPaymentAmount { get; set; }

        public string Status { get; set; }

        public int MonthlyPaymentsCompleted { get; set; }

        public float RepaidAmount { get; set; }

        public float Penalty { get; set; }

        public Loan(int loanID, string userCnp, float loanAmount, DateTime applicationDate, DateTime repaymentDate, float interestRate, int numberOfMonths, float monthlyPaymentAmount, int monthlyPaymentsCompleted, float repaidAmount, float penalty, string status)
        {
            this.Id = loanID;
            this.UserCnp = userCnp;
            this.LoanAmount = loanAmount;
            this.ApplicationDate = applicationDate;
            this.RepaymentDate = repaymentDate;
            this.InterestRate = interestRate;
            this.NumberOfMonths = numberOfMonths;
            this.MonthlyPaymentAmount = monthlyPaymentAmount;
            this.Status = status;
            this.MonthlyPaymentsCompleted = monthlyPaymentsCompleted;
            this.RepaidAmount = repaidAmount;
            this.Penalty = penalty;
        }
    }
}

