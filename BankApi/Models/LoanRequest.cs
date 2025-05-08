namespace BankApi.Models
{
    using System;

    public class LoanRequest
    {
        public int Id { get; set; }

        public string UserCnp { get; set; }

        public float Amount { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime RepaymentDate { get; set; }

        public string Status { get; set; }

        public LoanRequest(int requestId, string userCnp, float amount, DateTime applicationDate, DateTime repaymentDate, string status)
        {
            this.Id = requestId;
            this.UserCnp = userCnp;
            this.Amount = amount;
            this.ApplicationDate = applicationDate;
            this.RepaymentDate = repaymentDate;
            this.Status = status;
        }
    }
}
