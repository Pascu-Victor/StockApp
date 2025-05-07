using Microsoft.EntityFrameworkCore;
using StockApp.Models;

namespace StockApp.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Add parameterless constructor for direct instantiation in code
        public AppDbContext() : base()
        {
        }

        public DbSet<BaseStock> BaseStocks { get; set; }
        public DbSet<Alert> Alerts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure BaseStock entity
            modelBuilder.Entity<BaseStock>(entity =>
            {
                entity.ToTable("STOCK");
                entity.HasKey(e => e.Name);
                entity.Property(e => e.Name).HasColumnName("STOCK_NAME");
                entity.Property(e => e.Symbol).HasColumnName("STOCK_SYMBOL");
                entity.Property(e => e.AuthorCNP).HasColumnName("AUTHOR_CNP");
            });

            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasKey(e => e.AlertId);
                entity.Property(e => e.StockName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UpperBound).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LowerBound).HasColumnType("decimal(18,2)");
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Use connection string from App.ConnectionString if available, otherwise use a default
                try
                {
                    optionsBuilder.UseSqlServer(App.ConnectionString);
                }
                catch
                {
                    // Fallback to a local database if ConnectionString is not available
                    optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=StockApp;Trusted_Connection=True;");
                }
            }
        }
    }
} 