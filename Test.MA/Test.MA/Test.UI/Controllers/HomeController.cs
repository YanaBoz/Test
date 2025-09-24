using Microsoft.AspNetCore.Mvc;
using Test.Application.DTOs;
using Test.Application.Services.Interfaces;

namespace Test.UI.Controllers
{
    // Контроллер для главной страницы и операций с файлами (загрузка, список, скачивание, объединение).
    public class HomeController : Controller
    {
        private readonly IFileProcessingService _fileService;

        // Внедряем сервис, который инкапсулирует логику загрузки/обработки/получения файлов
        public HomeController(IFileProcessingService fileService)
        {
            _fileService = fileService;
        }

        // GET: /Home/Index
        // Показывает страницу загрузки файла.
        public IActionResult Index() => View();

        // POST: /Home/Upload
        // Принимает IFormFile (загруженный файл из формы) и обрабатывает его.
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            // Базовая валидация: файл обязателен и не пустой
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Файл обязателен.");
                return View("Index");
            }

            // Передаём файл в сервис (внутри сервис сохранит файл, распарсит Excel и сохранит данные)
            var dto = await _fileService.UploadAndProcessFileAsync(file);

            // Если данных нет — возвращаем модель с ошибкой валидации
            if (!dto.Any())
            {
                ModelState.AddModelError("", "Данные не найдены в файле.");
                return View("Index");
            }

            // Если всё успешно — показываем страницу результата и передаём DTO
            return View("Result", dto);
        }

        // GET: /Home/Files
        // Возвращает список ранее загруженных файлов (FileDto)
        public async Task<IActionResult> Files()
        {
            var files = await _fileService.GetFilesAsync();
            return View(files);
        }

        // GET: /Home/Load_R?fileId=1
        // Отображает обработанные данные конкретного файла.
        public async Task<IActionResult> Load_R(int fileId)
        {
            var dto = await _fileService.GetFileDataAsync(fileId);
            if (!dto.Any()) return NotFound(); // Если данных нет — 404
            return View("Result", dto);
        }

        // POST: /Home/MergeFiles
        // Объединяет все файлы в один Excel и отправляет его пользователю.
        [HttpPost]
        public async Task<IActionResult> MergeFiles()
        {
            try
            {
                // Сервис формирует файл и возвращает путь к нему на диске
                var mergedFilePath = await _fileService.MergeAllFilesAsync();

                // Читаем файл в память для отправки клиенту
                var fileBytes = await System.IO.File.ReadAllBytesAsync(mergedFilePath);
                var fileName = System.IO.Path.GetFileName(mergedFilePath);

                // Возвращаем файл как response (Content-Type для .xlsx)
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                // При ошибке добавляем сообщение в ModelState и перенаправляем на страницу списка файлов
                // (в редких случаях можно показать более дружелюбную страницу с деталями ошибки)
                ModelState.AddModelError("", ex.Message);
                return RedirectToAction("Files");
            }
        }

        // GET: /Home/DownloadFile?fileId=1
        // Скачивание конкретного файла (созданного сервисом). Возвращает 404 при ошибке.
        public async Task<IActionResult> DownloadFile(int fileId)
        {
            try
            {
                var filePath = await _fileService.DownloadFileAsync(fileId);
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = System.IO.Path.GetFileName(filePath);

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch
            {
                // В целях безопасности не возвращаем текст ошибки клиенту — просто 404
                return NotFound();
            }
        }
    }
}
