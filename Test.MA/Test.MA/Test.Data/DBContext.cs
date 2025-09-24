using Microsoft.EntityFrameworkCore;
using Test.Core.Models;

namespace Test.Data
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Turnover> Turnover { get; set; } = default!;
        public DbSet<Group> Groups { get; set; } = default!;
        public DbSet<OperClass> OperClasses { get; set; } = default!;
        public DbSet<FileModel> Files { get; set; }
    }
}
