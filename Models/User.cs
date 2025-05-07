namespace StockApp.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Represents an application user, with profile details, financial information, and permissions.
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string CNP { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string HashedPassword { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsModerator { get; set; } = false;

        public string Image { get; set; } = string.Empty;

        public bool IsHidden { get; set; } = false;

        public int GemBalance { get; set; } = 0;

        public int NumberOfOffenses { get; set; } = 0;

        public int RiskScore { get; set; } = 0;

        public decimal ROI { get; set; } = 0;

        public int CreditScore { get; set; } = 0;

        public DateOnly Birthday { get; set; }

        public string ZodiacSign { get; set; } = string.Empty;

        public string ZodiacAttribute { get; set; } = string.Empty;

        public int NumberOfBillSharesPaid { get; set; } = 0;

        public int Income { get; set; } = 0;

        public decimal Balance { get; set; } = 0;
    }
}
