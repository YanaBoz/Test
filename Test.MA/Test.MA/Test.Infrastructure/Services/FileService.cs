using System.IO;
using System.Threading;
using Test.Core.Interfaces;

namespace Test.Infrastructure.Services
{
    // Сервис для работы с файловой системой: загрузка, скачивание, удаление.
    public class FileService : IFileService
    {
        private readonly string _basePath;

        // Если basePath не указан — по умолчанию используем wwwroot/Files
        public FileService(string basePath)
        {
            _basePath = string.IsNullOrWhiteSpace(basePath)
                ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files")
                : basePath;
        }

        // Сохраняет поток в файл и возвращает путь к нему.
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);

            var filePath = Path.Combine(_basePath, fileName);

            // Создаём FileStream и копируем поток. Обратите внимание на режим FileShare.None — файл блокируется на запись.
            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await fileStream.CopyToAsync(stream, cancellationToken);
            return filePath;
        }

        // Возвращает байты файла асинхронно
        public Task<byte[]> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default) =>
            File.ReadAllBytesAsync(filePath, cancellationToken);

        // Удаление файла, если он существует
        public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            return Task.CompletedTask;
        }
    }
}
