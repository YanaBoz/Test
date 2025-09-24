using System.Data;
using System.Data.SqlClient;
using System.Text;
using Test.MA.Models;

namespace Test.MA.Services
{
    // Сервис для работы с базой: импорт файлов в таблицу Stroka и вызов хранимой процедуры.
    public class DatabaseService : IDisposable
    {
        private readonly SqlConnection _connection;
        private bool _disposed = false;

        public DatabaseService()
        {
            // Строка подключения хардкодом; по возможности получать из конфигурации
            var connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30";
            _connection = new SqlConnection(connectionString);

            try
            {
                _connection.Open();
                Console.WriteLine("Подключение к TestDB установлено успешно!");
            }
            catch (SqlException ex)
            {
                // Если не получилось подключиться — пробуем инициализировать БД (создать) и повторить подключение
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
                Console.WriteLine("Попытка переинициализировать базу данных...");
                DatabaseInitializer.InitializeDatabase();
                _connection.Open();
                Console.WriteLine("Подключение установлено после создания БД!");
            }
        }

        // Импорт файла построчно с батчевой заливкой через SqlBulkCopy.
        // batchSize: сколько строк на один bulk write (умолчание 10_000).
        public async Task ImportFromFileAsync(string filePath, int batchSize = 10_000)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Файл не существует");
                return;
            }

            long totalLines = FileService.CountLines(filePath);
            if (totalLines == 0)
            {
                Console.WriteLine("Файл пуст");
                return;
            }

            // Создаём прогресс-бар, резервируем строку для него
            var progressBar = new ConsoleProgressBar(0, Console.CursorTop, 50);
            Console.WriteLine(); // Переходим на новую строку для прогресс-бара

            var table = CreateDataTableSchema();
            long processedCount = 0;

            try
            {
                // Начинаем транзакцию — все батчи будут в одной транзакции.
                // Это обеспечивает атомарность, но увеличивает время удержания блокировок.
                using var transaction = _connection.BeginTransaction();
                using var reader = new StreamReader(filePath, Encoding.UTF8, true, 65536);
                string? line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    try
                    {
                        // Парсим строку в модель Stroka (Parse выбросит исключение при неверном формате)
                        var stroka = Stroka.Parse(line);

                        // Добавляем в DataTable (схема должна совпадать с таблицей в БД)
                        var row = table.NewRow();
                        row["Date"] = stroka.Date;
                        row["Latin"] = stroka.Latin;
                        row["Rus"] = stroka.Rus;
                        row["Num_Int"] = stroka.Num_Int;
                        row["Num_Float"] = stroka.Num_Float;
                        table.Rows.Add(row);

                        processedCount++;

                        // Обновляем прогресс-бар пакетно
                        if (processedCount % 1000 == 0 || processedCount == totalLines)
                        {
                            int progress = (int)((double)processedCount / totalLines * 100);
                            progressBar.ShowProgress(progress, processedCount, totalLines);
                        }

                        // Если накопилось достаточно строк — отправляем батч в БД
                        if (table.Rows.Count >= batchSize)
                        {
                            await WriteBatchAsync(table, transaction);
                            table.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ошибка парсинга/записи конкретной строки — логируем и продолжаем
                        Console.WriteLine($"\nОшибка обработки строки {processedCount + 1}: {ex.Message}");
                        continue;
                    }
                }

                // Остаток строк
                if (table.Rows.Count > 0)
                {
                    await WriteBatchAsync(table, transaction);
                    table.Clear();
                }

                // Коммит транзакции — данные сохраняются в БД
                transaction.Commit();
                Console.WriteLine("\n\nИмпорт завершён успешно!");
            }
            catch (Exception ex)
            {
                // При ошибке логируем и пробрасываем исключение (точнее: можно откатить, но using transaction откатит при dispose)
                Console.WriteLine($"\nОшибка импорта: {ex.Message}");
                Console.WriteLine("Откат транзакции...");
                throw;
            }
        }

        // Создаём DataTable со схемой, соответствующей таблице Stroka (включая типы колонок)
        private static DataTable CreateDataTableSchema()
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("Date", typeof(DateTime)));
            table.Columns.Add(new DataColumn("Latin", typeof(string)) { MaxLength = 10 });
            table.Columns.Add(new DataColumn("Rus", typeof(string)) { MaxLength = 10 });
            // В DB Num_Int объявлен как BIGINT, поэтому здесь используется long
            table.Columns.Add(new DataColumn("Num_Int", typeof(long)));
            table.Columns.Add(new DataColumn("Num_Float", typeof(double)));
            return table;
        }

        // Запись батча в БД через SqlBulkCopy с сопоставлением колонок
        private async Task WriteBatchAsync(DataTable tableBatch, SqlTransaction transaction)
        {
            using var bulk = new SqlBulkCopy(_connection, SqlBulkCopyOptions.KeepIdentity, transaction)
            {
                DestinationTableName = "Stroka",
                BatchSize = tableBatch.Rows.Count,
                BulkCopyTimeout = 0 // без ограничения по времени
            };

            // Настройка mapping'а колонок DataTable -> таблица в БД
            bulk.ColumnMappings.Add("Date", "Date");
            bulk.ColumnMappings.Add("Latin", "Latin");
            bulk.ColumnMappings.Add("Rus", "Rus");
            bulk.ColumnMappings.Add("Num_Int", "Num_Int");
            bulk.ColumnMappings.Add("Num_Float", "Num_Float");

            await bulk.WriteToServerAsync(tableBatch);
        }

        // Вызов хранимой процедуры db_SumMed и вывод результата в консоль
        public void ExecuteStoredProcedure()
        {
            try
            {
                using var command = new SqlCommand("db_SumMed", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    Console.WriteLine("{0,-15}\t{1}", "Медиана", "Сумма");
                    Console.WriteLine(new string('-', 30));

                    while (reader.Read())
                    {
                        // Безопасное чтение полей (проверка DBNull)
                        double median = reader.IsDBNull(0) ? 0 : Convert.ToDouble(reader[0]);
                        long sum = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                        Console.WriteLine("{0,-15:F8}\t{1:N0}", median, sum);
                    }
                }
                else
                {
                    Console.WriteLine("Нет данных в таблице");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка хранимой процедуры: {ex.Message}");
            }
        }

        // Стандартный IDisposable для закрытия соединения
        public void Dispose()
        {
            if (!_disposed)
            {
                _connection?.Close();
                _connection?.Dispose();
                _disposed = true;
            }
        }
    }
}
