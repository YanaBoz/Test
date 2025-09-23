using Test.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace Test.Application.Services.Interfaces
{
    public interface IFileProcessingService
    {
        Task<List<ConstructorDto>> UploadAndProcessFileAsync(IFormFile file);
        Task<List<FileDto>> GetFilesAsync();
        Task<List<ConstructorDto>> GetFileDataAsync(int fileId);
        Task<string> MergeAllFilesAsync();
        Task<string> DownloadFileAsync(int fileId);
    }
}
