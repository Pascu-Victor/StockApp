namespace BankApi.Models
{
    using System;

    /// <summary>
    /// Represents a single transaction entry in the transaction log,
    /// containing details such as stock symbol, type, amount, and author.
    /// </summary>
    public class TransactionLogTransaction
    {
        /// <summary>
        /// Gets the trading symbol of the stock involved in the transaction.
        /// </summary>
        public string StockSymbol { get; }

        /// <summary>
        /// Gets the display name of the stock involved in the transaction.
        /// </summary>
        public string StockName { get; }

        /// <summary>
        /// Gets the type of transaction ("BUY" or "SELL").
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the number of shares involved in the transaction.
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// Gets the price paid per share in the transaction.
        /// </summary>
        public int PricePerStock { get; }

        /// <summary>
        /// Gets the total value of the transaction (Amount multiplied by PricePerStock).
        /// </summary>
        public int TotalValue => this.Amount * this.PricePerStock;

        /// <summary>
        /// Gets the date and time when the transaction took place.
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        /// Gets the CNP identifier of the user who executed the transaction.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogTransaction"/> class.
        /// </summary>
        /// <param name="stockSymbol">The trading symbol of the stock; cannot be null or whitespace.</param>
        /// <param name="stockName">The display name of the stock; cannot be null or whitespace.</param>
        /// <param name="type">The transaction type, must be "BUY" or "SELL".</param>
        /// <param name="amount">The number of shares; must be greater than zero.</param>
        /// <param name="pricePerStock">The price per share; must be greater than zero.</param>
        /// <param name="date">The date and time when the transaction occurred.</param>
        /// <param name="author">The CNP of the user who performed the transaction; cannot be null or whitespace.</param>
        public TransactionLogTransaction(
            string stockSymbol,
            string stockName,
            string type,
            int amount,
            int pricePerStock,
            DateTime date,
            string author)
        {
            // Validate that the stock symbol is provided
            if (string.IsNullOrWhiteSpace(stockSymbol))
            {
                throw new ArgumentException("StockSymbol required");
            }

            // Validate that the stock name is provided
            if (string.IsNullOrWhiteSpace(stockName))
            {
                throw new ArgumentException("StockName required");
            }

            // Validate that the transaction type is either BUY or SELL
            if (type is not ("BUY" or "SELL"))
            {
                throw new ArgumentException("Type must be BUY or SELL");
            }

            // Validate that the amount is positive
            if (amount <= 0)
            {
                throw new ArgumentException("Amount > 0");
            }

            // Validate that the price per stock is positive
            if (pricePerStock <= 0)
            {
                throw new ArgumentException("PricePerStock > 0");
            }

            // Validate that the author identifier is provided
            if (string.IsNullOrWhiteSpace(author))
            {
                throw new ArgumentException("Author required");
            }

            this.StockSymbol = stockSymbol;
            this.StockName = stockName;
            this.Type = type;
            this.Amount = amount;
            this.PricePerStock = pricePerStock;
            this.Date = date;
            this.Author = author;
        }
    }
}
