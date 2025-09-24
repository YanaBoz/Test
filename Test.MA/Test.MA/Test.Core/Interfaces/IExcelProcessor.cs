using Test.Core.Models;

namespace Test.Core.Interfaces
{
    public interface IExcelProcessor
    {
        Task<List<Turnover>> ProcessExcelFileAsync(string filePath, CancellationToken cancellationToken = default);

        Task<string> GenerateReportAsync(List<Constructor> data, CancellationToken cancellationToken = default);
    }
}
