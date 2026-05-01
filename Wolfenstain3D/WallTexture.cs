using System; // Потрібно для Math.Clamp
using System.Drawing; // Потрібно для Bitmap, Color, Graphics, Pen

namespace Wolfenstain3D
{
    internal class WallTexture
    {
        public const int Size = 64; // Розмір текстури: 64 на 64 пікселі

        private readonly Bitmap _bitmap; // Готова картинка текстури стіни

        public WallTexture()
        {
            _bitmap = new Bitmap(Size, Size); // Створюємо квадратну текстуру
            GenerateBrickTexture(); // Одразу генеруємо світло-сірий кам'яний малюнок
        }

        public Color GetPixel(int x, int y)
        {
            x = Math.Clamp(x, 0, Size - 1); // Захищаємо X від виходу за межі текстури
            y = Math.Clamp(y, 0, Size - 1); // Захищаємо Y від виходу за межі текстури

            return _bitmap.GetPixel(x, y); // Повертаємо колір конкретного пікселя текстури
        }

        private void GenerateBrickTexture()
        {
            using Graphics graphics = Graphics.FromImage(_bitmap); // Дозволяє малювати прямо на Bitmap

            graphics.Clear(Color.FromArgb(175, 175, 175)); // Основний світло-сірий колір каменю

            using Pen darkPen = new Pen(Color.FromArgb(75, 75, 75), 2); // Темні шви між блоками
            using Pen lightPen = new Pen(Color.FromArgb(225, 225, 225), 1); // Світлі верхні краї для об'єму
            using Pen noisePen = new Pen(Color.FromArgb(145, 145, 145), 1); // Дрібні нерівності каменю

            for (int y = 0; y <= Size; y += 16)
            {
                graphics.DrawLine(darkPen, 0, y, Size, y); // Горизонтальні шви між рядами блоків
            }

            for (int row = 0; row < 4; row++)
            {
                int offset = row % 2 == 0 ? 0 : 16; // Кожен другий ряд зміщений, як кам'яна кладка
                int y = row * 16; // Верхня координата поточного ряду

                for (int x = -offset; x <= Size; x += 32)
                {
                    graphics.DrawLine(darkPen, x, y, x, y + 16); // Вертикальний шов між блоками
                    graphics.DrawLine(lightPen, x + 3, y + 3, x + 27, y + 3); // Світла грань зверху блока
                }
            }

            for (int y = 6; y < Size; y += 13)
            {
                for (int x = 5; x < Size; x += 17)
                {
                    graphics.DrawLine(noisePen, x, y, x + 5, y); // Маленька тріщина/нерівність на камені
                }
            }
        }
    }
}