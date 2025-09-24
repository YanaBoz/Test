using System.Text;
using Test.MA.Generators;
using Test.MA.Models;

namespace Test.MA.Services
{
    // Утилитарный класс для операцией с файлами:
    // - генерация файлов с тестовыми данными,
    // - слияние/фильтрация файлов,
    // - подсчёт строк,
    // - последовательное чтение объектов Stroka из файла.
    public class FileService
    {
        // Генерация fileCount файлов, каждый с linesPerFile строками, в папке basePath.
        // Возвращает массив путей к созданным файлам.
        public static string[] GenerateFiles(int fileCount, int linesPerFile, string basePath)
        {
            var files = new string[fileCount];
            var generator = new StrokaGenerator();

            // Убедимся, что директория существует
            Directory.CreateDirectory(basePath);

            for (int i = 0; i < fileCount; i++)
            {
                string filePath = Path.Combine(basePath, $"filename{i + 1}.txt");
                files[i] = filePath;

                // Запись в UTF-8 с большим буфером для скорости
                using var writer = new StreamWriter(filePath, false, Encoding.UTF8, 65536);
                var data = generator.Generate(linesPerFile);

                foreach (var item in data)
                {
                    writer.WriteLine(item.ToString());
                }
            }

            return files;
        }

        // Слияние множества файлов в один, с фильтрацией строк, содержащих filter.
        // Возвращает количество удалённых/отфильтрованных строк.
        public static long MergeFiles(string filter, string[] inputFiles, string outputFile)
        {
            long removedCount = 0;

            // Создаём директорию назначения
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile) ?? ".");

            using var outputWriter = new StreamWriter(outputFile, false, Encoding.UTF8, 65536);

            foreach (var inputFile in inputFiles)
            {
                using var reader = new StreamReader(inputFile, Encoding.UTF8, true, 65536);
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    // Если строка не содержит фильтр — записываем в выходной файл
                    if (!line.Contains(filter))
                    {
                        outputWriter.WriteLine(line);
                    }
                    else
                    {
                        removedCount++;
                    }
                }
            }

            return removedCount;
        }

        // Подсчёт строк в файле (эффективно для небольших/средних файлов).
        // Для очень больших файлов можно оптимизировать, читая блоками байтов.
        public static int CountLines(string filePath)
        {
            int count = 0;

            using var reader = new StreamReader(filePath);
            while (reader.ReadLine() != null)
            {
                count++;
            }

            return count;
        }

        // Итератор для последовательного чтения моделей Stroka из файла.
        // Позволяет экономить память: строки парсятся по одной и возвращаются вызывающему по мере чтения.
        public static IEnumerable<Stroka> ReadStrokaFromFile(string filePath)
        {
            using var reader = new StreamReader(filePath);
            string? line;

            while ((line = reader.ReadLine()) != null)
            {
                yield return Stroka.Parse(line);
            }
        }
    }
}
