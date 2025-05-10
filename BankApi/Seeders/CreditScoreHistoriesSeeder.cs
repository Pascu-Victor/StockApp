namespace BankApi.Seeders
{
    public class CreditScoreHistoriesSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM CreditScoreHistories) 
            BEGIN
                INSERT INTO CreditScoreHistories 
                    (UserCnp, Date, Score)
                VALUES
                    ('1234567890123', '2025-04-01', 720),
                    ('9876543210987', '2025-03-15', 650),
                    ('2345678901234', '2025-02-20', 780),
                    ('3456789012345', '2025-01-10', 600),
                    ('4567890123456', '2025-05-05', 740);
            END;
        ";
    }
}