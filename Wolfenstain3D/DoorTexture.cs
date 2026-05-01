using System; // Потрібно для Math.Clamp
using System.Drawing; // Потрібно для Bitmap, Color, Graphics, Pen, Brush, Rectangle

namespace Wolfenstain3D
{
    internal class DoorTexture
    {
        public const int Size = 64; // Розмір текстури: 64 на 64 пікселі

        private readonly Bitmap _bitmap; // Готова картинка текстури дверей

        public DoorTexture()
        {
            _bitmap = new Bitmap(Size, Size); // Створюємо квадратну текстуру дверей
            GenerateTurquoiseDoorTexture(); // Одразу малюємо бірюзові двері із золотою ручкою
        }

        public Color GetPixel(int x, int y)
        {
            x = Math.Clamp(x, 0, Size - 1); // Захищаємо X від виходу за межі текстури
            y = Math.Clamp(y, 0, Size - 1); // Захищаємо Y від виходу за межі текстури

            return _bitmap.GetPixel(x, y); // Повертаємо колір конкретного пікселя дверей
        }

        private void GenerateTurquoiseDoorTexture()
        {
            using Graphics graphics = Graphics.FromImage(_bitmap); // Дозволяє малювати прямо на Bitmap
            using Brush baseBrush = new SolidBrush(Color.FromArgb(18, 150, 158)); // Основний бірюзовий метал
            using Brush darkBrush = new SolidBrush(Color.FromArgb(8, 72, 82)); // Темні тіні в пазах і рамці
            using Brush lightBrush = new SolidBrush(Color.FromArgb(55, 215, 220)); // Світлі бірюзові відблиски
            using Brush panelBrush = new SolidBrush(Color.FromArgb(20, 175, 184)); // Трохи світліші панелі дверей
            using Brush goldBrush = new SolidBrush(Color.FromArgb(218, 166, 36)); // Основний золотий колір ручки
            using Brush goldLightBrush = new SolidBrush(Color.FromArgb(255, 220, 85)); // Світлий блік на ручці
            using Pen darkPen = new Pen(Color.FromArgb(5, 45, 52), 2); // Темні контури металу
            using Pen lightPen = new Pen(Color.FromArgb(83, 240, 238), 1); // Світлі тонкі краї
            using Pen scratchPen = new Pen(Color.FromArgb(12, 115, 125), 1); // Подряпини та технічні лінії

            graphics.Clear(Color.FromArgb(6, 42, 48)); // Темний фон рамки дверей
            graphics.FillRectangle(baseBrush, 6, 3, 52, 58); // Основне полотно дверей
            graphics.FillRectangle(darkBrush, 6, 3, 52, 4); // Верхня темна рамка
            graphics.FillRectangle(darkBrush, 6, 57, 52, 4); // Нижня темна рамка
            graphics.FillRectangle(darkBrush, 6, 3, 4, 58); // Ліва темна рамка
            graphics.FillRectangle(darkBrush, 54, 3, 4, 58); // Права темна рамка

            graphics.FillRectangle(panelBrush, 13, 10, 16, 18); // Верхня ліва металева панель
            graphics.FillRectangle(panelBrush, 35, 10, 16, 18); // Верхня права металева панель
            graphics.FillRectangle(panelBrush, 13, 36, 16, 17); // Нижня ліва металева панель
            graphics.FillRectangle(panelBrush, 35, 36, 16, 17); // Нижня права металева панель

            graphics.DrawRectangle(darkPen, 12, 9, 18, 20); // Контур верхньої лівої панелі
            graphics.DrawRectangle(darkPen, 34, 9, 18, 20); // Контур верхньої правої панелі
            graphics.DrawRectangle(darkPen, 12, 35, 18, 19); // Контур нижньої лівої панелі
            graphics.DrawRectangle(darkPen, 34, 35, 18, 19); // Контур нижньої правої панелі

            graphics.DrawLine(lightPen, 14, 11, 27, 11); // Світлий край верхньої лівої панелі
            graphics.DrawLine(lightPen, 36, 11, 49, 11); // Світлий край верхньої правої панелі
            graphics.DrawLine(lightPen, 14, 37, 27, 37); // Світлий край нижньої лівої панелі
            graphics.DrawLine(lightPen, 36, 37, 49, 37); // Світлий край нижньої правої панелі

            graphics.FillRectangle(lightBrush, 30, 7, 4, 50); // Вертикальний світлий металевий розділювач
            graphics.DrawLine(darkPen, 31, 7, 31, 57); // Темна тінь у центрі дверей
            graphics.DrawLine(scratchPen, 17, 16, 23, 16); // Маленька подряпина на верхній лівій панелі
            graphics.DrawLine(scratchPen, 40, 22, 47, 22); // Маленька подряпина на верхній правій панелі
            graphics.DrawLine(scratchPen, 18, 45, 25, 45); // Маленька подряпина на нижній лівій панелі
            graphics.DrawLine(scratchPen, 39, 43, 47, 43); // Маленька подряпина на нижній правій панелі

            graphics.FillEllipse(goldBrush, 45, 29, 8, 8); // Кругла золота основа ручки справа
            graphics.FillRectangle(goldBrush, 47, 32, 9, 3); // Золота ручка, витягнута вправо
            graphics.FillEllipse(goldLightBrush, 47, 30, 3, 3); // Світлий блік на золотій ручці
            graphics.DrawEllipse(darkPen, 45, 29, 8, 8); // Темний контур основи ручки
        }
    }
}