namespace BankApi.Models
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a gem deal with a specified price, gem amount, and optional special duration.
    /// </summary>
    /// <param name="title">The title of the gem deal.</param>
    /// <param name="gemAmount">The number of gems included in this deal.</param>
    /// <param name="price">The price of the deal.</param>
    /// <param name="isSpecial">Whether this deal is a special, time‑limited offer.</param>
    /// <param name="durationMinutes">
    /// The duration, in minutes, for which a special deal remains active; null for non‑timed deals.
    /// </param>
    public class GemDeal(string title, int gemAmount, double price, bool isSpecial = false, int? durationMinutes = null)
    {
        /// <summary>
        /// Occurs when one of this object's properties changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the title of the gem deal.
        /// </summary>
        public string Title { get; } = title;

        /// <summary>
        /// Gets the number of gems included in this deal.
        /// </summary>
        public int GemAmount { get; } = gemAmount;

        /// <summary>
        /// Gets the price of the deal.
        /// </summary>
        public double Price { get; } = price;

        /// <summary>
        /// Gets a value indicating whether this is a special, time‑limited deal.
        /// </summary>
        public bool IsSpecial { get; } = isSpecial;

        /// <summary>
        /// Gets the optional duration (in minutes) for which a special deal remains active.
        /// </summary>
        public int? DurationMinutes { get; } = durationMinutes;

        /// <summary>
        /// Gets the UTC time when this deal expires, or <see cref="DateTime.MaxValue"/> if it never expires.
        /// </summary>
        public DateTime ExpirationTime

            // If special and has a duration, calculate expiration; else never expires
            => this.IsSpecial && this.DurationMinutes.HasValue
                   ? DateTime.UtcNow.AddMinutes(this.DurationMinutes.Value)
                   : DateTime.MaxValue;

        /// <summary>
        /// Gets a value indicating whether this deal is still available.
        /// </summary>
        public bool IsAvailable

            // Available if not special, no duration set, or the current time is before expiration
            => !this.IsSpecial
               || this.DurationMinutes is null
               || DateTime.UtcNow <= this.ExpirationTime;

        /// <summary>
        /// Gets the price formatted with two decimals and the euro symbol.
        /// </summary>
        public string FormattedPrice

            // Format price as "0.00€"
            => $"{this.Price:0.00}€";

        /// <summary>
        /// Gets the expiration time formatted as "HH:mm:ss", or an empty string if it never expires.
        /// </summary>
        public string ExpirationTimeFormatted

            // Return empty when expiration is infinite, otherwise format the time
            => this.ExpirationTime == DateTime.MaxValue
                   ? string.Empty
                   : this.ExpirationTime.ToString("HH:mm:ss");

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="prop">
        /// The name of the property that changed. Automatically provided when called without arguments.
        /// </param>
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)

            // Notify subscribers that a property value has changed
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
