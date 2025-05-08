namespace StockApp.Models
{
    using System;
    using BankApi.Models.Articles;

    /// <summary>
    /// Represents an application user, with profile details, financial information, and permissions.
    /// </summary>
    public class User
    {

        User(
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
            this.Id = id ?? 0;
            this.CNP = cnp ?? string.Empty;
            this.Username = username ?? string.Empty;
            this.FirstName = firstName ?? string.Empty;
            this.LastName = lastName ?? string.Empty;
            this.Email = email ?? string.Empty;
            this.PhoneNumber = phoneNumber ?? string.Empty;
            this.HashedPassword = hashedPassword ?? string.Empty;
            this.Description = description ?? string.Empty;
            this.IsModerator = isModerator ?? false;
            this.Image = image ?? string.Empty;
            this.IsHidden = isHidden ?? false;
            this.GemBalance = gem_balance ?? 0;
            this.NumberOfOffenses = numberOfOffenses ?? 0;
            this.RiskScore = riskScore ?? 0;
            this.ROI = roi ?? 0;
            this.CreditScore = creditScore ?? 0;
            this.Birthday = birthday ?? new DateOnly();
            this.ZodiacSign = zodiacSign ?? string.Empty;
            this.ZodiacAttribute = zodiacAttribute ?? string.Empty;
            this.NumberOfBillSharesPaid = numberOfBillSharesPaid ?? 0;
            this.Income = income ?? 0;
            this.Balance = balance ?? 0;
        }

        public User()
        {
        }

        /// <summary>
        /// Gets or sets the user's unique identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the user's CNP identifier.
        /// </summary>
        public string CNP { get; set; }

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's phone number.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the user's hashed password.
        /// </summary>
        public string HashedPassword { get; set; }

        /// <summary>
        /// Gets or sets the user's profile description or bio.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this user is a moderator.
        /// </summary>
        public bool IsModerator { get; set; }

        /// <summary>
        /// Gets or sets the path or URL to the user's avatar image.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's profile is hidden.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets the user's gem balance.
        /// </summary>
        public int GemBalance { get; set; }

        /// <summary>
        /// Gets or sets the number of offenses committed by the user.
        /// </summary>
        public int NumberOfOffenses { get; set; }

        /// <summary>
        /// Gets or sets the user's risk score.
        /// </summary>
        public int RiskScore { get; set; }

        /// <summary>
        /// Gets or sets the user's return on investment.
        /// </summary>
        public decimal ROI { get; set; }

        /// <summary>
        /// Gets or sets the user's credit score.
        /// </summary>
        public int CreditScore { get; set; }

        /// <summary>
        /// Gets or sets the user's birthday.
        /// </summary>
        public DateOnly Birthday { get; set; }

        /// <summary>
        /// Gets or sets the user's zodiac sign.
        /// </summary>
        public string ZodiacSign { get; set; }

        /// <summary>
        /// Gets or sets the user's zodiac attribute.
        /// </summary>
        public string ZodiacAttribute { get; set; }

        /// <summary>
        /// Gets or sets the number of bill shares paid by the user.
        /// </summary>
        public int NumberOfBillSharesPaid { get; set; }

        /// <summary>
        /// Gets or sets the user's income.
        /// </summary>
        public int Income { get; set; }

        /// <summary>
        /// Gets or sets the user's account balance.
        /// </summary>
        public decimal Balance { get; set; }

        public List<UserArticle> RelatedArticles = [];
    }
}
