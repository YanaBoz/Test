namespace Test.MA.Models
{
    // Модель одной строки данных (Stroka)
    public class Stroka
    {
        // Первичный ключ (автоинкремент в базе)
        public int ID { get; set; }

        // Дата записи
        public DateTime Date { get; set; }

        // Поле с латинскими символами (ограничение длины задаётся в месте записи в БД)
        public string Latin { get; set; } = string.Empty;

        // Поле с русскими символами
        public string Rus { get; set; } = string.Empty;

        // Целое число — в коде int; в SQL-таблице у вас сейчас BIGINT (несоответствие — смотреть примечание)
        public int Num_Int { get; set; }

        // Вещественное число
        public double Num_Float { get; set; }

        // Сериализация строки в формат "yyyy-MM-dd||Latin||Rus||Num_Int||Num_Float"
        // Замечание: формат даты и число форматируются согласно текущей культуре среды.
        // При межмашинном обмене лучше использовать CultureInfo.InvariantCulture.
        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd}||{Latin}||{Rus}||{Num_Int}||{Num_Float:F8}";
        }

        // Парсинг строки в объект Stroka (ожидается ровно 5 частей, разделитель "||")
        // Бросает FormatException, если формат неверный или парсинг неудачен.
        public static Stroka Parse(string line)
        {
            var parts = line.Split("||");
            if (parts.Length != 5)
                throw new FormatException("Invalid line format");

            // Прямой парсинг: DateTime.Parse / int.Parse / double.Parse.
            // В реальной среде рекомендую использовать TryParse с CultureInfo.InvariantCulture
            // чтобы избежать ошибок при разных локалях (например запятая/точка в дробях).
            return new Stroka
            {
                Date = DateTime.Parse(parts[0]),
                Latin = parts[1],
                Rus = parts[2],
                Num_Int = int.Parse(parts[3]),
                Num_Float = double.Parse(parts[4])
            };
        }
    }
}
