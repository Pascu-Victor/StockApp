using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockApp.Models
{
    public class GemStore
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(13)]
        public string UserCnp { get; set; }

        [Required]
        public int GemBalance { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }

        [ForeignKey("UserCnp")]
        public virtual User User { get; set; }
    }
} 