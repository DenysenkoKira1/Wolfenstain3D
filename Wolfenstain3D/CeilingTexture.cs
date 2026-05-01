using System; // Потрібно для Math.Clamp
using System.Drawing; // Потрібно для Bitmap і Color

namespace Wolfenstain3D
{
    internal class CeilingTexture
    {
        public const int Size = 64; // Розмір текстури: 64 на 64 пікселі

        private readonly Bitmap _bitmap; // Готова картинка текстури стелі

        public CeilingTexture()
        {
            _bitmap = new Bitmap(Size, Size); // Створюємо квадратну текстуру стелі
            GenerateDarkGrayTexture(); // Малюємо просту темно-сіру стелю, як в оригінальному Wolfenstein
        }

        public Color GetPixel(int x, int y)
        {
            x = Math.Clamp(x, 0, Size - 1); // Захищаємо X від виходу за межі текстури
            y = Math.Clamp(y, 0, Size - 1); // Захищаємо Y від виходу за межі текстури

            return _bitmap.GetPixel(x, y); // Повертаємо колір конкретного пікселя стелі
        }

        private void GenerateDarkGrayTexture()
        {
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    int noise = ((x * 7 + y * 5) % 5) - 2; // Дуже легка зернистість, щоб стеля не була абсолютно плоскою
                    int gray = 49 + noise; // Темно-сірий тон, близький до стелі на референсі

                    _bitmap.SetPixel(x, y, Color.FromArgb(gray, gray, gray)); // Записуємо піксель графітово-сірої стелі
                }
            }
        }
    }
}