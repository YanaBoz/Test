using Bogus;

namespace Test.MA
{
    public class Stroka : IStroka 
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public string Latin { get; set; } // латинские символы 
        public string Rus { get; set; } // русские символы
        public int Num_Int { get; set; } //целое число
        public double Num_Float { get; set; } // вещественное число

        // функция генерации случайных значений
        public List<Stroka> Generate(int n) 
        {
            var StrokId = 1;
            var generator = new Faker<Stroka>()
             .StrictMode(true)
             .RuleFor(x => x.ID, _ => StrokId++) //случайное id
             .RuleFor(x => x.Date, f => f.Date.Past(5)) //случайная дата за последние 5 лет
             .RuleFor(x => x.Latin, f => f.Random.String(1)) 
             .RuleFor(x => x.Rus, f => f.Random.String(1))
             .RuleFor(x => x.Num_Int, f => f.Random.Int(1, 100000000)) //случайное целое число 
             .RuleFor(x => x.Num_Float, f => f.Random.Int(1, 2000000000)); // случайное вещественное число 

            var gen = new List<Stroka>(); // инициализация набора

            char[] alphabet = Enumerable.Range('a', 'z' - 'a' + 1)   //набор латинских букв
                                        .Select(i => (char)i)
                                        .Concat(Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char)i))
                                        .ToArray();
            char[] alphabet2 = Enumerable.Range('а', 'я' - 'а' + 1)  //набор русских букв
                                        .Select(i => (char)i)
                                        .Concat(Enumerable.Range('А', 'Я' - 'А' + 1).Select(i => (char)i))
                                        .ToArray();

            for (int i = 1; i <= n; i++)
            {
                var stroka = generator.Generate();
                stroka.Latin = new string(Enumerable.Repeat(alphabet, 10)                  //генерация строки из 10 латинских букв
                                              .Select(s => s[Random.Shared.Next(s.Length)])
                                              .ToArray());
                stroka.Rus = new string(Enumerable.Repeat(alphabet2, 10)                  //генерация строки из 10 русских букв
                                              .Select(s => s[Random.Shared.Next(s.Length)])
                                              .ToArray());
                stroka.Num_Float = Math.Round(stroka.Num_Float / 100000000, 8);   // приведение к формату с 8 цифрами после запятой
                gen.Add(stroka); //добавление объекта в набор
            }

            return gen;
        }
    }
}