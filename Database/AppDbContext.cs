namespace StockApp.Database
{
    using Microsoft.EntityFrameworkCore;
    using Src.Model;
    using StockApp.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Add parameterless constructor for direct instantiation in code
        public AppDbContext() : base()
        {
        }

        public DbSet<ChatReport> ChatReports { get; set; }
        public DbSet<BaseStock> BaseStocks { get; set; }
        public DbSet<Alert> Alerts { get; set; } = null!;
        public DbSet<CreditScoreHistory> CreditScoreHistories { get; set; } = null!;


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

            modelBuilder.Entity<CreditScoreHistory>(entity =>
            {
                entity.ToTable("CreditScoreHistory");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserCnp).IsRequired().HasMaxLength(13);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Score).IsRequired();
            });

            modelBuilder.Entity<ChatReport>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ReportedUserCnp)
                      .IsRequired()
                      .HasMaxLength(15);

                entity.Property(e => e.ReportedMessage)
                      .IsRequired();
            });

            modelBuilder.Entity<GemStore>(entity =>
            {
                entity.ToTable("GemStore");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cnp).IsRequired().HasMaxLength(13);
                entity.Property(e => e.GemBalance).IsRequired();
                entity.Property(e => e.IsGuest).IsRequired();
                entity.Property(e => e.LastUpdated).IsRequired();
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
                    optionsBuilder.UseSqlServer("Data Source=VM;Initial Catalog=StockApp_DB;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");
                }
            }
        }
    }
}