using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Models
{
    public class LoanRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(13)]
        public string UserCnp { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime RepaymentDate { get; set; }

        [Required]
        [MaxLength(100)]
        public string Status { get; set; }

    }
}
