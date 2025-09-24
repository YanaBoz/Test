using Test.MA.Services;
using System.Data.SqlClient;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== Инициализация системы ===");

            // Проверяем доступность LocalDB (локальной инстанции SQL Server Express LocalDB)
            CheckLocalDbInstallation();

            // Инициализируем базу (создание TestDB, таблицы и процедуры — если нужно)
            Console.WriteLine("Инициализация базы данных...");
            DatabaseInitializer.InitializeDatabase();

            // Параметры генерации/работы: количество файлов, строк на файл, папка для файлов и путь для объединённого файла
            const int fileCount = 100;
            const int linesPerFile = 100000;
            const string basePath = @"C:\Temp\";
            const string mergedFilePath = @"C:\Temp\file.txt";

            // Создаём директорию, если отсутствует
            Directory.CreateDirectory(basePath);

            Console.WriteLine("Генерация файлов...");
            // Генерируем тестовые файлы — метод возвращает массив путей
            var files = FileService.GenerateFiles(fileCount, linesPerFile, basePath);
            Console.WriteLine($"Сгенерировано {fileCount} файлов");

            // Создаём сервис работы с БД (открывает соединение и при необходимости вызывает инициализатор)
            using var dbService = new DatabaseService();

            // Главное меню — интерактивное
            while (true)
            {
                DisplayMenu();
                var choice = GetMenuChoice();

                switch (choice)
                {
                    case 1:
                        await MergeFilesCase(files, mergedFilePath);
                        break;
                    case 2:
                        await ImportToDatabaseCase(mergedFilePath, dbService);
                        break;
                    case 3:
                        ExecuteStoredProcedureCase(dbService);
                        break;
                    case 0:
                        Console.WriteLine("Выход...");
                        return;
                }
            }
        }
        catch (Exception ex)
        {
            // Любая необработанная ошибка — выводим и ожидаем нажатия клавиши
            Console.WriteLine($"Критическая ошибка: {ex.Message}");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadLine();
        }
    }

    // Проверка доступности LocalDB: пытаемся подключиться и запросить @@VERSION
    private static void CheckLocalDbInstallation()
    {
        Console.WriteLine("Проверка LocalDB...");
        try
        {
            using var connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=true;Connect Timeout=3");
            connection.Open();

            using var versionCmd = new SqlCommand("SELECT @@VERSION", connection);
            var version = versionCmd.ExecuteScalar();

            Console.WriteLine($"✓ LocalDB доступен: {version}");
            connection.Close();
        }
        catch (Exception ex)
        {
            // Если LocalDB недоступен — информируем пользователя и прерываем выполнение (throw)
            Console.WriteLine($"✗ LocalDB не доступен: {ex.Message}");
            Console.WriteLine("Установите LocalDB через Visual Studio Installer");
            Console.WriteLine("Или скачайте: https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb");
            throw;
        }
    }

    // Отрисовка простого меню
    private static void DisplayMenu()
    {
        Console.Clear();
        Console.WriteLine("=== Меню управления ===");
        Console.WriteLine("1. Объединение файлов с фильтрацией");
        Console.WriteLine("2. Импорт в базу данных");
        Console.WriteLine("3. Выполнить хранимую процедуру");
        Console.WriteLine("0. Выход");
        Console.Write("Выберите действие: ");
    }

    // Чтение выбора пользователя — валидация целого 0..3
    private static int GetMenuChoice()
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 0 && choice <= 3)
                return choice;

            Console.Write("Неверный ввод. Выберите действие (0-3): ");
        }
    }

    // Сценарий: объединение файлов с фильтрацией — пользователь вводит подстроку, строки содержащие её будут удалены
    private static async Task MergeFilesCase(string[] files, string mergedFilePath)
    {
        Console.Write("Введите подстроку для фильтрации: ");
        string filter = Console.ReadLine() ?? string.Empty;

        Console.WriteLine("Объединение файлов...");
        // Операция синхронная CPU/IO — оборачиваем в Task.Run, чтобы не блокировать UI потока (в консоли это необязательно)
        long removedCount = await Task.Run(() => FileService.MergeFiles(filter, files, mergedFilePath));

        Console.WriteLine($"\nОбъединение завершено. Удалено строк: {removedCount}");
        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    // Сценарий: импорт объединённого файла в базу данных
    private static async Task ImportToDatabaseCase(string filePath, DatabaseService dbService)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл не существует. Сначала выполните объединение файлов.");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Начало импорта в базу данных...");
        await dbService.ImportFromFileAsync(filePath);

        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    // Сценарий: вызов хранимой процедуры и вывод результата
    private static void ExecuteStoredProcedureCase(DatabaseService dbService)
    {
        Console.WriteLine("Результаты хранимой процедуры:");
        Console.WriteLine(new string('=', 30));

        dbService.ExecuteStoredProcedure();

        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }
}
