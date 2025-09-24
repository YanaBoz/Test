using System.Threading.Tasks;

namespace Test.Application.Interfaces
{
    public interface IOperClassService
    {
        Task<string> GetClassNameAsync(int classId);
    }
}