namespace BankApi.Seeders
{
    public class AlertsSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM Alerts) 
            BEGIN
                INSERT INTO Alerts 
                    (StockName, Name, UpperBound, LowerBound, ToggleOnOff)
                VALUES
                    ('AAPL', 'Apple Stock Alert', 180.00, 120.00, 1),
                    ('GOOGL', 'Alphabet Price Watch', 2900.00, 2500.00, 1),
                    ('TSLA', 'Tesla Trading Alert', 800.00, 600.00, 1),
                    ('AMZN', 'Amazon Stock Monitor', 3500.00, 3000.00, 0),
                    ('MSFT', 'Microsoft Price Alert', 350.00, 280.00, 1);
            END;
        ";
    }
}