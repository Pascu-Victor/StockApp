namespace BankApi.Seeders
{
    public class GivenTipsSeeder(IConfiguration configuration) : ForeignKeyTableSeeder(configuration)
    {
        protected override string GetReferencedTableName() => "Tips";

        protected override string GetQueryWithForeignKeys(List<int> foreignKeys)
        {
            return $@"
                IF NOT EXISTS (SELECT 1 FROM GivenTips) 
                BEGIN
                    INSERT INTO GivenTips 
                        (TipId, UserCNP, Date)
                    VALUES
                        ({foreignKeys[0]}, '1234567890123', '2025-04-01'),
                        ({foreignKeys[1]}, '9876543210987', '2025-03-15'),
                        ({foreignKeys[2]}, '2345678901234', '2025-02-20'),
                        ({foreignKeys[3]}, '3456789012345', '2025-01-10'),
                        ({foreignKeys[4]}, '4567890123456', '2025-05-05');
                END;
            ";
        }
    }
}