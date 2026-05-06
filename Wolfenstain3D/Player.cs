using System; // Потрібно для MathF.Sin і MathF.Cos
using System.Drawing; // Потрібно для Graphics, Font, Brush, Color
using System.Windows.Forms; // Потрібно для Keys

namespace Wolfenstain3D
{
    internal class Player
    {
        public float X { get; private set; } // Позиція гравця по горизонталі
        public float Y { get; private set; } // Позиція гравця по вертикалі
        public float Angle { get; private set; } // Кут, куди дивиться гравець
        private const float CollisionRadius = 12f; // Радіус гравця для перевірки зіткнень зі стінами

        public Player(float startX, float startY, float startAngle)
        {
            X = startX; // Початкова X-позиція
            Y = startY; // Початкова Y-позиція
            Angle = startAngle; // Початковий напрямок погляду
        }

        public void Update(InputController input, GameMap map)
        {
            float moveSpeed = 2.5f; // Швидкість руху гравця
            float rotationSpeed = 0.05f; // Швидкість повороту гравця

            if (input.IsKeyPressed(Keys.A)) // Перевіряємо, чи натиснута клавіша A для повороту вліво
            {
                Angle -= rotationSpeed; // Повертаємо гравця вліво
            }

            if (input.IsKeyPressed(Keys.D)) // Перевіряємо, чи натиснута клавіша D для повороту вправо
            {
                Angle += rotationSpeed; // Повертаємо гравця вправо
            }

            if (input.IsKeyPressed(Keys.W)) // Перевіряємо, чи натиснута клавіша W для руху вперед
            {
                TryMove(MathF.Cos(Angle) * moveSpeed, MathF.Sin(Angle) * moveSpeed, map); // Рух вперед
            }

            if (input.IsKeyPressed(Keys.S)) // Перевіряємо, чи натиснута клавіша S для руху назад
            {
                TryMove(-MathF.Cos(Angle) * moveSpeed, -MathF.Sin(Angle) * moveSpeed, map); // Рух назад
            }

            if (input.IsKeyPressed(Keys.Q)) // Перевіряємо, чи натиснута клавіша Q для руху боком вліво
            {
                float strafeAngle = Angle - MathF.PI / 2; // Кут для руху вліво
                TryMove(MathF.Cos(strafeAngle) * moveSpeed, MathF.Sin(strafeAngle) * moveSpeed, map); // Рух вліво
            }

            if (input.IsKeyPressed(Keys.E)) // Перевіряємо, чи натиснута клавіша E для руху боком вправо
            {
                float strafeAngle = Angle + MathF.PI / 2; // Кут для руху вправо
                TryMove(MathF.Cos(strafeAngle) * moveSpeed, MathF.Sin(strafeAngle) * moveSpeed, map); // Рух вправо
            }
        }

        private void TryMove(float deltaX, float deltaY, GameMap map)
        {
            float nextX = X + deltaX; // Нова можлива X-позиція
            float nextY = Y + deltaY; // Нова можлива Y-позиція

            if (!TouchesWall(nextX, nextY, map))
            {
                X = nextX; // Рухаємося тільки якщо гравець не торкається стіни
                Y = nextY; // Рухаємося тільки якщо гравець не торкається стіни
            }
        }

        private bool TouchesWall(float x, float y, GameMap map)
        {
            float leftX = x - CollisionRadius; // Ліва межа кола гравця
            float rightX = x + CollisionRadius; // Права межа кола гравця
            float topY = y - CollisionRadius; // Верхня межа кола гравця
            float bottomY = y + CollisionRadius; // Нижня межа кола гравця

            return map.IsBlocking(leftX, topY)
                || map.IsBlocking(rightX, topY)
                || map.IsBlocking(leftX, bottomY)
                || map.IsBlocking(rightX, bottomY); // Перевіряємо 4 кути кола гравця відносно дверей і стін
        }

        public void RenderDebugInfo(Graphics graphics, int x, int y)
        {
            using Font font = new Font("Segoe UI", 12, FontStyle.Regular); // Шрифт для компактного debug-тексту
            using Brush brush = new SolidBrush(Color.LightGreen); // Колір debug-тексту

            graphics.DrawString($"X: {X:0.0}", font, brush, x, y); // Показуємо X з одним знаком після коми
            graphics.DrawString($"Y: {Y:0.0}", font, brush, x, y + 24); // Показуємо Y з одним знаком після коми
            graphics.DrawString($"Angle: {Angle:0.00}", font, brush, x, y + 48); // Показуємо кут у радіанах
        }

        public void RenderMiniMap(Graphics graphics, int mapX, int mapY, float miniMapScale)
        {
            using Brush playerBrush = new SolidBrush(Color.Red); // Колір гравця на міні-мапі
            using Pen directionPen = new Pen(Color.Yellow, 2); // Лінія напрямку погляду

            float playerMapX = mapX + X * miniMapScale; // Позиція гравця на міні-мапі по X
            float playerMapY = mapY + Y * miniMapScale; // Позиція гравця на міні-мапі по Y

            float radiusOnMiniMap = CollisionRadius * miniMapScale; // Радіус гравця на міні-мапі
            graphics.FillEllipse(
                playerBrush,
                playerMapX - radiusOnMiniMap,
                playerMapY - radiusOnMiniMap,
                radiusOnMiniMap * 2,
                radiusOnMiniMap * 2); // Малюємо гравця з радіусом

            float directionLength = 25; // Довжина лінії напрямку
            float directionX = playerMapX + MathF.Cos(Angle) * directionLength; // Кінець лінії напрямку по X
            float directionY = playerMapY + MathF.Sin(Angle) * directionLength; // Кінець лінії напрямку по Y

            graphics.DrawLine(directionPen, playerMapX, playerMapY, directionX, directionY); // Малюємо напрямок погляду
        }
    }
}