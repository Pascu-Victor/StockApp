namespace BankApi.Models
{
    using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankApi.Models
{
    public class BillSplitReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(13)]
        public string ReportedUserCnp { get; set; }

        [Required]
        [StringLength(13)]
        public string ReportingUserCnp { get; set; }

        [Required]
        public DateTime DateOfTransaction { get; set; }

        [Required]
        public float BillShare { get; set; }

        public BillSplitReport(int id, string reportedCNP, string reporterCNP, DateTime dateTransaction, float billShare)
        {
            this.Id = id;
            this.ReportedUserCnp = reportedCNP;
            this.ReportingUserCnp = reporterCNP;
            this.DateOfTransaction = dateTransaction;
            this.BillShare = billShare;
        }

        public BillSplitReport()
        {
            this.Id = 0;
            this.ReportedUserCnp = string.Empty;
            this.ReportingUserCnp = string.Empty;
            this.DateOfTransaction = DateTime.Now;
            this.BillShare = 0;
        }
    }
}