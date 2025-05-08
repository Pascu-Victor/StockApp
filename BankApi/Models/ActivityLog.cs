namespace BankApi.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        public string UserCnp { get; set; }

        public string ActivityName { get; set; }

        public int LastModifiedAmount { get; set; }

        public string ActivityDetails { get; set; }

        public ActivityLog(int id, string userCNP, string name, int amount, string details)
        {
            this.Id = id;
            this.UserCnp = userCNP;
            this.ActivityName = name;
            this.LastModifiedAmount = amount;
            this.ActivityDetails = details;
        }

        public ActivityLog()
        {
            this.Id = 0;
            this.UserCnp = string.Empty;
            this.ActivityName = string.Empty;
            this.LastModifiedAmount = 0;
            this.ActivityDetails = string.Empty;
        }
    }
}
