namespace StockApp.Models
{
    /// <summary>
    /// Represents the base information for a stock, including its name, symbol, and the author's CNP.
    /// </summary>
    public class BaseStock
    {
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of the stock.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the trading symbol of the stock.
        /// </summary>
        public string Symbol { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the CNP identifier of the author who created this entry.
        /// </summary>
        public string AuthorCNP { get; set; } = string.Empty;

        /// <summary>
        /// Default constructor required by Entity Framework
        /// </summary>
        public BaseStock()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseStock"/> class.
        /// </summary>
        /// <param name="name">The display name of the stock.</param>
        /// <param name="symbol">The trading symbol of the stock.</param>
        /// <param name="authorCnp">The CNP identifier of the author who created this entry.</param>
        public BaseStock(string name, string symbol, string authorCnp)
        {
            Name = name;
            Symbol = symbol;
            AuthorCNP = authorCnp;
        }
    }
}
