using Bogus;
using Test.MA.Interfaces;
using Test.MA.Models;

namespace Test.MA.Generators
{
    // Генератор тестовых данных типа Stroka с использованием Bogus
    public class StrokaGenerator : IStrokaGenerator
    {
        // Создание латинского алфавита: 'a'..'z' + 'A'..'Z'
        private static readonly string LatinAlphabet =
            string.Concat(Enumerable.Range('a', 26).Select(c => (char)c))
            + string.Concat(Enumerable.Range('A', 26).Select(c => (char)c));

        // Попытка создать русский алфавит через диапазон символов.
        // Замечание: русский алфавит содержит букву 'ё'/'Ё', и количество букв в Unicode-последовательности
        // не всегда совпадает с ожидаемым — явное перечисление букв надёжнее.
        private static readonly string RussianAlphabet =
            string.Concat(Enumerable.Range('а', 32).Select(c => (char)c))
            + string.Concat(Enumerable.Range('А', 32).Select(c => (char)c));

        // Генерация списка Stroka заданного размера
        public List<Stroka> Generate(int count)
        {
            var faker = new Faker<Stroka>()
                // Дата — случайная в прошлые 5 лет
                .RuleFor(x => x.Date, f => f.Date.Past(5))
                // Latin и Rus — случайные строки из соответствующих алфавитов
                .RuleFor(x => x.Latin, f => GenerateRandomString(f, LatinAlphabet))
                .RuleFor(x => x.Rus, f => GenerateRandomString(f, RussianAlphabet))
                // Целое число в диапазоне
                .RuleFor(x => x.Num_Int, f => f.Random.Int(1, 100_000_000))
                // Вещественное число с округлением до 8 знаков
                .RuleFor(x => x.Num_Float, f => Math.Round(f.Random.Double(1, 20), 8));

            return faker.Generate(count);
        }

        // Вспомогательный метод: случайная строка фиксированной длины из переданного алфавита.
        // Bogus.Random.String2 умеет генерировать строки из заданного набора символов.
        private static string GenerateRandomString(Faker f, string alphabet, int length = 10)
        {
            return f.Random.String2(length, alphabet);
        }
    }
}
