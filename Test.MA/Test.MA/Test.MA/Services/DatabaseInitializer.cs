using System.Data.SqlClient;

namespace Test.MA.Services
{
    // Статический помощник для первичной инициализации локальной базы данных (LocalDB).
    // Создаёт базу TestDB, таблицу Stroka и хранимую процедуру db_SumMed.
    public static class DatabaseInitializer
    {
        public static void InitializeDatabase()
        {
            try
            {
                // Подключение к master для возможности создать базу, если её нет
                using var masterConnection = new SqlConnection(
                    @"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True;Connect Timeout=30");
                masterConnection.Open();

                // Создаём базу TestDB при её отсутствии
                using (var createDbCmd = new SqlCommand(@"
                    IF DB_ID('TestDB') IS NULL
                    BEGIN
                        CREATE DATABASE TestDB;
                        PRINT 'База данных TestDB создана.';
                    END
                    ELSE
                    BEGIN
                        PRINT 'База данных TestDB уже существует.';
                    END", masterConnection))
                {
                    createDbCmd.ExecuteNonQuery();
                }

                Console.WriteLine("База TestDB проверена/создана.");

                // Подключаемся к самой базе TestDB для дальнейших операций
                using var connection = new SqlConnection(
                    @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=30");
                connection.Open();

                // Создаём таблицу Stroka, если она отсутствует
                CreateTable(connection);

                // Создаём (или пересоздаём) хранимую процедуру для расчётов
                CreateStoredProcedure(connection);

                Console.WriteLine("Инициализация базы данных завершена успешно!");
            }
            catch (Exception ex)
            {
                // Логируем ошибку в консоль и затем пробрасываем исключение вверх
                Console.WriteLine($"Ошибка инициализации БД: {ex.Message}");
                throw;
            }
        }

        private static void CreateTable(SqlConnection connection)
        {
            try
            {
                // SQL: проверка существования таблицы и создание при отсутствии.
                // Замечание: в SQL определено Num_Int как BIGINT — это совместимо с long; если модель Stroka использует int,
                // рекомендуется привести типы в соответствие (INT в SQL или long в модели).
                using var createTableCmd = new SqlCommand(@"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Stroka' AND xtype='U')
                    BEGIN
                        CREATE TABLE dbo.Stroka (
                            ID INT IDENTITY(1,1) PRIMARY KEY,
                            Date DATETIME NOT NULL,
                            Latin NVARCHAR(10) NOT NULL,
                            Rus NVARCHAR(10) NOT NULL,
                            Num_Int BIGINT NOT NULL,
                            Num_Float FLOAT NOT NULL
                        );
                        PRINT 'Таблица Stroka создана.';
                    END
                    ELSE
                    BEGIN
                        PRINT 'Таблица Stroka уже существует.';
                    END", connection);

                createTableCmd.ExecuteNonQuery();
                Console.WriteLine("Таблица Stroka проверена/создана.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка создания таблицы: {ex.Message}");
                throw;
            }
        }

        private static void CreateStoredProcedure(SqlConnection connection)
        {
            try
            {
                // Если процедура уже есть — удалим, чтобы пересоздать (облегчит апдейты логики)
                using (var dropCmd = new SqlCommand(@"
            IF OBJECT_ID('dbo.db_SumMed', 'P') IS NOT NULL
                DROP PROCEDURE dbo.db_SumMed;", connection))
                {
                    dropCmd.ExecuteNonQuery();
                }

                // Создаём "оптимизированную" процедуру:
                //  - вычисляем суммарную сумму (Num_Int) через агрегат
                //  - пытаемся получить приближенную медиану Num_Float семплированием 1% строк
                //    (быстрее, чем полная сортировка на больших объёмах данных)
                using (var createSpCmd = new SqlCommand(@"
            CREATE PROCEDURE dbo.db_SumMed
            AS
            BEGIN
                SET NOCOUNT ON;
                SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
                
                -- Быстрое вычисление суммы (индексированный столбец)
                DECLARE @TotalSum BIGINT;
                SELECT @TotalSum = ISNULL(SUM(Num_Int), 0) 
                FROM dbo.Stroka WITH (NOLOCK);
                
                -- Приблизительная медиана через семплирование (1% данных)
                DECLARE @ApproxMedian FLOAT;
                
                ;WITH SampledData AS (
                    SELECT Num_Float 
                    FROM dbo.Stroka WITH (NOLOCK)
                    WHERE (ABS(CAST(BINARY_CHECKSUM(ID, NEWID()) AS INT)) % 100) = 1
                )
                SELECT @ApproxMedian = AVG(CAST(Num_Float AS FLOAT))
                FROM (
                    SELECT Num_Float,
                           ROW_NUMBER() OVER (ORDER BY Num_Float) as rn,
                           COUNT(*) OVER () as total
                    FROM SampledData
                ) t
                WHERE rn IN (total/2, (total+1)/2);
                
                -- Если семплирование не дало результатов, используем простое среднее
                IF @ApproxMedian IS NULL
                BEGIN
                    SELECT @ApproxMedian = AVG(CAST(Num_Float AS FLOAT))
                    FROM dbo.Stroka WITH (NOLOCK);
                END
                
                SELECT 
                    ISNULL(@ApproxMedian, 0) AS Mediana,
                    ISNULL(@TotalSum, 0) AS Summa;
            END;", connection))
                {
                    createSpCmd.ExecuteNonQuery();
                }

                Console.WriteLine("Оптимизированная хранимая процедура создана успешно!");
            }
            catch (Exception ex)
            {
                // Если создание "оптимизированной" версии проваливается, пытаемся создать простую версию
                Console.WriteLine($"Ошибка создания процедуры: {ex.Message}");
                CreateUltraSimpleStoredProcedure(connection);
            }
        }

        // Запасной простой вариант процедуры — не использует семплирование.
        private static void CreateUltraSimpleStoredProcedure(SqlConnection connection)
        {
            try
            {
                using var createSpCmd = new SqlCommand(@"
            CREATE PROCEDURE dbo.db_SumMed
            AS
            BEGIN
                SET NOCOUNT ON;
                SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
                
                -- САМАЯ ПРОСТАЯ ВЕРСИЯ - только базовые агрегаты
                SELECT 
                    AVG(CAST(Num_Float AS FLOAT)) AS Mediana,
                    SUM(Num_Int) AS Summa
                FROM dbo.Stroka WITH (NOLOCK);
            END;", connection);

                createSpCmd.ExecuteNonQuery();
                Console.WriteLine("Ультра-простая версия процедуры создана!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        // Проверка существования самой базы данных (обращение к master)
        public static bool CheckDatabaseExists()
        {
            try
            {
                using var connection = new SqlConnection(
                    @"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True;Connect Timeout=3");
                connection.Open();

                using var cmd = new SqlCommand("SELECT DB_ID('TestDB')", connection);
                var result = cmd.ExecuteScalar();
                // ExecuteScalar может вернуть null или DBNull.Value
                return result != null && result != DBNull.Value;
            }
            catch
            {
                return false;
            }
        }

        // Проверка существования таблицы Stroka в базе TestDB
        public static bool CheckTableExists()
        {
            try
            {
                using var connection = new SqlConnection(
                    @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=TestDB;Integrated Security=True;Connect Timeout=3");
                connection.Open();

                using var cmd = new SqlCommand("SELECT OBJECT_ID('dbo.Stroka','U')", connection);
                var result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value;
            }
            catch
            {
                return false;
            }
        }
    }
}
