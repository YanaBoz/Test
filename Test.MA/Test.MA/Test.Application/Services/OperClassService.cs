using Microsoft.EntityFrameworkCore;
using Test.Application.Interfaces;
using Test.; // или ваш namespace для DBContext

namespace Test.Application.Services
{
    public class OperClassService : IOperClassService
    {
        private readonly DBContext _context;

        public OperClassService(DBContext context)
        {
            _context = context;
        }

        public async Task<string> GetClassNameAsync(int classId)
        {
            var operClass = await _context.Oper_Classes
                .FirstOrDefaultAsync(t => t.Id == classId);

            return operClass?.Name ?? classId.ToString();
        }
    }
}