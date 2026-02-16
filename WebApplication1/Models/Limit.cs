using System.ComponentModel.DataAnnotations;

namespace FinanceManager.Models
{
    public class Limit
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Категорія")]
        public string Category { get; set; } = string.Empty; // Напр. "Їжа"

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Ліміт суми")]
        public decimal Amount { get; set; }
    }
}