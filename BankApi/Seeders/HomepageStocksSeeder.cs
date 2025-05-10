namespace BankApi.Seeders
{
    public class HomepageStocksSeeder(IConfiguration configuration) : ForeignKeyTableSeeder(configuration)
    {
        protected override string GetReferencedTableName() => "BaseStocks";

        protected override string GetQueryWithForeignKeys(List<int> foreignKeys)
        {
            return $@"
                IF NOT EXISTS (SELECT 1 FROM HomepageStocks) 
                BEGIN
                    INSERT INTO HomepageStocks 
                        (Id, Symbol, Change)
                    VALUES
                        ({foreignKeys[0]}, 'AAPL', 1.25),
                        ({foreignKeys[1]}, 'GOOGL', -0.75),
                        ({foreignKeys[2]}, 'TSLA', 2.30),
                        ({foreignKeys[3]}, 'AMZN', -1.10),
                        ({foreignKeys[4]}, 'MSFT', 0.50);
                END;
            ";
        }
    }
}