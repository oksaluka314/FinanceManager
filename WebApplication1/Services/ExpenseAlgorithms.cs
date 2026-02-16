using FinanceManager.Models;
using System.Collections.Generic;
using System.Linq;

namespace FinanceManager.Services
{
    public static class ExpenseAlgorithms
    {
        // ==========================================
        // 1. ПОШУК (Linear Search)
        // ==========================================
        // Шукає текст (підрядок) ігноруючи регістр
        public static List<Expense> LinearSearch(List<Expense> data, string searchTerm)
        {
            var result = new List<Expense>();

            // Нормалізуємо запит (малі літери)
            var term = searchTerm.ToLower().Trim();

            // Проходимо по кожному елементу (O(n))
            foreach (var item in data)
            {
                // Захист від null (якщо опис порожній)
                if (string.IsNullOrEmpty(item.Description)) continue;

                // Перевірка входження
                if (item.Description.ToLower().Contains(term))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        // ==========================================
        // 2. СОРТУВАННЯ (Merge Sort)
        // ==========================================
        // Сортує за сумою (стабільно)
        public static List<Expense> MergeSort(List<Expense> list)
        {
            if (list.Count <= 1) return list;

            var mid = list.Count / 2;
            var left = list.Take(mid).ToList();
            var right = list.Skip(mid).ToList();

            return Merge(MergeSort(left), MergeSort(right));
        }

        private static List<Expense> Merge(List<Expense> left, List<Expense> right)
        {
            var result = new List<Expense>();
            int i = 0, j = 0;

            while (i < left.Count && j < right.Count)
            {
                // Порівнюємо суми
                if (left[i].Amount < right[j].Amount)
                {
                    result.Add(left[i]);
                    i++;
                }
                else
                {
                    result.Add(right[j]);
                    j++;
                }
            }
            result.AddRange(left.Skip(i));
            result.AddRange(right.Skip(j));
            return result;
        }

        // ==========================================
        // 3. АНАЛІТИКА ТОП МІСЦЬ (Hash Table + HashSet)
        // ==========================================
        public static Dictionary<string, int> GetTopLocations(List<Expense> expenses)
        {
            var locationStats = new Dictionary<string, int>();

            // 1. Словник фраз, які ми хочемо вважати одним цілим
            // Ключ: як пишуть люди (з пробілом), Значення: тимчасова заміна (без пробілу)
            var knownBrands = new Dictionary<string, string>
    {
        { "katty soho", "katty_soho" },
        { "burger king", "burger_king" },
        { "new yorker", "new_yorker" },
        { "пузата хата", "пузата_хата" },
        { "нова пошта", "нова_пошта" }
    };

            var stopWords = new HashSet<string>
    {
        "купив", "оплата", "за", "в", "на", "та", "і", "для",
        "магазин", "чек", "переказ", "картка", "готівка", "сума", "грн"
    };

            foreach (var expense in expenses)
            {
                if (string.IsNullOrEmpty(expense.Description)) continue;

                // КРОК A: Нормалізуємо весь опис до малих літер
                string processedDesc = expense.Description.ToLower();

                // КРОК B: Об'єднуємо відомі бренди в одне "слово"
                // Замінюємо "katty soho" на "katty_soho"
                foreach (var brand in knownBrands)
                {
                    if (processedDesc.Contains(brand.Key))
                    {
                        processedDesc = processedDesc.Replace(brand.Key, brand.Value);
                    }
                }

                // КРОК C: Тепер розбиваємо по пробілах. 
                // "katty_soho" тепер залишиться цілим!
                var words = processedDesc.Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in words)
                {
                    var cleanWord = word; // Вже lower case

                    if (!stopWords.Contains(cleanWord) && cleanWord.Length > 2)
                    {
                        // КРОК D: Якщо це наш бренд, повертаємо пробіл назад для краси
                        if (cleanWord.Contains("_"))
                        {
                            cleanWord = cleanWord.Replace("_", " ");
                            // Робимо перші букви великими для краси (Katty Soho)
                            cleanWord = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cleanWord);
                        }

                        if (locationStats.ContainsKey(cleanWord))
                            locationStats[cleanWord]++;
                        else
                            locationStats[cleanWord] = 1;
                    }
                }
            }

            return locationStats
                .OrderByDescending(x => x.Value)
                .Take(5)
                .ToDictionary(x => x.Key, x => x.Value);
        }
        // ==========================================
        // 4. АВТО-КАТЕГОРІЗАЦІЯ (Hash Table Lookup)
        // ==========================================
        // Повертає назву категорії або null, якщо не знайдено
        public static string? SuggestCategory(string description)
        {
            if (string.IsNullOrEmpty(description)) return null;

            // Наш "Розумний словник" маркерів
            var keywordMap = new Dictionary<string, string>
            {
                // Транспорт
                { "uber", "Транспорт" }, { "bolt", "Транспорт" }, { "uklon", "Транспорт" },
                { "wog", "Транспорт" }, { "okko", "Транспорт" }, { "бензин", "Транспорт" }, { "метро", "Транспорт" },
                
                // Продукти
                { "сільпо", "Продукти" }, { "атб", "Продукти" }, { "novus", "Продукти" },
                { "ашан", "Продукти" }, { "фора", "Продукти" }, { "хліб", "Продукти" }, { "молоко", "Продукти" },
                
                // Здоров'я
                { "аптека", "Здоров'я" }, { "ліки", "Здоров'я" }, { "лікар", "Здоров'я" }, { "аналізи", "Здоров'я" },
                
                // Кафе
                { "mcdonalds", "Ресторани/Кафе" }, { "kfc", "Ресторани/Кафе" }, { "кава", "Ресторани/Кафе" },
                { "латте", "Ресторани/Кафе" }, { "піца", "Ресторани/Кафе" },
                
                // Розваги
                { "netflix", "Розваги" }, { "steam", "Розваги" }, { "кіно", "Розваги" }, { "youtube", "Розваги" }
            };

            var words = description.ToLower().Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                // Миттєвий пошук O(1)
                if (keywordMap.ContainsKey(word))
                {
                    return keywordMap[word];
                }
            }

            return null; // Нічого не знайшли
        }
        // ==========================================
        // 5. СОРТУВАННЯ ВСТАВКОЮ (Insertion Sort)
        // ==========================================
        // Сортує за ДАТОЮ (від нових до старих)
        // Ідеально підходить, коли список майже відсортований
        public static List<Expense> InsertionSortByDate(List<Expense> list)
        {
            for (int i = 1; i < list.Count; i++)
            {
                var key = list[i];
                int j = i - 1;

                // Порівнюємо дати. 
                // Використовуємо '<', щоб новіші дати (більші) були на початку (Descending)
                while (j >= 0 && list[j].Date < key.Date)
                {
                    list[j + 1] = list[j];
                    j--;
                }
                list[j + 1] = key;
            }
            return list;
        }
        // ==========================================
        // 6. ФІЛЬТРАЦІЯ ПО КАТЕГОРІЇ
        // ==========================================
        // Вибирає тільки ті записи, що відповідають категорії
        public static List<Expense> FilterByCategory(List<Expense> list, string category)
        {
            var result = new List<Expense>();

            foreach (var item in list)
            {
                // Порівнюємо рядки. Якщо збігаються - додаємо в результат.
                if (item.Category == category)
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }
}