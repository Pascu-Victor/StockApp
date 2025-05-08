namespace BankApi.Models
{
    public class InvestmentPortfolio
    {
        public string FirstName { get; set; }

        public string SecondName { get; set; }

        public decimal TotalAmountInvested { get; set; }

        public decimal TotalAmountReturned { get; set; }

        public decimal AverageROI { get; set; }

        public int NumberOfInvestments { get; set; }

        public int RiskFactor { get; set; }

        public InvestmentPortfolio(string firstName, string secondName, decimal totalAmountInvested, decimal totalAmountReturned, decimal averageROI, int numberOfInvestments, int riskFactor)
        {
            this.FirstName = firstName;
            this.SecondName = secondName;
            this.TotalAmountInvested = totalAmountInvested;
            this.TotalAmountReturned = totalAmountReturned;
            this.AverageROI = averageROI;
            this.NumberOfInvestments = numberOfInvestments;
            this.RiskFactor = riskFactor;
        }
    }
}
