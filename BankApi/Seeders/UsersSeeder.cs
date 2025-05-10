namespace BankApi.Seeders
{
    public class UsersSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM Users) 
            BEGIN
                INSERT INTO Users 
                    (CNP, Username, FirstName, LastName, Email, PhoneNumber, HashedPassword, Description, IsModerator, Image, IsHidden, GemBalance, NumberOfOffenses, RiskScore, ROI, CreditScore, Birthday, ZodiacSign, ZodiacAttribute, NumberOfBillSharesPaid, Income, Balance)
                VALUES
                    ('1234567890123', 'user1', 'John', 'Doe', 'john.doe@example.com', '1234567890', 'hashedpass1', 'User one', 0, 'user1.jpg', 0, 100, 0, 5, 1.5, 700, '1990-05-15', 'Taurus', 'Earth', 2, 5000, 10000),
                    ('9876543210987', 'user2', 'Jane', 'Smith', 'jane.smith@example.com', '0987654321', 'hashedpass2', 'User two', 1, 'user2.jpg', 0, 200, 1, 3, 2.0, 750, '1995-08-21', 'Leo', 'Fire', 5, 6000, 15000),
                    ('2345678901234', 'user3', 'Alice', 'Brown', 'alice.brown@example.com', '2345678901', 'hashedpass3', 'User three', 0, 'user3.jpg', 1, 50, 3, 7, 1.2, 680, '1988-11-03', 'Scorpio', 'Water', 3, 4000, 8000),
                    ('3456789012345', 'user4', 'Bob', 'Williams', 'bob.williams@example.com', '3456789012', 'hashedpass4', 'User four', 1, 'user4.jpg', 0, 300, 0, 2, 2.5, 780, '2000-02-10', 'Aquarius', 'Air', 4, 7000, 20000),
                    ('4567890123456', 'user5', 'Charlie', 'Davis', 'charlie.davis@example.com', '4567890123', 'hashedpass5', 'User five', 0, 'user5.jpg', 1, 20, 5, 9, 0.8, 650, '1985-06-30', 'Cancer', 'Water', 1, 3000, 5000);
            END;
        ";
    }
}