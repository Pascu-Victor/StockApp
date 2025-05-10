namespace BankApi.Seeders
{
    public class BaseStocksSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM BaseStocks) 
            BEGIN
                INSERT INTO BaseStocks 
                    (Name, Symbol, AuthorCNP, Discriminator, Price, Quantity)
                VALUES
                    ('Apple Inc.', 'AAPL', '1234567890123', 'Stock', 175.50, 100),
                    ('Alphabet Inc.', 'GOOGL', '9876543210987', 'Stock', 2850.75, 50),
                    ('Tesla Inc.', 'TSLA', '2345678901234', 'Stock', 725.30, 200),
                    ('Amazon.com Inc.', 'AMZN', '3456789012345', 'Stock', 3425.60, 80),
                    ('Microsoft Corp.', 'MSFT', '4567890123456', 'Stock', 325.20, 150);
            END;
        ";
    }
}