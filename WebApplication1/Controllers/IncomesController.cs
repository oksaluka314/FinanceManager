using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceManager.Data;
using FinanceManager.Models;

namespace FinanceManager.Controllers
{
    public class IncomesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncomesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Список доходів
        public async Task<IActionResult> Index()
        {
            return View(await _context.Incomes.OrderByDescending(i => i.Date).ToListAsync());
        }

        // Сторінка створення (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Збереження (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Source,Date,Amount")] Income income)
        {
            if (ModelState.IsValid)
            {
                _context.Add(income);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(income);
        }

        // Видалення
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var income = await _context.Incomes.FindAsync(id);
            if (income != null)
            {
                _context.Incomes.Remove(income);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}