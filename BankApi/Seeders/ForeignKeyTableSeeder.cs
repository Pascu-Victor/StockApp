namespace BankApi.Seeders
{
    using Microsoft.Data.SqlClient;

    public abstract class ForeignKeyTableSeeder(IConfiguration configuration) : TableSeeder(configuration)
    {
        public override async Task SeedAsync()
        {
            try
            {
                using SqlConnection conn = new(this.connectionString);
                await conn.OpenAsync();

                using SqlCommand cmd = new(await this.GetQueryAsync(), conn);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Database seeding failed: {ex.Message}");
                throw;
            }
        }

        protected abstract string GetReferencedTableName();

        protected abstract string GetQueryWithForeignKeys(List<int> foreignKeys);

        private async Task<List<int>> GetForeignKeys()
        {
            List<int> ids = [];
            string referencedTableName = this.GetReferencedTableName();

            // Fetch existing User IDs dynamically
            using SqlConnection conn = new(this.connectionString);
            await conn.OpenAsync();

            using SqlCommand cmd = new($"SELECT Id FROM {referencedTableName} ORDER BY Id ASC", conn);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                ids.Add(reader.GetInt32(0));
            }

            // Ensure we have at least 5 users before proceeding
            if (ids.Count < 5)
            {
                throw new InvalidOperationException($"Not enough records in the {referencedTableName} table to seed.");
            }

            return ids;
        }
        
        private async Task<string> GetQueryAsync()
        {
            var foreignKeys = await this.GetForeignKeys();
            return this.GetQueryWithForeignKeys(foreignKeys);
        }
    }
}
