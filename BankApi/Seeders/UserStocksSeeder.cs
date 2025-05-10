namespace BankApi.Seeders
{
    public class UserStocksSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM UserStocks) 
            BEGIN
                INSERT INTO UserStocks 
                    (UserCnp, StockName, Quantity)
                VALUES
                    ('1234567890123', 'Apple Inc.', 10),
                    ('9876543210987', 'Alphabet Inc.', 5),
                    ('2345678901234', 'Tesla Inc.', 20),
                    ('3456789012345', 'Amazon.com Inc.', 8),
                    ('4567890123456', 'Microsoft Corp.', 15);
            END;
        ";
    }
}