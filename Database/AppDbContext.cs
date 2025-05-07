namespace StockApp.Database
{
    using Microsoft.EntityFrameworkCore;
    using StockApp.Models;
    using Src.Model;

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
        public DbSet<ActivityLog> ActivityLogs { get; set; }

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

            // Configure ActivityLog entity
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.ToTable("ActivityLog");
                entity.HasKey(e => e.Id);
                
                // Configure relationship with User
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserCnp)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure indexes
                entity.HasIndex(e => e.UserCnp);
                entity.HasIndex(e => e.CreatedAt);
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