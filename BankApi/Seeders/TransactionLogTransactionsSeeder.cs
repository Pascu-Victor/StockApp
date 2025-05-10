namespace BankApi.Seeders
{
    public class TransactionLogTransactionsSeeder(IConfiguration configuration) : ForeignKeyTableSeeder(configuration)
    {
        protected override string GetReferencedTableName() => "Users";

        protected override string GetQueryWithForeignKeys(List<int> foreignKeys)
        {
            return $@"
                IF NOT EXISTS (SELECT 1 FROM TransactionLogTransactions) 
                BEGIN
                    INSERT INTO TransactionLogTransactions 
                        (Id, StockSymbol, StockName, Type, Amount, PricePerStock, Date)
                    VALUES
                        ({foreignKeys[0]}, 'AAPL', 'Apple Inc.', 'BUY', 10, 150, '2025-04-01'),
                        ({foreignKeys[1]}, 'GOOGL', 'Alphabet Inc.', 'SELL', 5, 2800, '2025-03-15'),
                        ({foreignKeys[2]}, 'TSLA', 'Tesla Inc.', 'BUY', 20, 750, '2025-02-20'),
                        ({foreignKeys[3]}, 'AMZN', 'Amazon.com Inc.', 'SELL', 8, 3400, '2025-01-10'),
                        ({foreignKeys[4]}, 'MSFT', 'Microsoft Corp.', 'BUY', 15, 320, '2025-05-05');
                END;
            ";
        }
    }
}