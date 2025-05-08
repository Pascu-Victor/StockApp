using System;
using System.ComponentModel.DataAnnotations;

namespace StockApp.Models
{
    public class GemStore
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Cnp { get; set; } = string.Empty;

        [Required]
        public int GemBalance { get; set; }

        [Required]
        public bool IsGuest { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
} 