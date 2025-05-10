namespace BankApi.Seeders
{
    public abstract class TableSeeder(IConfiguration configuration)
    {
        protected readonly string connectionString = configuration.GetConnectionString("DefaultConnection")!;

        public abstract Task SeedAsync();
    }
}