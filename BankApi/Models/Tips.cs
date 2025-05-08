namespace BankApi.Models
{
    public class Tip
    {
        public int Id { get; set; }

        public string CreditScoreBracket { get; set; }

        public string TipText { get; set; }

        public Tip(int id, string creditScoreBracket, string tipText)
        {
            this.Id = id;
            this.CreditScoreBracket = creditScoreBracket;
            this.TipText = tipText;
        }

        public Tip()
        {
            this.Id = 0;
            this.CreditScoreBracket = string.Empty;
            this.TipText = string.Empty;
        }
    }
}
