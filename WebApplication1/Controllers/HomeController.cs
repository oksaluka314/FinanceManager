using FinanceManager.Models;
using FinanceManager.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using FinanceManager.Services;

namespace FinanceManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // <-- Додаємо доступ до БД

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context; // <-- Ініціалізуємо БД
        }

        public async Task<IActionResult> Index()
        {
            // 1. ВИТРАТИ
            var expenses = await _context.Expenses.ToListAsync();
            decimal totalExpenses = expenses.Sum(e => e.Amount);

            // 2. ДОХОДИ (Нове!)
            var incomes = await _context.Incomes.ToListAsync();
            decimal totalIncomes = incomes.Sum(i => i.Amount);

            // 3. БАЛАНС (Формула)
            decimal balance = totalIncomes - totalExpenses;

            // Передаємо все у View
            ViewBag.TotalExpenses = totalExpenses;
            ViewBag.TotalIncomes = totalIncomes;
            ViewBag.Balance = balance; // <-- Ось наш залишок
            // 1. Загальна сума
            decimal totalSum = expenses.Sum(e => e.Amount);
            ViewBag.TotalExpenses = totalSum;

            // 2. Останні 5 витрат
            ViewBag.RecentExpenses = expenses.OrderByDescending(e => e.Date).Take(5).ToList();

            // 3. Статистика по категоріях
            var categoryStats = new Dictionary<string, decimal>();
            foreach (var item in expenses)
            {
                string cat = string.IsNullOrEmpty(item.Category) ? "Інше" : item.Category;
                if (categoryStats.ContainsKey(cat)) categoryStats[cat] += item.Amount;
                else categoryStats.Add(cat, item.Amount);
            }
            ViewBag.CategorySummary = categoryStats;

            // --- НОВЕ: АЛГОРИТМ ЧАСТОТНОГО АНАЛІЗУ ---
            // Використовуємо наш сервіс, щоб знайти найпопулярніші слова (магазини)
            var topPlaces = ExpenseAlgorithms.GetTopLocations(expenses);
            ViewBag.TopPlaces = topPlaces;
            // -----------------------------------------
            // ... (попередній код: доходи, витрати, баланс) ...

            // --- ПЕРЕВІРКА ЛІМІТІВ (НОВЕ) ---
            var limits = await _context.Limits.ToListAsync();

            // Список попереджень (будемо передавати на екран)
            var warnings = new List<string>();

            // Перетворюємо ліміти в словник для швидкості: Категорія -> Ліміт
            // Це ще один приклад використання Dictionary (Хеш-таблиці)
            var limitsDict = limits.ToDictionary(l => l.Category, l => l.Amount);

            // Проходимо по фактичних витратах (які ми вже порахували раніше для діаграми)
            // categoryStats - це Dictionary<string, decimal> з сумами витрат
            foreach (var categoryStat in categoryStats)
            {
                string catName = categoryStat.Key;
                decimal spentAmount = categoryStat.Value;

                // Перевіряємо, чи є ліміт для цієї категорії
                if (limitsDict.ContainsKey(catName))
                {
                    decimal limitAmount = limitsDict[catName];

                    // Якщо витратили більше ліміту
                    if (spentAmount > limitAmount)
                    {
                        decimal overspent = spentAmount - limitAmount;
                        // Формуємо повідомлення
                        string msg = $" Увага! Ви перевищили ліміт на '{catName}' на {overspent:C0}. " +
                                     $"(Ліміт: {limitAmount:C0}, Витрачено: {spentAmount:C0}). " +
                                     $"Рекомендуємо обмежити витрати в цій категорії до кінця місяця.";
                        warnings.Add(msg);
                    }
                }
            }

            ViewBag.LimitWarnings = warnings;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}