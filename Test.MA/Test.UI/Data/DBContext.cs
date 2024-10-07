using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Test.UI.Models;

namespace Test.UI.Data
{
    public class DBContext : DbContext
    {
        public DBContext (DbContextOptions<DBContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Test.UI.Models.Turnover> Turnover { get; set; } = default!;
        public DbSet<Test.UI.Models.Group> Groups { get; set; } = default!;
        public DbSet<Test.UI.Models.Oper_Class> Oper_Classes { get; set; } = default!;
        public DbSet<Test.UI.Models.FileModel> Files { get; set; }
    }
}
