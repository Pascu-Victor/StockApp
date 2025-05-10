namespace BankApi.Seeders
{
    public class BillSplitReportsSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM BillSplitReports) 
            BEGIN
                INSERT INTO BillSplitReports
                    (ReportedUserCnp, ReportingUserCnp, DateOfTransaction, BillShare)
                VALUES
                    ('1234567890123', '9876543210987', '2025-04-01', 150.00),
                    ('2345678901234', '3456789012345', '2025-03-15', 90.25),
                    ('9876543210987', '1234567890123', '2025-02-20', 200.50),
                    ('3456789012345', '4567890123456', '2025-01-10', 75.75),
                    ('4567890123456', '2345678901234', '2025-05-05', 180.00);
            END;
        ";
    }
}