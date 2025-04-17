namespace StockApp.Models
{
    public class Alert
    {
        public int AlertId { get; set; } // Primary Key

        public string StockName { get; set; }

        public string Name { get; set; }

        public decimal UpperBound { get; set; }

        public decimal LowerBound { get; set; }

        public bool ToggleOnOff { get; set; }
    }
}
