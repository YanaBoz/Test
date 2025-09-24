using Test.Application.Interfaces;
using Test.Core.Models;

namespace Test.UI.ViewModels
{
    // ViewModel, которая использует ITurnoverService для построения представления на основе сырой коллекции Turnover.
    // Обратите внимание: данный класс не является "традиционным" ViewModel, т.к. содержит сервисы — это ближе к helper/service внутри UI.
    public class MyViewModel
    {
        private readonly ITurnoverService _turnoverService;

        // Внедряем зависимость — сервис для обработки оборотов
        public MyViewModel(ITurnoverService turnoverService)
        {
            _turnoverService = turnoverService;
        }

        // Асинхронный метод, который преобразует список Turnover в список Constructor (готовый для отображения)
        public async Task<List<Constructor>> PrintAsync(List<Turnover> data)
        {
            // Валидация входных данных
            if (data == null || !data.Any())
                return new List<Constructor>();

            try
            {
                // Делегируем обработку TurnoverService (который группирует/суммирует данные)
                var result = await _turnoverService.ProcessTurnoverDataAsync(data);
                return result;
            }
            catch (Exception ex)
            {
                // Логируем ошибку в консоль (в продакшене использовать ILogger)
                Console.WriteLine($"Ошибка при обработке данных: {ex.Message}");
                throw; // Пробрасываем исключение выше — вызывающий код должен его обработать
            }
        }
    }
}
