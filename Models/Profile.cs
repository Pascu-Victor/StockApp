using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockApp.Models
{
    /// <summary>
    /// Represents a user profile in the application.
    /// </summary>
    public class Profile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(13)]
        public string CNP { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsModerator { get; set; }

        [StringLength(200)]
        public string Image { get; set; } = string.Empty;

        public bool IsHidden { get; set; }

        public int GemBalance { get; set; }

        public int NumberOfOffenses { get; set; }

        public int RiskScore { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ROI { get; set; }

        public int CreditScore { get; set; }

        public DateTime Birthday { get; set; }

        [StringLength(20)]
        public string ZodiacSign { get; set; } = string.Empty;

        [StringLength(50)]
        public string ZodiacAttribute { get; set; } = string.Empty;

        public int NumberOfBillSharesPaid { get; set; }

        public int Income { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        [Required]
        [StringLength(100)]
        public string HashedPassword { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastModifiedAt { get; set; }

        public virtual ICollection<Stock> UserStocks { get; set; }
    }
} 