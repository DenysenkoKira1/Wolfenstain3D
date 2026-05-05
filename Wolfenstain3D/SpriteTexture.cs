using System; // Потрібно для AppContext, Math
using System.Drawing; // Потрібно для Bitmap, Color, Graphics, Rectangle
using System.Drawing.Drawing2D; // Потрібно для InterpolationMode
using System.Drawing.Imaging; // Потрібно для PixelFormat, BitmapData, ImageLockMode
using System.IO; // Потрібно для Path і File
using System.Runtime.InteropServices; // Потрібно для швидкого читання пікселів з Bitmap

namespace Wolfenstain3D
{
    internal sealed class SpriteTexture
    {
        private readonly int[] _pixels; // Пікселі PNG у форматі ARGB, щоб не використовувати повільний GetPixel під час гри

        private SpriteTexture(int width, int height, int[] pixels)
        {
            Width = width; // Ширина текстури
            Height = height; // Висота текстури
            _pixels = pixels; // Збережений масив пікселів
        }

        public int Width { get; } // Ширина PNG
        public int Height { get; } // Висота PNG

        public static SpriteTexture FromAssets(string fileName)
        {
            return FromAssets(fileName, 0); // Завантажуємо PNG без зменшення
        }

        public static SpriteTexture FromAssets(string fileName, int maxHeight)
        {
            string assetPath = Path.Combine(AppContext.BaseDirectory, "Assets", fileName); // Основний шлях після запуску з bin

            if (!File.Exists(assetPath))
            {
                assetPath = Path.Combine("Assets", fileName); // Резервний шлях під час запуску з папки проєкту
            }

            return Load(assetPath, maxHeight); // Завантажуємо PNG у масив пікселів
        }

        public int GetArgb(int x, int y)
        {
            x = Math.Clamp(x, 0, Width - 1); // Захист від виходу за межі по X
            y = Math.Clamp(y, 0, Height - 1); // Захист від виходу за межі по Y

            return _pixels[y * Width + x]; // Повертаємо готовий ARGB-піксель
        }

        private static SpriteTexture Load(string path, int maxHeight)
        {
            using Bitmap source = new Bitmap(path); // Відкриваємо оригінальний PNG з прозорістю
            Rectangle sourceBounds = FindOpaqueBounds(source); // Беремо тільки реальну непрозору частину спрайта, без зайвого прозорого поля
            float scale = maxHeight > 0 && sourceBounds.Height > maxHeight
                ? (float)maxHeight / sourceBounds.Height
                : 1f; // Зменшуємо великі PNG до робочого розміру один раз під час завантаження
            int targetWidth = Math.Max(1, (int)MathF.Round(sourceBounds.Width * scale)); // Ширина оптимізованої текстури
            int targetHeight = Math.Max(1, (int)MathF.Round(sourceBounds.Height * scale)); // Висота оптимізованої текстури

            using Bitmap bitmap = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb); // Bitmap у стабільному ARGB-форматі
            using Graphics graphics = Graphics.FromImage(bitmap); // Graphics потрібен для crop + scale
            graphics.Clear(Color.Transparent); // Зберігаємо прозорий фон
            graphics.InterpolationMode = scale < 1f ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor; // Якісно стискаємо великі спрайти
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality; // Робимо масштабування акуратнішим
            graphics.DrawImage(source, new Rectangle(0, 0, targetWidth, targetHeight), sourceBounds, GraphicsUnit.Pixel); // Копіюємо тільки видиму частину спрайта

            int[] pixels = new int[bitmap.Width * bitmap.Height]; // Масив під усі пікселі
            Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height); // Область усього PNG
            BitmapData data = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb); // Швидкий доступ до пам'яті bitmap

            try
            {
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length); // Копіюємо всі пікселі одним блоком
            }
            finally
            {
                bitmap.UnlockBits(data); // Завжди розблоковуємо bitmap
            }

            return new SpriteTexture(bitmap.Width, bitmap.Height, pixels); // Повертаємо готову оптимізовану текстуру
        }

        private static Rectangle FindOpaqueBounds(Bitmap source)
        {
            int minX = source.Width; // Ліва межа непрозорої частини
            int minY = source.Height; // Верхня межа непрозорої частини
            int maxX = -1; // Права межа непрозорої частини
            int maxY = -1; // Нижня межа непрозорої частини

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    if (source.GetPixel(x, y).A <= 20)
                    {
                        continue; // Повністю прозорий фон не враховуємо
                    }

                    minX = Math.Min(minX, x); // Оновлюємо ліву межу
                    minY = Math.Min(minY, y); // Оновлюємо верхню межу
                    maxX = Math.Max(maxX, x); // Оновлюємо праву межу
                    maxY = Math.Max(maxY, y); // Оновлюємо нижню межу, де якраз знаходяться ступні
                }
            }

            if (maxX < minX || maxY < minY)
            {
                return new Rectangle(0, 0, source.Width, source.Height); // Якщо раптом PNG порожній, беремо все зображення
            }

            return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1); // Повертаємо точну область видимого спрайта
        }
    }
}
