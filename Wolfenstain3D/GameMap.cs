using System; // Потрібно для MathF
using System.Drawing; // Потрібно для Graphics, Brush, Pen, Color

namespace Wolfenstain3D
{
    internal class GameMap
    {
        private readonly int[,] _tiles = // Масив карти: 1 - стіна, 0 - підлога, 2 - двері
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 1, 1, 0, 1, 2, 2, 1, 0, 1, 1, 0, 0, 1 },
            { 1, 0, 0, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        public int TileSize { get; } = 64; // Розмір однієї клітинки у світі гри
        public int Width => _tiles.GetLength(1); // Ширина карти в клітинках
        public int Height => _tiles.GetLength(0); // Висота карти в клітинках
        public float PlayerStartX => 8 * TileSize; // Стартова позиція гравця по X у центрі карти
        public float PlayerStartY => 8 * TileSize; // Стартова позиція гравця по Y у центрі карти
        public float PlayerStartAngle => -MathF.PI / 2f; // Гравець дивиться вгору

        public bool IsDoor(int tileX, int tileY)
        {
            if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height)
            {
                return false; // За межами карти дверей немає
            }

            return _tiles[tileY, tileX] == 2; // 2 означає двері
        }

        public void TryOpenDoor(float playerX, float playerY, float playerAngle)
        {
            float interactDistance = TileSize * 0.75f; // Наскільки далеко попереду гравець може відкрити двері

            float checkX = playerX + MathF.Cos(playerAngle) * interactDistance; // Точка перевірки попереду гравця по X
            float checkY = playerY + MathF.Sin(playerAngle) * interactDistance; // Точка перевірки попереду гравця по Y

            int tileX = (int)(checkX / TileSize); // Клітинка карти по X
            int tileY = (int)(checkY / TileSize); // Клітинка карти по Y

            if (IsDoor(tileX, tileY))
            {
                _tiles[tileY, tileX] = 0; // Якщо перед гравцем двері, відкриваємо їх
            }
        }

        public bool IsWall(int tileX, int tileY)
        {
            if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height)
            {
                return true; // За межами карти вважаємо стіною
            }

            return _tiles[tileY, tileX] == 1 || _tiles[tileY, tileX] == 2; // Стіни і закриті двері блокують рух
        }

        public void RenderMiniMap(Graphics graphics, int mapX, int mapY, int cellSize)
        {
            using Brush wallBrush = new SolidBrush(Color.FromArgb(90, 90, 90)); // Колір стін
            using Brush floorBrush = new SolidBrush(Color.FromArgb(25, 25, 25)); // Колір підлоги
            using Brush doorBrush = new SolidBrush(Color.DarkGoldenrod); // Колір дверей
            using Pen gridPen = new Pen(Color.FromArgb(60, Color.White)); // Лінії сітки

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Brush tileBrush;

                    if (IsDoor(x, y))
                    {
                        tileBrush = doorBrush; // Якщо це двері, малюємо двері
                    }
                    else if (IsWall(x, y))
                    {
                        tileBrush = wallBrush; // Якщо це стіна, малюємо стіну
                    }
                    else
                    {
                        tileBrush = floorBrush; // Інакше це підлога
                    }

                    int screenX = mapX + x * cellSize; // X клітинки на екрані
                    int screenY = mapY + y * cellSize; // Y клітинки на екрані

                    graphics.FillRectangle(tileBrush, screenX, screenY, cellSize, cellSize); // Малюємо клітинку
                    graphics.DrawRectangle(gridPen, screenX, screenY, cellSize, cellSize); // Малюємо межу клітинки
                }
            }
        }
    }
}