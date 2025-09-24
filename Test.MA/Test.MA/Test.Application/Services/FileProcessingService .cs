using ClosedXML.Excel;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Test.Application.DTOs;
using Test.Application.Interfaces;
using Test.Application.Services.Interfaces;
using Test.Core.Interfaces;
using Test.Core.Models;

namespace Test.Application.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        private readonly IExcelProcessingService _excelService; // Сервис обработки Excel
        private readonly ITurnoverService _turnoverService;      // Сервис обработки оборотов
        private readonly IUnitOfWork _unitOfWork;               // Работа с репозиториями
        private readonly IMapper _mapper;                       // Mapster для маппинга моделей и DTO

        public FileProcessingService(
            IExcelProcessingService excelService,
            ITurnoverService turnoverService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _excelService = excelService;
            _turnoverService = turnoverService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // Загрузка и обработка файла
        public async Task<List<ConstructorDto>> UploadAndProcessFileAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();

            // Получаем обороты из Excel
            var turnovers = await _excelService.ProcessUploadedFileAsync(stream, file.FileName);
            if (!turnovers.Any()) return new List<ConstructorDto>();

            // Проверяем, был ли файл уже загружен
            var existingFile = (await _unitOfWork.FileRepository.GetAllAsync())
                .FirstOrDefault(f => f.Path == file.FileName);

            if (existingFile != null)
            {
                // Если файл есть, обрабатываем его данные
                var existingTurnovers = (await _unitOfWork.TurnoverRepository.GetAllAsync())
                    .Where(t => t.SourceFileName == file.FileName)
                    .ToList();

                var processedExisting = await _turnoverService.ProcessTurnoverDataAsync(existingTurnovers);
                return _mapper.Map<List<ConstructorDto>>(processedExisting);
            }

            // Сохраняем новый файл
            var fileModel = new FileModel { Name = file.FileName, Path = file.FileName, UploadDate = DateTime.UtcNow };
            await _unitOfWork.FileRepository.AddAsync(fileModel);
            await _unitOfWork.SaveChangesAsync();

            // Сохраняем обороты, привязанные к файлу
            foreach (var t in turnovers)
            {
                t.SourceFileName = file.FileName;
                await _unitOfWork.TurnoverRepository.AddAsync(t);
            }
            await _unitOfWork.SaveChangesAsync();

            var processed = await _turnoverService.ProcessTurnoverDataAsync(turnovers);
            return _mapper.Map<List<ConstructorDto>>(processed);
        }

        // Получение списка всех файлов
        public async Task<List<FileDto>> GetFilesAsync()
        {
            var files = await _unitOfWork.FileRepository.GetAllAsync();
            return _mapper.Map<List<FileDto>>(files);
        }

        // Получение обработанных данных конкретного файла
        public async Task<List<ConstructorDto>> GetFileDataAsync(int fileId)
        {
            var file = await _unitOfWork.FileRepository.GetByIdAsync(fileId);
            if (file == null) return new List<ConstructorDto>();

            var turnovers = (await _unitOfWork.TurnoverRepository.GetAllAsync())
                .Where(t => t.SourceFileName == file.Path)
                .ToList();

            if (!turnovers.Any()) return new List<ConstructorDto>();

            var processed = await _turnoverService.ProcessTurnoverDataAsync(turnovers);
            return _mapper.Map<List<ConstructorDto>>(processed);
        }

        // Генерация Excel-файла для скачивания
        public async Task<string> DownloadFileAsync(int fileId)
        {
            var file = await _unitOfWork.FileRepository.GetByIdAsync(fileId);
            if (file == null) throw new InvalidOperationException("Файл не найден.");

            var turnovers = (await _unitOfWork.TurnoverRepository.GetAllAsync())
                .Where(t => t.SourceFileName == file.Path)
                .ToList();
            if (!turnovers.Any()) throw new InvalidOperationException("Нет данных для загрузки.");

            var processed = await _turnoverService.ProcessTurnoverDataAsync(turnovers);

            // Путь для сохранения Excel
            var baseDir = Path.Combine(Directory.GetCurrentDirectory() ?? "", "wwwroot", "Files_Load");
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

            var fileName = $"File_{file.Name}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(baseDir, fileName);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Data");

            // Заголовки
            ws.Cell(1, 1).Value = "Б/сч";
            ws.Cell(1, 2).Value = "ВХОДЯЩЕЕ САЛЬДО Актив";
            ws.Cell(1, 3).Value = "ВХОДЯЩЕЕ САЛЬДО Пассив";
            ws.Cell(1, 4).Value = "ОБОРОТЫ Дебет";
            ws.Cell(1, 5).Value = "ОБОРОТЫ Кредит";
            ws.Cell(1, 6).Value = "ИСХОДЯЩЕЕ САЛЬДО Актив";
            ws.Cell(1, 7).Value = "ИСХОДЯЩЕЕ САЛЬДО Пассив";

            int r = 2;

            // Группировка данных по разделам
            foreach (var sectionGroup in processed.GroupBy(x => x.CL))
            {
                var sectionSums = new ConstructorDto
                {
                    B_sch = sectionGroup.Key,
                    VS_A = 0,
                    VS_P = 0,
                    O_D = 0,
                    O_C = 0,
                    IS_A = 0,
                    IS_P = 0
                };

                foreach (var item in sectionGroup)
                {
                    if (item.B_sch == item.CL) continue; // Пропускаем строки сумм раздела

                    // Заполнение данных
                    ws.Cell(r, 1).Value = item.B_sch;
                    ws.Cell(r, 2).Value = item.VS_A;
                    ws.Cell(r, 3).Value = item.VS_P;
                    ws.Cell(r, 4).Value = item.O_D;
                    ws.Cell(r, 5).Value = item.O_C;
                    ws.Cell(r, 6).Value = item.IS_A;
                    ws.Cell(r, 7).Value = item.IS_P;

                    // Форматирование чисел
                    for (int c = 2; c <= 7; c++)
                        ws.Cell(r, c).Style.NumberFormat.Format = "#,##0.00";

                    // Суммирование по разделу
                    sectionSums.VS_A += item.VS_A;
                    sectionSums.VS_P += item.VS_P;
                    sectionSums.O_D += item.O_D;
                    sectionSums.O_C += item.O_C;
                    sectionSums.IS_A += item.IS_A;
                    sectionSums.IS_P += item.IS_P;

                    r++;
                }

                // Добавляем итог раздела
                ws.Cell(r, 1).Value = sectionSums.B_sch;
                ws.Cell(r, 2).Value = sectionSums.VS_A / 2; // Возможно, деление для корректировки
                ws.Cell(r, 3).Value = sectionSums.VS_P / 2;
                ws.Cell(r, 4).Value = sectionSums.O_D / 2;
                ws.Cell(r, 5).Value = sectionSums.O_C / 2;
                ws.Cell(r, 6).Value = sectionSums.IS_A / 2;
                ws.Cell(r, 7).Value = sectionSums.IS_P / 2;

                ws.Row(r).Style.Font.Bold = true;
                for (int c = 2; c <= 7; c++)
                    ws.Cell(r, c).Style.NumberFormat.Format = "#,##0.00";

                r++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(filePath);

            return filePath;
        }

        // Объединение всех файлов в один Excel
        public async Task<string> MergeAllFilesAsync()
        {
            var files = await _unitOfWork.FileRepository.GetAllAsync();
            if (!files.Any()) throw new InvalidOperationException("Нет файлов для объединения.");

            var allTurnovers = new List<Turnover>();
            foreach (var file in files)
            {
                var turnovers = (await _unitOfWork.TurnoverRepository.GetAllAsync())
                    .Where(t => t.SourceFileName == file.Path)
                    .ToList();
                allTurnovers.AddRange(turnovers);
            }
            if (!allTurnovers.Any()) throw new InvalidOperationException("Нет данных для объединения.");

            var processed = await _turnoverService.ProcessTurnoverDataAsync(allTurnovers);

            var baseDir = Path.Combine(Directory.GetCurrentDirectory() ?? "", "wwwroot", "Files_Load");
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

            var fileName = $"Merged_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(baseDir, fileName);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Merged");

            ws.Cell(1, 1).Value = "Б/сч";
            ws.Cell(1, 2).Value = "ВХОДЯЩЕЕ САЛЬДО Актив";
            ws.Cell(1, 3).Value = "ВХОДЯЩЕЕ САЛЬДО Пассив";
            ws.Cell(1, 4).Value = "ОБОРОТЫ Дебет";
            ws.Cell(1, 5).Value = "ОБОРОТЫ Кредит";
            ws.Cell(1, 6).Value = "ИСХОДЯЩЕЕ САЛЬДО Актив";
            ws.Cell(1, 7).Value = "ИСХОДЯЩЕЕ САЛЬДО Пассив";

            int r = 2;

            foreach (var sectionGroup in processed.GroupBy(x => x.CL))
            {
                var sectionSums = new ConstructorDto
                {
                    B_sch = sectionGroup.Key,
                    VS_A = 0,
                    VS_P = 0,
                    O_D = 0,
                    O_C = 0,
                    IS_A = 0,
                    IS_P = 0
                };

                foreach (var item in sectionGroup)
                {
                    if (item.B_sch == item.CL) continue;

                    ws.Cell(r, 1).Value = item.B_sch;
                    ws.Cell(r, 2).Value = item.VS_A;
                    ws.Cell(r, 3).Value = item.VS_P;
                    ws.Cell(r, 4).Value = item.O_D;
                    ws.Cell(r, 5).Value = item.O_C;
                    ws.Cell(r, 6).Value = item.IS_A;
                    ws.Cell(r, 7).Value = item.IS_P;

                    for (int c = 2; c <= 7; c++)
                        ws.Cell(r, c).Style.NumberFormat.Format = "#,##0.00";

                    sectionSums.VS_A += item.VS_A;
                    sectionSums.VS_P += item.VS_P;
                    sectionSums.O_D += item.O_D;
                    sectionSums.O_C += item.O_C;
                    sectionSums.IS_A += item.IS_A;
                    sectionSums.IS_P += item.IS_P;

                    r++;
                }

                ws.Cell(r, 1).Value = sectionSums.B_sch;
                ws.Cell(r, 2).Value = sectionSums.VS_A / 2;
                ws.Cell(r, 3).Value = sectionSums.VS_P / 2;
                ws.Cell(r, 4).Value = sectionSums.O_D / 2;
                ws.Cell(r, 5).Value = sectionSums.O_C / 2;
                ws.Cell(r, 6).Value = sectionSums.IS_A / 2;
                ws.Cell(r, 7).Value = sectionSums.IS_P / 2;

                ws.Row(r).Style.Font.Bold = true;
                for (int c = 2; c <= 7; c++)
                    ws.Cell(r, c).Style.NumberFormat.Format = "#,##0.00";

                r++;
            }

            ws.Columns().AdjustToContents();
            workbook.SaveAs(filePath);

            return filePath;
        }
    }
}
