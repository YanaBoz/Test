using Test.MA.Models;

namespace Test.MA.Interfaces
{
    public interface IStrokaGenerator
    {
        List<Stroka> Generate(int count);
    }
}