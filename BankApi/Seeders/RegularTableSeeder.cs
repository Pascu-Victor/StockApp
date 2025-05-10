namespace BankApi.Seeders
{
    using Microsoft.Data.SqlClient;

    public abstract class RegularTableSeeder(IConfiguration configuration) : TableSeeder(configuration)
    {
        protected abstract string GetQuery();

        public override async Task SeedAsync()
        {
            try
            {
                using SqlConnection conn = new(this.connectionString);
                await conn.OpenAsync();

                using SqlCommand cmd = new(this.GetQuery(), conn);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Database seeding failed: {ex.Message}");
                throw;
            }
        }
    }
}
