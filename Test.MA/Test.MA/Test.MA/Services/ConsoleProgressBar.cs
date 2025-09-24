namespace Test.MA.Services
{
    // Небольшой консольный прогресс-бар для вывода состояния обработки.
    public class ConsoleProgressBar
    {
        // Позиция левого верхнего угла прогресс-бара в консоли.
        public int Left { get; }
        public int Top { get; }
        // Длина "градации" прогресс-бара (количество символов внутри скобок).
        public int Length { get; }

        public ConsoleProgressBar(int left, int top, int length)
        {
            Left = left;
            Top = top;
            Length = length;
        }

        // progress: 0..100 (процент), count: текущее число обработанных элементов, total: общее число
        public void ShowProgress(int progress, long count, long total)
        {
            // Расчёт заполнения в символах (0..Length)
            int p = progress * Length / 100;

            // Сохраняем текущую позицию курсора, чтобы вернуть её в конце
            (int left, int top) = Console.GetCursorPosition();

            // Перемещаем курсор в позицию прогресс-бара
            Console.SetCursorPosition(Left, Top);

            // Рисуем прогресс-бар:
            // - '█' и '▓' используются для заполненной части (в оригинале использовано деление на 2 — оставил логику),
            // - '░' — для незаполненной части.
            // Важно: деление p/2 + p%2 приводит к тому, что первая часть рисуется полублоками — это стилистика.
            Console.Write($"[{new string('█', p / 2)}" +
                $"{new string('▓', p / 2 + p % 2)}" +
                $"{new string('░', Length - p)}] {count}");

            // Позиционируемся для вывода "/{total}" — в оригинале используются абсолютные смещения (left + 85).
            // Это хак — можно заменить на вычисление на основе Left+Length. Оставил текущую логику, но рекомендую заменить.
            Console.SetCursorPosition(left + 85, top);
            Console.Write($"/{total}");

            // Возвращаем курсор на прежнюю позицию, чтобы не сломать дальнейший вывод.
            Console.SetCursorPosition(left, top);
        }
    }
}
