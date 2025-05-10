namespace BankApi.Seeders
{
    public class LoansSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM Loans) 
            BEGIN
                INSERT INTO Loans 
                    (UserCnp, LoanAmount, ApplicationDate, RepaymentDate, InterestRate, NumberOfMonths, MonthlyPaymentAmount, Status, MonthlyPaymentsCompleted, RepaidAmount, Penalty)
                VALUES
                    ('1234567890123', 5000.00, '2025-04-01', '2027-04-01', 5.5, 24, 220.50, 'Pending', 0, 0.00, 0.00),
                    ('9876543210987', 12000.50, '2025-03-15', '2026-03-15', 4.0, 12, 1050.25, 'Approved', 3, 3150.75, 0.00),
                    ('2345678901234', 3500.75, '2025-02-20', '2026-02-20', 6.2, 18, 215.00, 'Rejected', 0, 0.00, 0.00),
                    ('3456789012345', 8000.00, '2025-01-10', '2028-01-10', 3.8, 36, 275.75, 'Pending', 0, 0.00, 0.00),
                    ('4567890123456', 15000.25, '2025-05-05', '2027-05-05', 5.0, 24, 670.00, 'Approved', 5, 3350.00, 50.00);
            END;
        ";
    }
}