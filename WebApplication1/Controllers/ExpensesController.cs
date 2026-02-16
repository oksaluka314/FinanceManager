using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using FinanceManager.Data;   // Ваша папка Data
using FinanceManager.Models; // Ваша папка Models
using FinanceManager.Services; // Ваша папка Services (для алгоритмів)

namespace FinanceManager.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // ЄДИНИЙ МЕТОД INDEX (Пошук + Сортування)
        // =========================================================
        // Додали параметр 'category'
        public async Task<IActionResult> Index(string searchTerm, string sortOrder, string category)
        {
            var expenses = await _context.Expenses.ToListAsync();

            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentCategory = category; // Зберігаємо, щоб знати, що фільтр активний

            // 1. СПОЧАТКУ ФІЛЬТРУЄМО ПО КАТЕГОРІЇ (Якщо вона обрана)
            if (!string.IsNullOrEmpty(category))
            {
                expenses = ExpenseAlgorithms.FilterByCategory(expenses, category);
            }

            // 2. ПОТІМ ПОШУК
            if (!string.IsNullOrEmpty(searchTerm))
            {
                expenses = ExpenseAlgorithms.LinearSearch(expenses, searchTerm);
            }

            // 3. ПОТІМ СОРТУВАННЯ
            if (sortOrder == "amount_asc")
            {
                expenses = ExpenseAlgorithms.MergeSort(expenses);
            }
            else
            {
                // Використовуємо наш Insertion Sort за датою
                expenses = ExpenseAlgorithms.InsertionSortByDate(expenses);
            }

            return View(expenses);
        }
        // =========================================================
        // МЕТОДИ СТВОРЕННЯ (Create)
        // =========================================================

        // GET: /Expenses/Create
        public IActionResult Create()
        {
            PopulateCategories();
            return View();
        }

        // POST: /Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,Date,Amount,Category")] Expense expense)
        {
            // АВТО-КАТЕГОРИЗАЦІЯ (Наша фішка з Хеш-таблицями!)
            // Якщо категорія не обрана, спробуємо визначити її автоматично за описом
            if (string.IsNullOrEmpty(expense.Category))
            {
                var suggested = ExpenseAlgorithms.SuggestCategory(expense.Description);
                if (suggested != null)
                {
                    expense.Category = suggested;
                    // Очищаємо помилки валідації для поля Category, бо ми його заповнили
                    ModelState.Remove("Category");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Якщо помилка - перезавантажуємо список категорій
            PopulateCategories();
            return View(expense);
        }

        // Допоміжний метод для списку категорій
        private void PopulateCategories()
        {
            var categories = new List<string>
            {
                "Продукти", "Транспорт", "Комунальні послуги",
                "Розваги", "Здоров'я", "Одяг", "Ресторани/Кафе", "Інше"
            };

            var listItems = categories.Select(c => new SelectListItem { Text = c, Value = c }).ToList();
            listItems.Insert(0, new SelectListItem { Text = "--- Виберіть категорію ---", Value = "" });

            ViewBag.Categories = listItems;
        }
        // =========================================================
        // ВИДАЛЕННЯ (Delete)
        // =========================================================

        // Цей метод спрацює, коли хтось натисне кнопку "Видалити"
        [HttpPost]
        [ValidateAntiForgeryToken] // Захист від хакерських атак
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Шукаємо запис в базі за його ID
            var expense = await _context.Expenses.FindAsync(id);

            // 2. Якщо знайшли - видаляємо
            if (expense != null)
            {
                _context.Expenses.Remove(expense);

                // 3. Зберігаємо зміни (комітимо в базу)
                await _context.SaveChangesAsync();
            }

            // 4. Оновлюємо сторінку (повертаємось до списку)
            return RedirectToAction(nameof(Index));
        }
    }

}