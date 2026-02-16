using System.ComponentModel.DataAnnotations;

namespace FinanceManager.Models
{
    public class Income
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Вкажіть джерело доходу")]
        [Display(Name = "Джерело")]
        public string Source { get; set; } = string.Empty; // Напр. "Зарплата", "Фріланс"

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }
    }
}