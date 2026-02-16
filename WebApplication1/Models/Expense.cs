using System.ComponentModel.DataAnnotations;

namespace FinanceManager.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        // ДОДАЄМО = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Будь ласка, виберіть категорію")]
        // ДОДАЄМО = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}