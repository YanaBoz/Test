using Test.Application.Interfaces;
using Test.Core.Interfaces;
using Test.Core.Models;

namespace Test.Application.Services
{
    public class ExcelProcessingService : IExcelProcessingService
    {
        private readonly IExcelProcessor _excelProcessor; // Сервис для обработки Excel-файлов
        private readonly IFileService _fileService;       // Сервис для работы с файлами (загрузка, хранение)

        public ExcelProcessingService(IExcelProcessor excelProcessor, IFileService fileService)
        {
            _excelProcessor = excelProcessor;
            _fileService = fileService;
        }

        // Обработка одного загруженного файла
        public async Task<List<Turnover>> ProcessUploadedFileAsync(Stream fileStream, string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            if (ext != ".xlsx" && ext != ".xls")
                throw new ArgumentException("Поддерживаются только .xls и .xlsx");

            // Сохраняем файл и получаем путь
            var path = await _fileService.UploadFileAsync(fileStream, fileName);

            // Обрабатываем Excel и возвращаем список оборотов
            return await _excelProcessor.ProcessExcelFileAsync(path);
        }

        // Обработка нескольких файлов по списку путей
        public async Task<List<Turnover>> ProcessMultipleFilesAsync(IEnumerable<string> filePaths)
        {
            var allData = new List<Turnover>();
            foreach (var path in filePaths)
            {
                if (!File.Exists(path)) continue; // Пропускаем отсутствующие файлы
                var data = await _excelProcessor.ProcessExcelFileAsync(path);
                allData.AddRange(data);
            }

            // Возвращаем отсортированный список по счету
            return allData.OrderBy(t => t.Account).ToList();
        }
    }

}
