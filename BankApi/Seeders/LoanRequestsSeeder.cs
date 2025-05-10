namespace BankApi.Seeders
{
    public class LoanRequestsSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM LoanRequests) 
            BEGIN
                INSERT INTO LoanRequests 
                    (UserCnp, Amount, ApplicationDate, RepaymentDate, Status)
                VALUES
                    ('1234567890123', 5000.00, '2025-04-01', '2025-10-01', 'Pending'),
                    ('9876543210987', 12000.50, '2025-03-15', '2025-09-15', 'Approved'),
                    ('2345678901234', 3500.75, '2025-02-20', '2025-08-20', 'Rejected'),
                    ('3456789012345', 8000.00, '2025-01-10', '2025-07-10', 'Pending'),
                    ('4567890123456', 15000.25, '2025-05-05', '2025-11-05', 'Approved');
            END;
        ";
    }
}
