namespace BankApi.Seeders
{
    public class TipsSeeder(IConfiguration configuration) : RegularTableSeeder(configuration)
    {
        protected override string GetQuery() => @"
            IF NOT EXISTS (SELECT 1 FROM Tips) 
            BEGIN
                INSERT INTO Tips 
                    (CreditScoreBracket, Type, TipText)
                VALUES
                    ('300-579', 'Credit Improvement', 'Consider paying down outstanding debt to improve your credit score.'),
                    ('580-669', 'Responsible Borrowing', 'Maintain timely payments and avoid maxing out credit cards.'),
                    ('670-739', 'Financial Growth', 'You qualify for better loan rates. Consider investing wisely.'),
                    ('740-799', 'Optimized Credit Usage', 'Your credit score is strong. Keep it that way by diversifying credit types.'),
                    ('800-850', 'Elite Financial Strategy', 'Your score is excellent! Utilize rewards programs and negotiate better rates.');
            END;
        ";
    }
}