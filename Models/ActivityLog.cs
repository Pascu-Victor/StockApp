namespace Src.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using StockApp.Models;

    [Table("ActivityLog")]
    public class ActivityLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(13)]
        public string UserCnp { get; set; }

        [Required]
        [StringLength(100)]
        public string ActivityName { get; set; }

        public int LastModifiedAmount { get; set; }

        [StringLength(500)]
        public string ActivityDetails { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for User relationship
        [ForeignKey("UserCnp")]
        public virtual User? User { get; set; }

        public ActivityLog()
        {
            this.UserCnp = string.Empty;
            this.ActivityName = string.Empty;
            this.LastModifiedAmount = 0;
            this.ActivityDetails = string.Empty;
        }

        public ActivityLog(int id, string userCNP, string name, int amount, string details)
        {
            this.Id = id;
            this.UserCnp = userCNP;
            this.ActivityName = name;
            this.LastModifiedAmount = amount;
            this.ActivityDetails = details;
            this.CreatedAt = DateTime.UtcNow;
        }
    }
}
