namespace BankApi.Models
{
    using System;

    /// <summary>
    /// Represents an application user, with profile details, financial information, and permissions.
    /// </summary>
    public class User(
        int? id = null,
        string? cnp = null,
        string? username = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        string? phoneNumber = null,
        string? hashedPassword = null,
        string? description = null,
        bool? isModerator = null,
        string? image = null,
        bool? isHidden = null,
        int? gem_balance = null,
        int? numberOfOffenses = null,
        int? riskScore = null,
        decimal? roi = null,
        int? creditScore = null,
        DateOnly? birthday = null,
        string? zodiacSign = null,
        string? zodiacAttribute = null,
        int? numberOfBillSharesPaid = null,
        int? income = null,
        decimal? balance = null)
    {
        /// <summary>
        /// Gets or sets the user's unique identifier.
        /// </summary>
        public int Id { get; set; } = id ?? 0;

        /// <summary>
        /// Gets or sets the user's CNP identifier.
        /// </summary>
        public string CNP { get; set; } = cnp ?? string.Empty;

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        public string Username { get; set; } = username ?? string.Empty;

        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        public string FirstName { get; set; } = firstName ?? string.Empty;

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        public string LastName { get; set; } = lastName ?? string.Empty;

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string Email { get; set; } = email ?? string.Empty;

        /// <summary>
        /// Gets or sets the user's phone number.
        /// </summary>
        public string PhoneNumber { get; set; } = phoneNumber ?? string.Empty;

        /// <summary>
        /// Gets or sets the user's hashed password.
        /// </summary>
        public string HashedPassword { get; set; } = hashedPassword ?? string.Empty;

        /// <summary>
        /// Gets or sets the user's profile description or bio.
        /// </summary>
        public string Description { get; set; } = description ?? string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this user is a moderator.
        /// </summary>
        public bool IsModerator { get; set; } = isModerator ?? false;

        /// <summary>
        /// Gets or sets the path or URL to the user's avatar image.
        /// </summary>
        public string Image { get; set; } = image ?? string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user's profile is hidden.
        /// </summary>
        public bool IsHidden { get; set; } = isHidden ?? false;

        /// <summary>
        /// Gets or sets the user's gem balance.
        /// </summary>
        public int GemBalance { get; set; } = gem_balance ?? 0;

        /// <summary>
        /// Gets or sets the number of offenses committed by the user.
        /// </summary>
        public int NumberOfOffenses { get; set; } = numberOfOffenses ?? 0;

        /// <summary>
        /// Gets or sets the user's risk score.
        /// </summary>
        public int RiskScore { get; set; } = riskScore ?? 0;

        /// <summary>
        /// Gets or sets the user's return on investment.
        /// </summary>
        public decimal ROI { get; set; } = roi ?? 0;

        /// <summary>
        /// Gets or sets the user's credit score.
        /// </summary>
        public int CreditScore { get; set; } = creditScore ?? 0;

        /// <summary>
        /// Gets or sets the user's birthday.
        /// </summary>
        public DateOnly Birthday { get; set; } = birthday ?? new DateOnly();

        /// <summary>
        /// Gets or sets the user's zodiac sign.
        /// </summary>
        public string ZodiacSign { get; set; } = zodiacSign ?? string.Empty;

        /// <summary>
        /// Gets or sets the user's zodiac attribute.
        /// </summary>
        public string ZodiacAttribute { get; set; } = zodiacAttribute ?? string.Empty;

        /// <summary>
        /// Gets or sets the number of bill shares paid by the user.
        /// </summary>
        public int NumberOfBillSharesPaid { get; set; } = numberOfBillSharesPaid ?? 0;

        /// <summary>
        /// Gets or sets the user's income.
        /// </summary>
        public int Income { get; set; } = income ?? 0;

        /// <summary>
        /// Gets or sets the user's account balance.
        /// </summary>
        public decimal Balance { get; set; } = balance ?? 0;
    }
}
