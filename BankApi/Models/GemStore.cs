using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankApi.Models
{
    public class GemStore
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Cnp { get; set; } = string.Empty;

        [Required]
        public int GemBalance { get; set; }

        [Required]
        public bool IsGuest { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }
    }
}