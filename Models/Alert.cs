using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace StockApp.Models
{
    /// <summary>
    /// Represents a stock alert with upper and lower price bounds and an on/off toggle.
    /// </summary>
    public class Alert : INotifyPropertyChanged
    {
        private int alertId;
        private string stockName = string.Empty;
        private string name = string.Empty;
        private decimal upperBound;
        private decimal lowerBound;
        private bool toggleOnOff;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the unique identifier for this alert.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AlertId
        {
            get => this.alertId;
            set
            {
                if (this.alertId != value)
                {
                    this.alertId = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the stock symbol or name that this alert monitors.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string StockName
        {
            get => this.stockName;
            set
            {
                if (this.stockName != value)
                {
                    this.stockName = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the user-defined name for this alert.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name
        {
            get => this.name;
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the upper price boundary for triggering the alert.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UpperBound
        {
            get => this.upperBound;
            set
            {
                if (this.upperBound != value)
                {
                    this.upperBound = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the lower price boundary for triggering the alert.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LowerBound
        {
            get => this.lowerBound;
            set
            {
                if (this.lowerBound != value)
                {
                    this.lowerBound = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the alert is active.
        /// </summary>
        [Required]
        public bool ToggleOnOff
        {
            get => this.toggleOnOff;
            set
            {
                if (this.toggleOnOff != value)
                {
                    this.toggleOnOff = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            // Notify any listeners that the given property has changed.
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
