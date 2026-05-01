using System; // Потрібно для Math.Clamp
using System.Drawing; // Потрібно для Bitmap і Color

namespace Wolfenstain3D
{
    internal class FloorTexture
    {
        public const int Size = 64; // Розмір текстури: 64 на 64 пікселі

        private readonly Bitmap _bitmap; // Готова картинка підлоги

        public FloorTexture()
        {
            _bitmap = new Bitmap(Size, Size); // Створюємо квадратну текстуру підлоги
            GeneratePlainGrayFloor(); // Малюємо просту сіру підлогу, як на референсі
        }

        public Color GetPixel(int x, int y)
        {
            x = Math.Clamp(x, 0, Size - 1); // Захищаємо X від виходу за межі картинки
            y = Math.Clamp(y, 0, Size - 1); // Захищаємо Y від виходу за межі картинки

            return _bitmap.GetPixel(x, y); // Повертаємо колір конкретного пікселя підлоги
        }

        private void GeneratePlainGrayFloor()
        {
            Color floorColor = Color.FromArgb(105, 105, 105); // Сірий колір підлоги, близький до скріну Wolfenstein 3D

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    _bitmap.SetPixel(x, y, floorColor); // Уся підлога одного сірого кольору без текстури
                }
            }
        }
    }
}