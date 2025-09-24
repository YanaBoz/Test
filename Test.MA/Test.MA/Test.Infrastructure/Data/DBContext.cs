using Microsoft.EntityFrameworkCore;
using Test.Core.Models;

namespace Test.Infrastructure.Data
{
    // Контекст базы данных EF Core. Содержит DbSet для каждой используемой сущности.
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        // DbSet'ы для таблиц. default! — чтобы избежать предупреждений nullable.
        public DbSet<Turnover> Turnover { get; set; } = default!;
        public DbSet<Group> Groups { get; set; } = default!;
        public DbSet<OperClass> OperClasses { get; set; } = default!;
        public DbSet<FileModel> Files { get; set; } = default!;
    }
}
