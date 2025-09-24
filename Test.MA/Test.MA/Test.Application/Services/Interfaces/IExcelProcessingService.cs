using Test.Core.Models;

namespace Test.Application.Interfaces
{
    public interface IExcelProcessingService
    {
        Task<List<Turnover>> ProcessUploadedFileAsync(Stream fileStream, string fileName);
    }
}
