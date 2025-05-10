namespace BankApi.Seeders
{
    public class ChatReportsSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM ChatReports) 
            BEGIN
                INSERT INTO ChatReports 
                    (ReportedUserCnp, ReportedMessage)
                VALUES
                    ('1234567890123', 'This user sent inappropriate content.'),
                    ('9876543210987', 'Reported for spamming multiple messages.'),
                    ('2345678901234', 'This user violated chat guidelines.'),
                    ('3456789012345', 'Reported for offensive language.'),
                    ('4567890123456', 'User harassed another member.');
            END;
        ";
    }
}