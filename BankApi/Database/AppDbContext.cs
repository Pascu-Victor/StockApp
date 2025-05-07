namespace BankApi.Database
{
    using Microsoft.EntityFrameworkCore;
    using StockApp.Models;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.CNP).IsRequired();
                entity.Property(u => u.Username).IsRequired();
                entity.Property(u => u.Email).IsRequired(false);
                entity.Property(u => u.FirstName).IsRequired(false);
                entity.Property(u => u.LastName).IsRequired(false);
            });
        }
    }
}
