using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Infrastructure.Data
{
    public interface IOperClassRepository
    {
        Task<string> GetClassNameAsync(int classId);
    }
}
