using BankApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }

        public DbSet<BaseStock> BaseStocks { get; set; }
        public DbSet<BillSplitReport> BillSplitReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure BaseStock entity
            modelBuilder.Entity<BaseStock>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<BaseStock>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<BaseStock>()
                .Property(s => s.Name)
                .IsRequired();

            modelBuilder.Entity<BaseStock>()
                .Property(s => s.Symbol)
                .IsRequired();

            modelBuilder.Entity<BaseStock>()
                .Property(s => s.AuthorCNP)
                .IsRequired();

            // Configure BillSplitReport entity
            modelBuilder.Entity<BillSplitReport>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<BillSplitReport>()
                .Property(b => b.ReportedUserCnp)
                .IsRequired();

            modelBuilder.Entity<BillSplitReport>()
                .Property(b => b.ReportingUserCnp)
                .IsRequired();

            modelBuilder.Entity<BillSplitReport>()
                .Property(b => b.DateOfTransaction)
                .IsRequired();

            modelBuilder.Entity<BillSplitReport>()
                .Property(b => b.BillShare)
                .IsRequired();
        }
    }
}