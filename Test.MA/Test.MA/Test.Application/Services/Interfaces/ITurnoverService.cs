using Test.Core.Models;

namespace Test.Application.Interfaces
{
    public interface ITurnoverService
    {
        Task<List<Constructor>> ProcessTurnoverDataAsync(IEnumerable<Turnover> data);
    }
}
