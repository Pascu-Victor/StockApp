namespace BankApi.Seeders
{
    public class ActivityLogsSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM ActivityLogs) 
            BEGIN
                INSERT INTO ActivityLogs 
                    (UserCnp, ActivityName, LastModifiedAmount, ActivityDetails, CreatedAt)
                VALUES
                    ('1234567890123', 'Deposit', 5000, 'Deposited funds into savings account', '2025-04-01'),
                    ('9876543210987', 'Withdrawal', -1200, 'Withdrew cash from ATM', '2025-03-15'),
                    ('2345678901234', 'Investment', 3500, 'Invested in stock market', '2025-02-20'),
                    ('3456789012345', 'Loan Payment', -800, 'Paid monthly loan installment', '2025-01-10'),
                    ('4567890123456', 'Account Transfer', -1500, 'Transferred funds to another account', '2025-05-05');
            END;
        ";
    }
}