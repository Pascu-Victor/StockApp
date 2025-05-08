namespace BankApi.Models
{
    using System;

    public class GivenTip
    {
        public int Id { get; set; }

        public string UserCnp { get; set; }

        public int TipID { get; set; }

        public int MessageID { get; set; }

        public DateOnly Date { get; set; }

        public GivenTip(int id, string userCNP, int tipID, int messageID, DateOnly date)
        {
            this.Id = id;
            this.UserCnp = userCNP;
            this.TipID = tipID;
            this.MessageID = messageID;
            this.Date = date;
        }

        public GivenTip()
        {
            this.Id = 0;
            this.UserCnp = string.Empty;
            this.TipID = 0;
            this.MessageID = 0;
            this.Date = new DateOnly();
        }
    }
}
