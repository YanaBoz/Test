namespace Test.Core.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
        Task<byte[]> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
