using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FinanceManager.Controllers
{
    public class TestingController : Controller
    {
        // Відображення сторінки тестування
        public IActionResult Index()
        {
            return View();
        }

        // Метод запуску тестів (викликається кнопкою з сайту)
        [HttpPost]
        public IActionResult RunBenchmark(int count = 5000)
        {
            var results = new List<BenchmarkResult>();

            // ---------------------------------------------------------
            // ЕТАП 0: ГЕНЕРАЦІЯ ДАНИХ
            // ---------------------------------------------------------
            // Генеруємо "Майже відсортований" список (99% в порядку + 1 новий в кінці).
            // Це ідеальний сценарій для перевірки роботи програми в реальному житті.
            var data = GenerateAlmostSortedData(count);


            // ---------------------------------------------------------
            // БЛОК 1: СОРТУВАННЯ
            // Порівнюємо: Ваш Insertion Sort проти Стандартного .OrderBy()
            // ---------------------------------------------------------

            // 1.1 Ваш алгоритм (Insertion Sort)
            var listForInsert = new List<Expense>(data); // Робимо копію
            var stopwatch = Stopwatch.StartNew();
            InsertionSort(listForInsert);
            stopwatch.Stop();

            results.Add(new BenchmarkResult
            {
                Group = "Сортування",
                AlgorithmName = "Insertion Sort (Вставкою)",
                TimeTaken = stopwatch.Elapsed.TotalMilliseconds,
                ItemCount = count,
                Description = "Використано практичний кейс відсортованого масива. Складність O(N)"
            });

            // 1.2 Стандартний алгоритм (.OrderBy)
            var listForStandard = new List<Expense>(data); // Робимо копію
            stopwatch.Restart();
            var sorted = listForStandard.OrderByDescending(x => x.Date).ToList();
            stopwatch.Stop();

            results.Add(new BenchmarkResult
            {
                Group = "Сортування",
                AlgorithmName = "QuickSort(Швидке)",
                TimeTaken = stopwatch.Elapsed.TotalMilliseconds,
                ItemCount = count,
                Description = "Такий же практичний кейс відсортованого масива. Складність O(N log N)"
            });


            // ---------------------------------------------------------
            // БЛОК 2: ПОШУК
            // Порівнюємо: Ваш Лінійний пошук проти Бінарного дерева
            // ---------------------------------------------------------
            string searchTerm = "СловоЯкогоНемаєXY"; // Найгірший випадок (проходимо все)

            // 2.1 Ваш алгоритм (Лінійний пошук)
            stopwatch.Restart();
            LinearSearch(data, searchTerm);
            stopwatch.Stop();

            results.Add(new BenchmarkResult
            {
                Group = "Пошук",
                AlgorithmName = "Linear Search (Лінійний)",
                TimeTaken = stopwatch.Elapsed.TotalMilliseconds,
                ItemCount = count,
                Description = "Шукаємо неіснуючий елемент. Послідовний перебір O(N). Дозволяє пошук підрядка."
            });

            // 2.2 Бінарне дерево пошуку (BST)
            // Спочатку будуємо дерево (перемішуємо дані, щоб дерево було гарним)
            stopwatch.Restart(); // <--- ЗАПУСКАЄМО ТАЙМЕР ТУТ

            // Етап 1: Побудова дерева (Це займе час!)
            // Перемішуємо, щоб дерево не стало "сосискою"
            var randomDataForTree = data.OrderBy(x => Guid.NewGuid()).ToList();
            var bst = new ExpenseBST();
            foreach (var item in randomDataForTree)
            {
                bst.Insert(item);
            }

            // Етап 2: Сам пошук
            var foundNode = bst.Search(searchTerm);

            stopwatch.Stop(); // <--- ЗУПИНЯЄМО ТАЙМЕР

            results.Add(new BenchmarkResult
            {
                Group = "Пошук",
                AlgorithmName = "BST (Побудова дерева + Пошук)",
                TimeTaken = stopwatch.Elapsed.TotalMilliseconds,
                ItemCount = count,
                Description = "Час створення структури + час пошуку неіснуючого елемента. O(N log N) + O(log N)"
            });

            // ---------------------------------------------------------
            // БЛОК 3: АГРЕГАЦІЯ (СТАТИСТИКА)
            // Порівнюємо: Ваші Хеш-таблиці проти Вкладених циклів
            // ---------------------------------------------------------

            // 3.1 Повільний метод (Вкладені цикли)
            stopwatch.Restart();
            NaiveAggregation(data);
            stopwatch.Stop();

            results.Add(new BenchmarkResult
            {
                Group = "Статистика",
                AlgorithmName = "Вкладені цикли",
                TimeTaken = stopwatch.Elapsed.TotalMilliseconds,
                ItemCount = count,
                Description = "Послідовний перебір на кількість категорій O(N*M). Перебір всього списку для кожної категорії."
            });

            // 3.2 Ваш алгоритм (Dictionary)
            stopwatch.Restart();
            DictionaryAggregation(data);
            stopwatch.Stop();

            results.Add(new BenchmarkResult
            {
                Group = "Статистика",
                AlgorithmName = "Dictionary (Хеш-таблиця)",
                TimeTaken = stopwatch.Elapsed.TotalMilliseconds,
                ItemCount = count,
                Description = "Послідовний перебір O(N). Одноразовий прохід по списку."
            });

            return Json(results);
        }


        // =========================================================
        // РЕАЛІЗАЦІЯ АЛГОРИТМІВ
        // =========================================================

        // 1. Insertion Sort (Вставка)
        private void InsertionSort(List<Expense> list)
        {
            for (int i = 1; i < list.Count; i++)
            {
                var key = list[i];
                int j = i - 1;
                while (j >= 0 && list[j].Date < key.Date)
                {
                    list[j + 1] = list[j];
                    j--;
                }
                list[j + 1] = key;
            }
        }

        // 2. Linear Search (Лінійний)
        private List<Expense> LinearSearch(List<Expense> list, string term)
        {
            var result = new List<Expense>();
            term = term.ToLower();
            foreach (var item in list)
            {
                if (item.Description.ToLower().Contains(term)) result.Add(item);
            }
            return result;
        }

        // 3. Dictionary Aggregation (Хеш-таблиця)
        private void DictionaryAggregation(List<Expense> list)
        {
            var stats = new Dictionary<string, decimal>();
            foreach (var item in list)
            {
                if (stats.ContainsKey(item.Category)) stats[item.Category] += item.Amount;
                else stats.Add(item.Category, item.Amount);
            }
        }

        // 4. Naive Aggregation (Повільні цикли)
        private void NaiveAggregation(List<Expense> list)
        {
            var uniqueCategories = list.Select(x => x.Category).Distinct().ToList();
            foreach (var cat in uniqueCategories)
            {
                decimal sum = 0;
                foreach (var item in list)
                {
                    if (item.Category == cat) sum += item.Amount;
                }
            }
        }

        // Генератор даних (Майже відсортованих)
        private List<Expense> GenerateAlmostSortedData(int count)
        {
            var list = new List<Expense>();
            var rand = new Random();
            var categories = new[] { "Їжа", "Транспорт", "Розваги", "Комуналка", "Освіта", "Здоров'я" };

            // 99.9% даних вже відсортовані (старі дати)
            for (int i = 1; i < count; i++)
            {
                list.Add(new Expense
                {
                    Date = DateTime.Now.AddDays(-i),
                    Amount = rand.Next(10, 2000),
                    Category = categories[rand.Next(categories.Length)],
                    Description = "Expense #" + i
                });
            }
            // Останній елемент - "Сьогодні", стоїть в кінці (порушує порядок)
            list.Add(new Expense
            {
                Date = DateTime.Now,
                Amount = 500,
                Category = "Тест",
                Description = "New Inserted Item"
            });
            return list;
        }

        // =========================================================
        // ДОПОМІЖНІ КЛАСИ (Моделі)
        // =========================================================

        public class BenchmarkResult
        {
            public string Group { get; set; }
            public string AlgorithmName { get; set; }
            public double TimeTaken { get; set; }
            public int ItemCount { get; set; }
            public string Description { get; set; }
        }

        // Спрощена модель Expense (видаліть, якщо використовуєте свою з Models)
        public class Expense
        {
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
            public string Category { get; set; }
            public string Description { get; set; }
        }

        // --- Класи для Бінарного Дерева ---
        public class TreeNode
        {
            public Expense Data;
            public TreeNode Left, Right;
            public TreeNode(Expense data) { Data = data; }
        }

        public class ExpenseBST
        {
            public TreeNode Root;
            public void Insert(Expense data) { Root = InsertRec(Root, data); }

            private TreeNode InsertRec(TreeNode root, Expense data)
            {
                if (root == null) return new TreeNode(data);
                if (string.Compare(data.Description, root.Data.Description) < 0)
                    root.Left = InsertRec(root.Left, data);
                else
                    root.Right = InsertRec(root.Right, data);
                return root;
            }

            public Expense Search(string desc) { return SearchRec(Root, desc); }

            private Expense SearchRec(TreeNode root, string desc)
            {
                if (root == null || root.Data.Description == desc) return root?.Data;
                if (string.Compare(desc, root.Data.Description) < 0) return SearchRec(root.Left, desc);
                return SearchRec(root.Right, desc);
            }
        }
    }
}