namespace BankApi.Models
{
    /// <summary>
    /// Represents a user’s stock holding, including its base information and quantity.
    /// </summary>
    /// <param name="name">The display name of the stock.</param>
    /// <param name="symbol">The trading symbol of the stock.</param>
    /// <param name="authorCnp">The CNP identifier of the author who created this entry.</param>
    /// <param name="quantity">The number of shares held by the user.</param>
    public class UserStock
        : BaseStock
    {
        public UserStock(string name, string symbol, string authorCnp, int quantity)
        {
            base.Name = name;
            base.Symbol = symbol;
            base.AuthorCNP = authorCnp;
            this.Quantity = quantity;
        }

        /// <summary>
        /// Gets or sets the number of shares the user holds.
        /// </summary>
        public int Quantity { get; set; }
    }
}
