using Test.Core.Models;

namespace Test.Core.Interfaces
{
    public interface IUnitOfWork
    {
        IRepository<Turnover> TurnoverRepository { get; }
        IRepository<Group> GroupRepository { get; }
        IRepository<FileModel> FileRepository { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
