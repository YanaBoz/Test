using Microsoft.EntityFrameworkCore;
using Test.Core.Interfaces;

namespace Test.Infrastructure.Data.Repositories
{
    // Общий репозиторий для CRUD-операций над сущностями EF Core.
    // Реализует базовые методы IRepository<T>.
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DBContext _context; // EF DbContext
        protected readonly DbSet<T> _dbSet;    // DbSet соответствующей сущности

        public Repository(DBContext context)
        {
            // Проверяем, что контекст передан
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        // Получение сущности по первичному ключу (id).
        // FindAsync использует кэш изменения (change tracker) если сущность уже загружена.
        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            await _dbSet.FindAsync(new object[] { id }, cancellationToken);

        // Получение всех сущностей из таблицы.
        // В продакшн-решениях будьте осторожны с этим методом для больших таблиц.
        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _dbSet.ToListAsync(cancellationToken);

        // Добавление новой сущности в контекст (не выполняет сохранение в БД).
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
            await _dbSet.AddAsync(entity, cancellationToken);

        // Обновление сущности. Просто помечаем как изменённую — SaveChangesAsync снимет изменения.
        public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        // Удаление сущности из контекста.
        public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
