using Microsoft.EntityFrameworkCore;
using Test.Core.Models;

namespace Test.Infrastructure.Data.Repositories
{
    // Специализированный репозиторий для сущности Turnover.
    // Добавляет метод для выборки по диапазону счетов.
    public class TurnoverRepository : Repository<Turnover>
    {
        public TurnoverRepository(DBContext context) : base(context) { }

        // Возвращает записи Turnover, у которых поле Account находится в заданном диапазоне [start, end].
        // Используется асинхронный ToListAsync для выполнения запроса на стороне БД.
        public async Task<IEnumerable<Turnover>> GetByAccountRangeAsync(int start, int end, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(t => t.Account >= start && t.Account <= end).ToListAsync(cancellationToken);
        }
    }
}
