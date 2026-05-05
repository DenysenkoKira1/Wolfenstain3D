using System; // Потрібно для Math.Clamp
using System.Drawing; // Потрібно для Bitmap, Color, Graphics, Pen, Brush, Point

namespace Wolfenstain3D
{
    internal class DoorTexture
    {
        public const int Size = 64; // Розмір текстури: 64 на 64 пікселі

        private readonly Color[] _pixels = new Color[Size * Size]; // Пікселі текстури дверей у швидкому масиві

        public DoorTexture()
        {
            using Bitmap bitmap = new Bitmap(Size, Size); // Тимчасова картинка тільки для генерації малюнка

            GenerateTurquoiseDoorTexture(bitmap); // Малюємо бірюзові двері із золотою емблемою по центру
            CopyPixelsToArray(bitmap); // Один раз копіюємо Bitmap у масив, щоб у грі не викликати GetPixel
        }

        public Color GetPixel(int x, int y)
        {
            x = Math.Clamp(x, 0, Size - 1); // Захищаємо X від виходу за межі текстури
            y = Math.Clamp(y, 0, Size - 1); // Захищаємо Y від виходу за межі текстури

            return _pixels[y * Size + x]; // Швидко повертаємо колір із масиву
        }

        private static void GenerateTurquoiseDoorTexture(Bitmap bitmap)
        {
            using Graphics graphics = Graphics.FromImage(bitmap); // Дозволяє малювати прямо на тимчасовий Bitmap
            using Brush baseBrush = new SolidBrush(Color.FromArgb(18, 150, 158)); // Основний бірюзовий метал
            using Brush darkBrush = new SolidBrush(Color.FromArgb(8, 72, 82)); // Темні тіні в пазах і рамці
            using Brush lightBrush = new SolidBrush(Color.FromArgb(55, 215, 220)); // Світлі бірюзові відблиски
            using Brush panelBrush = new SolidBrush(Color.FromArgb(20, 175, 184)); // Трохи світліші панелі дверей
            using Brush goldBrush = new SolidBrush(Color.FromArgb(218, 166, 36)); // Основний золотий колір емблеми
            using Brush goldLightBrush = new SolidBrush(Color.FromArgb(255, 224, 92)); // Світлий блік на емблемі
            using Brush goldDarkBrush = new SolidBrush(Color.FromArgb(126, 82, 18)); // Темна тінь золотої емблеми
            using Pen darkPen = new Pen(Color.FromArgb(5, 45, 52), 2); // Темні контури металу
            using Pen lightPen = new Pen(Color.FromArgb(83, 240, 238), 1); // Світлі тонкі краї
            using Pen goldPen = new Pen(Color.FromArgb(255, 230, 96), 1); // Світлий контур золотої емблеми
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

            DrawGoldEmblem(graphics, goldBrush, goldLightBrush, goldDarkBrush, goldPen, darkPen); // Малюємо центральну золоту емблему замість ручки
        }

        private static void DrawGoldEmblem(Graphics graphics, Brush goldBrush, Brush goldLightBrush, Brush goldDarkBrush, Pen goldPen, Pen darkPen)
        {
            Point[] shadowDiamond =
            {
                new Point(33, 22), // Верхня точка тіні емблеми
                new Point(44, 33), // Права точка тіні емблеми
                new Point(33, 44), // Нижня точка тіні емблеми
                new Point(22, 33) // Ліва точка тіні емблеми
            };

            Point[] mainDiamond =
            {
                new Point(32, 20), // Верхня точка золотої емблеми
                new Point(45, 32), // Права точка золотої емблеми
                new Point(32, 45), // Нижня точка золотої емблеми
                new Point(19, 32) // Ліва точка золотої емблеми
            };

            graphics.FillPolygon(goldDarkBrush, shadowDiamond); // Темна золота тінь робить емблему об'ємною
            graphics.FillPolygon(goldBrush, mainDiamond); // Основна золота форма емблеми
            graphics.DrawPolygon(darkPen, mainDiamond); // Темний контур емблеми
            graphics.DrawPolygon(goldPen, new[] { new Point(32, 22), new Point(42, 32), new Point(32, 42), new Point(22, 32) }); // Світлий внутрішній контур

            Point[] star =
            {
                new Point(32, 25), // Верхній промінь зірки
                new Point(34, 30),
                new Point(39, 30),
                new Point(35, 34),
                new Point(37, 39),
                new Point(32, 36),
                new Point(27, 39),
                new Point(29, 34),
                new Point(25, 30),
                new Point(30, 30)
            };

            graphics.FillPolygon(goldLightBrush, star); // Світла зірка всередині золотої емблеми
            graphics.DrawPolygon(darkPen, star); // Темний контур зірки, щоб вона читалась у низькій роздільності
        }

        private void CopyPixelsToArray(Bitmap bitmap)
        {
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    _pixels[y * Size + x] = bitmap.GetPixel(x, y); // Копіюємо піксель один раз під час створення текстури
                }
            }
        }
    }
}