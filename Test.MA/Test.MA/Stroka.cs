using Bogus;

namespace Test.MA
{
    public class Stroka : IStroka
    {
        public DateTime Date { get; set; }
        public string Latin { get; set; }
        public string Rus { get; set; }
        public int Num_Int { get; set; }
        public double Num_Float { get; set; }

        public List<Stroka> Generate(int n)
        {
            var generator = new Faker<Stroka>()
             .StrictMode(true)
             .RuleFor(x => x.Date, f => f.Date.Past(5))
             .RuleFor(x => x.Latin, f => f.Random.String(1))
             .RuleFor(x => x.Rus, f => f.Random.String(1))
             .RuleFor(x => x.Num_Int, f => f.Random.Int(1, 100000000))
             .RuleFor(x => x.Num_Float, f => f.Random.Int(1, 2000000000));

            var gen = new List<Stroka>();

            char[] alphabet = Enumerable.Range('a', 'z' - 'a' + 1)
                                        .Select(i => (char)i)
                                        .Concat(Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (char)i))
                                        .ToArray();
            char[] alphabet2 = Enumerable.Range('а', 'я' - 'а' + 1)
                                        .Select(i => (char)i)
                                        .Concat(Enumerable.Range('А', 'Я' - 'А' + 1).Select(i => (char)i))
                                        .ToArray();

            for (int i = 1; i <= n; i++)
            {
                var stroka = generator.Generate();
                stroka.Latin = new string(Enumerable.Repeat(alphabet, 10)
                                              .Select(s => s[Random.Shared.Next(s.Length)])
                                              .ToArray());
                stroka.Rus = new string(Enumerable.Repeat(alphabet2, 10)
                                              .Select(s => s[Random.Shared.Next(s.Length)])
                                              .ToArray());
                stroka.Num_Float = Math.Round(stroka.Num_Float / 100000000, 8);
                gen.Add(stroka);
            }

            return gen;
        }
    }
}
