using FinanceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Data // Правильний namespace
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Limit> Limits { get; set; }
        public DbSet<Income> Incomes { get; set; }
    }
}