namespace BankApi.Seeders
{
    public class InvestmentsSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM Investments) 
            BEGIN
                INSERT INTO Investments 
                    (InvestorCnp, Details, AmountInvested, AmountReturned, InvestmentDate)
                VALUES
                    ('1234567890123', 'Tech startup funding', 5000.00, 5500.00, '2025-04-01'),
                    ('9876543210987', 'Stock market investment', 12000.50, 13000.00, '2025-03-15'),
                    ('2345678901234', 'Real estate project', 3500.75, 4000.00, '2025-02-20'),
                    ('3456789012345', 'Cryptocurrency investment', 8000.00, 7500.00, '2025-01-10'),
                    ('4567890123456', 'Renewable energy initiative', 15000.25, 16000.00, '2025-05-05');
            END;
        ";
    }
}