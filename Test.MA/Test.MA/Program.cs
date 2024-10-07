using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Test.MA;

class Program
{
    static int Main(string[] args)
    {
        int n = 100; // кол-во файлов
        int m = 100000;  // кол-во строк
        string[] files = new string[n]; // массив для хранения пути к файлам 
        // подключение к бд
        SqlConnection connection = new(ConfigurationManager.ConnectionStrings["TestDB"].ConnectionString); 

        Stroka str = new(); // хранение 
        var gena = new List<Stroka>(); // набор объектов 

        try
        {
            for (int o = 1; o <= n; o++)
            {
                // создание нового файла для записи
                StreamWriter sw = new(@"V:\\filename" + o + ".txt"); 
                files[o - 1] = "V:\\filename" + o + ".txt"; // хранение пути 

                gena = str.Generate(m); //генерация случайных данных

                for (int i = 0; i < gena.Count; i++) // запись данных в файл
                {
                    sw.WriteLine($"{gena[i].Date:d}||{gena[i].Latin}||{gena[i].Rus}||{gena[i].Num_Int}||{gena[i].Num_Float}");
                }
                sw.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        // доступ к бд
        try
        {
            connection.Open(); 
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
        Men(files, connection); // функция навигации
        return 0;
    }
    // функция навигации
    static void Men(string[] files, SqlConnection connection) 
    {
        int num;

        //отображение навигации
        Console.WriteLine("Выберите действие: ");
        Console.WriteLine("1. Соединение в один файл");
        Console.WriteLine("2. Импорт в СУБД");
        Console.WriteLine("3. Хранимая процедура");
        Console.WriteLine("0. Выход ");

        // проверка на ввод(число, больше -1 и меньше 4)
        while (true) 
        {
            var input = Console.ReadLine();

            if (int.TryParse(input, out num) && Convert.ToInt32(input) <= 3 && Convert.ToInt32(input) >= 0)
                break;
            else
            {
                //отображение навигации
                Console.Clear();
                Console.WriteLine("Неправильный ввод");
                Console.WriteLine("Выберите действие: ");
                Console.WriteLine("1. Соединение в один файл");
                Console.WriteLine("2. Импорт в СУБД");
                Console.WriteLine("3. Хранимая процедура");
                Console.WriteLine("0. Выход ");
            }
        }
        string paths = @"V:\\file.txt"; // путь к общему файлу
        switch (num)
        {
            case 1:
                Console.Clear();
                Console.WriteLine("Введите подстроку:"); 
                string path = Console.ReadLine(); //получение строки для фильтрации
                long min = Obed(path, files, paths); // функция объединения файлов
                Console.WriteLine($"Кол-во удалённых: {min}");
                Men(files, connection);  // функция навигации
                break;
            case 2:
                Console.WriteLine("Введите путь к файлу:");
                string filePath = Console.ReadLine();
                if (File.Exists(filePath))
                {
                    Console.Clear();
                    Vivod(filePath, connection);
                }//вывод данных из общего файла в бд
                Men(files, connection);// функция навигации
                break;
            case 3:
                Console.Clear();
                SumMed(connection); // получение суммы и медианы
                Men(files, connection);  // функция навигации
                break;
            case 4:
                Console.Clear();
                Process.GetCurrentProcess().Kill();
                break;
            default:
                Process.GetCurrentProcess().Kill();
                break;
        }
    }
    // функция объединения файлов, возвращает количество удаленных строк
    static long Obed(string path, string[] files, string paths)
    {
        bool exist;

        int count = 0; // счётчик удаленных строк
        int is_new = 0; // переменная для создания нового файла при первом запуске функции

        //проход по всем файлам для объединения
        foreach (var file in files)
        {
            try
            {
                string line; 
                // открытие файла для чтения
                StreamReader sr = new(file);
                if (is_new == 0) 
                {
                    exist = false;  // установка значений для открытия файла записи false - создание и открытие, true - открытие для дозаписи
                    is_new++;
                } else exist = true;
                // открытие файла для записи
                StreamWriter sw2 = new(paths, exist); 
                line = sr.ReadLine(); 
                while (line != null)
                {
                    // проверка на вхождение значения, если в строке есть заданная комбинация, инкрементация счетчика
                    if (line.Contains(path) is false) 
                    {
                        sw2.WriteLine($"{line}");

                    }
                    else count++; 
                    line = sr.ReadLine();

                }
                sr.Close();
                sw2.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message); // вывод ошибок
            }

        }
        return count;
    }
    // класс для отображения прогресса при загрузке данных в бд
    public class ConsoleProgressBar
    {
        public int Left { get; }
        public int Top { get; }
        public int Length { get; }

        public ConsoleProgressBar(int left, int top, int length)
        {
            Left = left;
            Top = top;
            Length = length;
        }
        //функция отображения прогресса
        public void ShowProgress(int progress, long count, long ob) // значения progress < 100 && progress > 0
        {
            int p = progress * Length / 100;
            (int left, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(Left, Top);
            Console.Write($"[{new string('█', p / 2)}" +
                new string('▓', p / 2 + p % 2) +
                $"{new string('░', Length - p)}] {count}");
            Console.SetCursorPosition(left, top);
            Console.SetCursorPosition(left+85, top);
            Console.Write($"/{ob}");
            Console.SetCursorPosition(left, top);
        }
    }
    //функция нахождения ко-ва строк в файле
    static int Col_Str(string paths)
    {
        int Count = 0;
        string line;
        StreamReader get = new(paths);
        line = get.ReadLine();
        while (line != null)
        {
            Count++;
            line = get.ReadLine();
        }
        get.Close();
        return Count;
    }
    // функция импорта файлов в бд
    static void Vivod(string paths, SqlConnection connection)
    {
        // счётчик загруженных строк
        long count=0;
        //получение общего кол-ва строк
        long ob = Col_Str(paths);

        List<Stroka> gena = new();
        //проверка на наличие строк
        if (ob != 0) 
        {
            try
            {
                // открытие файла для чтения, начало чтения из файла
                string line;
                StreamReader get_genu = new(paths); 
                line = get_genu.ReadLine(); 
                while (line != null)
                {
                    // разделение строки на подстроки и заполнение данных объекта
                    Stroka stroka = new Stroka();
                    string[] words = line.Split("||"); 
                    stroka.Date = DateTime.Parse(words[0]);
                    stroka.Latin = words[1];
                    stroka.Rus = words[2];
                    stroka.Num_Int = Convert.ToInt32(words[3]);
                    stroka.Num_Float = Convert.ToDouble(words[4]);
                    gena.Add(stroka);
                    line = get_genu.ReadLine();
                }
                get_genu.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message); // вывод ошибок
            }

            // создание команды запроса
            SqlCommand command = new("INSERT INTO [Stroka] (Date,Latin,Rus,Num_Int,Num_Float) VALUES (@Date,@Latin,@Rus,@Num_Int,@Num_Float)", connection);
            command.Parameters.Add("@Date", SqlDbType.DateTime);
            command.Parameters.Add("@Latin", SqlDbType.NVarChar, 10);
            command.Parameters.Add("@Rus", SqlDbType.NVarChar, 10);
            command.Parameters.Add("@Num_Int", SqlDbType.BigInt);
            command.Parameters.Add("@Num_Float", SqlDbType.Float);

            // объявление данных для визуализации прогресса
            var pos = Console.CursorTop;
            string template = " Loading: ";
            Console.WriteLine(template);
            ConsoleProgressBar bar = new(template.Length, pos, 60);

            //заполнение бд данными и отправка
            for (int i = 0; i < gena.Count; i++)
            {
                command.Parameters["@Date"].Value = gena[i].Date;
                command.Parameters["@Latin"].Value = gena[i].Latin;
                command.Parameters["@Rus"].Value = gena[i].Rus;
                command.Parameters["@Num_Int"].Value = gena[i].Num_Int;
                command.Parameters["@Num_Float"].Value = gena[i].Num_Float;

                command.ExecuteNonQuery();
                count++;
                bar.ShowProgress((int)(count * 100 / ob), count, ob);
            }
            Console.WriteLine($"100%");
            Console.ResetColor();
        } else Console.WriteLine($"Сначала объедините файлы"); // обработка исключений
    }
    // функция использования хранимой процедуры
    static void SumMed(SqlConnection connection)
    {
        Console.Clear();

        //создание команды использования процедуры для отображения всей таблицы
        string sqlExpression = "db_Show";
        SqlCommand command_Show = new(sqlExpression, connection);
        command_Show.CommandType = System.Data.CommandType.StoredProcedure;

        // проверка на наличие записей
        object result = command_Show.ExecuteScalar();
        if (result != null)
        {
            //создание команды использования процедуры для отображения суммы и медианы
            string sqlExpression_SumMed = "db_SumMed";
            SqlCommand command_SumMed = new(sqlExpression_SumMed, connection);
            command_SumMed.CommandType = System.Data.CommandType.StoredProcedure;
            var reader_SumMed = command_SumMed.ExecuteReader();
            if (reader_SumMed.HasRows)
            {
                Console.WriteLine("{0}\t{1}", reader_SumMed.GetName(0), reader_SumMed.GetName(1));

                while (reader_SumMed.Read())
                {
                    long Sum = reader_SumMed.GetInt64(1);
                    double Med = reader_SumMed.GetDouble(0);
                    Console.WriteLine("{0}\t{1}", Med, Sum); // отображение данных в консоли 
                }
            }
            reader_SumMed.Close(); // закрытие 
        }
    }
}