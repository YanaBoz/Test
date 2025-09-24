using Test.Core.Interfaces;
using Test.Core.Models;
using Test.Infrastructure.Data.Repositories;

namespace Test.Infrastructure.Data
{
    // Реализация шаблона Unit of Work.
    // Инкапсулирует доступ к репозиториям и предоставляет SaveChangesAsync.
    // Реализует IDisposable и IAsyncDisposable для корректного освобождения DbContext.
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable, IDisposable
    {
        private readonly DBContext _context;

        // Lazy создание репозиториев — создаются при первом обращении
        private Repository<Turnover>? _turnoverRepository;
        private Repository<Group>? _groupRepository;
        private Repository<OperClass>? _operClassRepository;
        private Repository<FileModel>? _fileRepository;

        public UnitOfWork(DBContext context)
        {
            _context = context;
        }

        // Свойства возвращают IRepository<T> и инициализируют соответствующий репозиторий при необходимости.
        public IRepository<Turnover> TurnoverRepository => _turnoverRepository ??= new Repository<Turnover>(_context);
        public IRepository<Group> GroupRepository => _groupRepository ??= new Repository<Group>(_context);
        public IRepository<OperClass> OperClassRepository => _operClassRepository ??= new Repository<OperClass>(_context);
        public IRepository<FileModel> FileRepository => _fileRepository ??= new Repository<FileModel>(_context);

        // Сохранение всех изменений в БД.
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            await _context.SaveChangesAsync(cancellationToken);

        // Асинхронное освобождение контекста
        public ValueTask DisposeAsync()
        {
            return _context.DisposeAsync();
        }

        // Синхронное освобождение контекста
        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
