namespace BankApi.Seeders
{
    public class GemStoresSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM GemStores) 
            BEGIN
                INSERT INTO GemStores 
                    (Cnp, GemBalance, IsGuest, LastUpdated)
                VALUES
                    ('1234567890123', 500, 0, '2025-04-01'),
                    ('9876543210987', 1200, 0, '2025-03-15'),
                    ('2345678901234', 350, 1, '2025-02-20'),
                    ('3456789012345', 800, 0, '2025-01-10'),
                    ('4567890123456', 1500, 1, '2025-05-05');
            END;
        ";
    }
}