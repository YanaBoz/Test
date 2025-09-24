namespace Test.Infrastructure.Data.Repositories
{
    public class OperClassRepository : IOperClassRepository
    {
        private readonly DBContext _context;

        public OperClassRepository(DBContext context)
        {
            _context = context;
        }

        public async Task<string> GetClassNameAsync(int classId)
        {
            var operClass = await _context.OperClasses
                .FirstOrDefaultAsync(t => t.Id == classId);

            return operClass?.Name ?? classId.ToString();
        }
    }
}
