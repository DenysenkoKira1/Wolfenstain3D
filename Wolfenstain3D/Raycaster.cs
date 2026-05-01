using System; // Потрібно для MathF
using System.Drawing; // Потрібно для Graphics, Pen, Brush, Color, Rectangle, PointF

namespace Wolfenstain3D
{
    internal class Raycaster
    {
        private const int MiniMapRayCount = 31; // Кількість променів для debug-відображення на міні-мапі
        private const float FieldOfView = MathF.PI / 3; // Кут огляду 60 градусів
        private const float MaxRayDistance = 500f; // Максимальна довжина променя
        private const float RayStep = 2f; // Крок перевірки променя

        public void Render3D(Graphics graphics, Player player, GameMap map, Rectangle viewport)
        {
            using Brush ceilingBrush = new SolidBrush(Color.FromArgb(35, 35, 45)); // Колір стелі
            using Brush floorBrush = new SolidBrush(Color.FromArgb(25, 25, 25)); // Колір підлоги

            graphics.FillRectangle(ceilingBrush, viewport.X, viewport.Y, viewport.Width, viewport.Height / 2); // Малюємо стелю
            graphics.FillRectangle(floorBrush, viewport.X, viewport.Y + viewport.Height / 2, viewport.Width, viewport.Height / 2); // Малюємо підлогу

            float projectionDistance = viewport.Width / (2f * MathF.Tan(FieldOfView / 2f)); // Відстань до уявної площини проєкції
            float angleStep = FieldOfView / viewport.Width; // Один промінь на кожну вертикальну колонку екрана
            float startAngle = player.Angle - FieldOfView / 2f; // Початковий кут лівого краю огляду

            for (int column = 0; column < viewport.Width; column++)
            {
                float rayAngle = startAngle + column * angleStep; // Кут поточного променя
                RayHit hit = CastRay(player.X, player.Y, rayAngle, map); // Шукаємо точку зіткнення зі стіною або дверима

                float correctedDistance = hit.Distance * MathF.Cos(rayAngle - player.Angle); // Прибираємо ефект "риб’ячого ока"
                correctedDistance = MathF.Max(correctedDistance, 1f); // Захист від ділення на нуль

                float wallHeight = map.TileSize * projectionDistance / correctedDistance; // Висота стіни на екрані
                float wallTop = viewport.Y + (viewport.Height - wallHeight) / 2f; // Верхня точка стіни
                float wallBottom = wallTop + wallHeight; // Нижня точка стіни

                int shade = CalculateWallShade(correctedDistance); // Яскравість стіни залежно від відстані
                Color wallColor = hit.IsDoor
                    ? Color.FromArgb(shade, shade * 3 / 4, shade / 3) // Двері малюємо теплішим коричнево-золотим кольором
                    : Color.FromArgb(shade, shade, shade); // Звичайні стіни малюємо сірими

                using Brush wallBrush = new SolidBrush(wallColor); // Колір поточної колонки стіни
                graphics.FillRectangle(wallBrush, viewport.X + column, wallTop, 1, wallBottom - wallTop); // Малюємо одну вертикальну колонку стіни
            }
        }

        public void RenderMiniMapRays(Graphics graphics, Player player, GameMap map, int mapX, int mapY, float miniMapScale)
        {
            using Pen rayPen = new Pen(Color.FromArgb(140, Color.Yellow), 1); // Колір променів на міні-мапі

            float startAngle = player.Angle - FieldOfView / 2; // Початковий кут лівого променя
            float angleStep = FieldOfView / (MiniMapRayCount - 1); // Відстань між променями за кутом

            for (int i = 0; i < MiniMapRayCount; i++)
            {
                float rayAngle = startAngle + angleStep * i; // Поточний кут променя
                RayHit hit = CastRay(player.X, player.Y, rayAngle, map); // Точка, де промінь зустрів перешкоду

                float startX = mapX + player.X * miniMapScale; // Початок променя на міні-мапі по X
                float startY = mapY + player.Y * miniMapScale; // Початок променя на міні-мапі по Y
                float endX = mapX + hit.Point.X * miniMapScale; // Кінець променя на міні-мапі по X
                float endY = mapY + hit.Point.Y * miniMapScale; // Кінець променя на міні-мапі по Y

                graphics.DrawLine(rayPen, startX, startY, endX, endY); // Малюємо промінь
            }
        }

        private RayHit CastRay(float startX, float startY, float angle, GameMap map)
        {
            float rayX = startX; // Поточна X-позиція променя
            float rayY = startY; // Поточна Y-позиція променя

            for (float distance = 0; distance < MaxRayDistance; distance += RayStep)
            {
                rayX = startX + MathF.Cos(angle) * distance; // Рух променя по X
                rayY = startY + MathF.Sin(angle) * distance; // Рух променя по Y

                int tileX = (int)(rayX / map.TileSize); // Клітинка карти по X
                int tileY = (int)(rayY / map.TileSize); // Клітинка карти по Y

                if (map.IsBlocking(rayX, rayY))
                {
                    bool isDoor = map.IsDoor(tileX, tileY) && !map.IsDoorOpen(tileX, tileY); // Визначаємо, чи це ще не відкрита повністю дверна панель
                    return new RayHit(new PointF(rayX, rayY), distance, isDoor); // Повертаємо точку, відстань і тип перешкоди
                }
            }

            return new RayHit(new PointF(rayX, rayY), MaxRayDistance, false); // Якщо перешкоду не знайдено, повертаємо максимальну відстань
        }

        private static int CalculateWallShade(float distance)
        {
            int shade = 255 - (int)(distance * 0.35f); // Чим далі стіна, тим темніша
            return Math.Clamp(shade, 45, 220); // Обмежуємо яскравість, щоб стіни не зникали
        }

        private readonly struct RayHit
        {
            public RayHit(PointF point, float distance, bool isDoor)
            {
                Point = point; // Точка зіткнення променя зі стіною або дверима
                Distance = distance; // Відстань від гравця до перешкоди
                IsDoor = isDoor; // Чи є перешкода дверима
            }

            public PointF Point { get; } // Координата зіткнення
            public float Distance { get; } // Довжина променя до перешкоди
            public bool IsDoor { get; } // Ознака, що промінь влучив у двері
        }
    }
}