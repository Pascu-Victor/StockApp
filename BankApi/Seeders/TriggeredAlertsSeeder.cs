namespace BankApi.Seeders
{
    public class TriggeredAlertsSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM TriggeredAlerts) 
            BEGIN
                INSERT INTO TriggeredAlerts 
                    (StockName, Message, TriggeredAt)
                VALUES
                    ('AAPL', 'Stock price exceeded upper bound of $180.00', '2025-04-01'),
                    ('GOOGL', 'Stock price dropped below lower bound of $2500.00', '2025-03-15'),
                    ('TSLA', 'Rapid volume surge detected', '2025-02-20'),
                    ('AMZN', 'Stock price exceeded upper bound of $3500.00', '2025-01-10'),
                    ('MSFT', 'Unusual market movement in after-hours trading', '2025-05-05');
            END;
        ";
    }
}