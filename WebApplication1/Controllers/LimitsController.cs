using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceManager.Data;
using FinanceManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceManager.Controllers
{
    public class LimitsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LimitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Limits
        public async Task<IActionResult> Index()
        {
            return View(await _context.Limits.ToListAsync());
        }

        // GET: Limits/Create
        public IActionResult Create()
        {
            // Передаємо список категорій, як у витратах
            var categories = new List<string> { "Продукти", "Транспорт", "Комунальні послуги", "Розваги", "Здоров'я", "Одяг", "Ресторани/Кафе", "Інше" };
            ViewBag.Categories = new SelectList(categories);
            return View();
        }

        // POST: Limits/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Category,Amount")] Limit limit)
        {
            // Перевіряємо, чи немає вже ліміту на цю категорію
            var existing = await _context.Limits.FirstOrDefaultAsync(l => l.Category == limit.Category);
            if (existing != null)
            {
                // Якщо є - оновлюємо старий
                existing.Amount = limit.Amount;
                _context.Update(existing);
            }
            else
            {
                // Якщо немає - створюємо новий
                _context.Add(limit);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var limit = await _context.Limits.FindAsync(id);
            if (limit != null)
            {
                _context.Limits.Remove(limit);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}