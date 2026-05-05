using System; // Потрібно для Math.Clamp
using System.Drawing; // Потрібно для Color

namespace Wolfenstain3D
{
    internal class FloorTexture
    {
        public const int Size = 64; // Розмір текстури: 64 на 64 пікселі

        private readonly Color[] _pixels = new Color[Size * Size]; // Пікселі підлоги у швидкому масиві

        public FloorTexture()
        {
            GeneratePlainGrayFloor(); // Малюємо просту сіру підлогу, як на референсі
        }

        public Color GetPixel(int x, int y)
        {
            x = Math.Clamp(x, 0, Size - 1); // Захищаємо X від виходу за межі картинки
            y = Math.Clamp(y, 0, Size - 1); // Захищаємо Y від виходу за межі картинки

            return _pixels[y * Size + x]; // Швидко повертаємо колір із масиву
        }

        private void GeneratePlainGrayFloor()
        {
            Color floorColor = Color.FromArgb(105, 105, 105); // Сірий колір підлоги, близький до скріну Wolfenstein 3D

            for (int i = 0; i < _pixels.Length; i++)
            {
                _pixels[i] = floorColor; // Уся підлога одного сірого кольору без текстури
            }
        }
    }
}