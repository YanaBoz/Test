using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Globalization;
using Test.Core.Interfaces;
using Test.Core.Models;

namespace Test.Infrastructure.Services
{
    // Сервис для чтения Excel-файлов (NPOI) и генерации отчетов (ClosedXML).
    public class ExcelProcessor : IExcelProcessor
    {
        public async Task<List<Turnover>> ProcessExcelFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            // Проверяем наличие файла
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("Excel file not found", filePath);

            var ext = Path.GetExtension(filePath).ToLower();
            IWorkbook workbook;

            // Открываем поток чтения файла. NPOI читает из потока.
            using var fs = File.OpenRead(filePath);
            if (ext == ".xls")
                workbook = new HSSFWorkbook(fs); // старый формат (BIFF)
            else if (ext == ".xlsx" || ext == ".xlsm" || ext == ".xltx" || ext == ".xltm")
                workbook = new XSSFWorkbook(fs); // новый формат (OpenXML)
            else
                throw new ArgumentException($"Unsupported Excel format '{ext}'");

            // Берём первый лист (index 0)
            var sheet = workbook.GetSheetAt(0);
            var data = new List<Turnover>();

            // В вашем коде данные начинаются с 8-й строки (индекс 8),
            // и вы идёте до sheet.LastRowNum - 2 (возможно, чтобы пропустить итоговые строки).
            // Это специфично для входного шаблона Excel — учтите это при изменении шаблона.
            for (int rowIndex = 8; rowIndex <= sheet.LastRowNum - 2; rowIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var row = sheet.GetRow(rowIndex);
                if (row == null) continue;

                // Первый столбец — номер счёта. Пытаемся распарсить в int.
                var accountCell = row.GetCell(0);
                if (!int.TryParse(accountCell?.ToString(), out int account) || (account / 100) <= 1)
                    continue; // Пропускаем строки без валидного счёта или с номером <= 100

                // Формируем объект Turnover из ячеек строки
                var turnover = new Turnover
                {
                    Account = account,
                    Start_Active = GetNumericCellValue(row.GetCell(1)),
                    Start_Passive = GetNumericCellValue(row.GetCell(2)),
                    Turn_Debit = GetNumericCellValue(row.GetCell(3)),
                    Turn_Credit = GetNumericCellValue(row.GetCell(4))
                };

                data.Add(turnover);
            }

            // Возвращаем результат. Здесь нет асинхронной работы — завернули в Task.FromResult.
            return await Task.FromResult(data);
        }
        public async Task<string> GenerateReportAsync(List<Constructor> data, CancellationToken cancellationToken = default)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            // Директория для сохранения отчётов — wwwroot/Files_Load
            var baseDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files_Load");
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

            var fileName = $"Report_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(baseDir, fileName);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Report");

            // Заголовки колонок
            ws.Cell(1, 1).Value = "CL";
            ws.Cell(1, 2).Value = "B_sch";
            ws.Cell(1, 3).Value = "VS_A";
            ws.Cell(1, 4).Value = "VS_P";
            ws.Cell(1, 5).Value = "O_D";
            ws.Cell(1, 6).Value = "O_C";
            ws.Cell(1, 7).Value = "IS_A";
            ws.Cell(1, 8).Value = "IS_P";

            int r = 2;
            foreach (var item in data)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Заполнение строк отчёта
                ws.Cell(r, 1).Value = item.CL;
                ws.Cell(r, 2).Value = item.B_sch;
                ws.Cell(r, 3).Value = item.VS_A;
                ws.Cell(r, 4).Value = item.VS_P;
                ws.Cell(r, 5).Value = item.O_D;
                ws.Cell(r, 6).Value = item.O_C;
                ws.Cell(r, 7).Value = item.IS_A;
                ws.Cell(r, 8).Value = item.IS_P;
                r++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(filePath);

            return await Task.FromResult(filePath);
        }

        private double GetNumericCellValue(ICell cell)
        {
            if (cell == null) return 0;

            switch (cell.CellType)
            {
                case CellType.Numeric:
                    return cell.NumericCellValue;

                case CellType.String:
                    // Убираем пробелы и заменяем запятую на точку для парсинга через InvariantCulture
                    var str = cell.StringCellValue.Trim().Replace(',', '.');
                    if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                        return val;
                    return 0;

                case CellType.Formula:
                    // Если формула возвращает числовое значение, вернём его.
                    // Заметьте: в некоторых случаях требуется EvaluateFormulaCell — это упрощение.
                    return cell.NumericCellValue;

                case CellType.Boolean:
                    // Логические значения приводим к 1/0
                    return cell.BooleanCellValue ? 1 : 0;

                default:
                    return 0;
            }
        }
    }
}
