namespace Src.Model
{
    using System;

    public class ChatReport
    {
        public int Id { get; set; }

        public string ReportedUserCnp { get; set; }  // foreign key

        public string ReportedMessage { get; set; }

        public ChatReport(int id = 0, string reportedUserCNP = "", string reportedMessage = "")
        {
            this.Id = id;
            this.ReportedUserCnp = reportedUserCNP;
            this.ReportedMessage = reportedMessage;
        }
    }
}
