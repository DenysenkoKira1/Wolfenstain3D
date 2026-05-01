using System; // Потрібно для MathF
using System.Collections.Generic; // Потрібно для List<Door>
using System.Drawing; // Потрібно для Graphics, Brush, Pen, Color

namespace Wolfenstain3D
{
    internal class GameMap
    {
        private readonly int[,] _tiles; // Карта рівня у вигляді числових тайлів
        private readonly List<Door> _doors = new List<Door>(); // Усі двері рівня як окремі об'єкти

        public GameMap()
        {
            string[] layout =
            {
                "#########################",
                "#........#...###........#",
                "#........#...###........#",
                "#........#...##.........#",
                "#........##D####........#",
                "####D######.######D###D##",
                "#.....#........#....#...#",
                "#.....D........#....#...#",
                "#.....#........#....#...#",
                "#.....#..##....#....#...#",
                "#.....#..##....#....#...#",
                "###D########D#####D##...#",
                "#.....#........#........#",
                "#.....D........D........#",
                "#.....#....P...#........#",
                "#.....##......##........#",
                "#.....#........#........#",
                "###D#######D########D####",
                "#.......#......##.......#",
                "#.......#......##.......#",
                "#.......D.......D.......#",
                "#.......#......##.......#",
                "#########################"
            };

            _tiles = new int[layout.Length, layout[0].Length]; // Створюємо масив під усю карту

            bool playerStartFound = false; // Прапорець, що старт гравця знайдено на карті

            for (int y = 0; y < layout.Length; y++)
            {
                for (int x = 0; x < layout[y].Length; x++)
                {
                    char tile = layout[y][x]; // Поточний символ карти

                    switch (tile)
                    {
                        case '#':
                            _tiles[y, x] = 1; // Стіна
                            break;
                        case 'D':
                            _tiles[y, x] = 2; // Двері, для яких пізніше створимо окремий об'єкт
                            break;
                        case 'P':
                            _tiles[y, x] = 0; // Стартова клітинка гравця є підлогою
                            PlayerStartX = (x + 0.5f) * TileSize; // Ставимо гравця в центр клітинки
                            PlayerStartY = (y + 0.5f) * TileSize; // Ставимо гравця в центр клітинки
                            playerStartFound = true;
                            break;
                        default:
                            _tiles[y, x] = 0; // Усе інше вважаємо підлогою
                            break;
                    }
                }
            }

            CreateDoors(); // Після читання всієї карти створюємо двері з правильним напрямком відкривання

            if (!playerStartFound)
            {
                PlayerStartX = 1.5f * TileSize; // Резервна стартова позиція по X
                PlayerStartY = 1.5f * TileSize; // Резервна стартова позиція по Y
            }

            PlayerStartAngle = 2.75f; // Стартовий погляд приблизно вліво-вниз, як на скріні
        }

        public int TileSize { get; } = 64; // Розмір однієї клітинки у світі гри
        public int Width => _tiles.GetLength(1); // Ширина карти в клітинках
        public int Height => _tiles.GetLength(0); // Висота карти в клітинках
        public float PlayerStartX { get; } // Стартова позиція гравця по X
        public float PlayerStartY { get; } // Стартова позиція гравця по Y
        public float PlayerStartAngle { get; } // Стартовий кут погляду гравця

        public bool IsDoor(int tileX, int tileY)
        {
            if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height)
            {
                return false; // За межами карти дверей немає
            }

            return _tiles[tileY, tileX] == 2; // 2 означає двері
        }

        public bool IsDoorOpen(int tileX, int tileY)
        {
            Door? door = FindDoor(tileX, tileY); // Шукаємо об'єкт дверей у списку

            return door != null && door.IsOpen; // Двері відкриті тільки якщо об'єкт існує і має стан Open
        }

        public void UpdateDoors()
        {
            foreach (Door door in _doors)
            {
                door.Update(); // Оновлюємо тільки реальні двері, а не всі клітинки карти
            }
        }

        public void TryOpenDoor(float playerX, float playerY, float playerAngle)
        {
            float interactDistance = TileSize * 0.75f; // Наскільки далеко попереду гравець може відкрити двері

            float checkX = playerX + MathF.Cos(playerAngle) * interactDistance; // Точка перевірки попереду гравця по X
            float checkY = playerY + MathF.Sin(playerAngle) * interactDistance; // Точка перевірки попереду гравця по Y

            int tileX = (int)(checkX / TileSize); // Клітинка карти по X
            int tileY = (int)(checkY / TileSize); // Клітинка карти по Y
            Door? door = FindDoor(tileX, tileY); // Знаходимо двері, на які дивиться гравець

            door?.StartOpening(); // Якщо двері знайдені, запускаємо їх відкривання
        }

        public bool IsWall(int tileX, int tileY)
        {
            if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height)
            {
                return true; // За межами карти вважаємо стіною
            }

            if (_tiles[tileY, tileX] == 1)
            {
                return true; // Стіна завжди блокує рух
            }

            if (_tiles[tileY, tileX] == 2)
            {
                Door? door = FindDoor(tileX, tileY); // Знаходимо об'єкт дверей для цієї клітинки

                return door == null || !door.IsOpen; // Двері блокують, поки повністю не відкрилися
            }

            return false; // Підлога не блокує рух
        }

        public bool IsBlocking(float worldX, float worldY)
        {
            int tileX = (int)(worldX / TileSize); // Клітинка карти по X для точки світу
            int tileY = (int)(worldY / TileSize); // Клітинка карти по Y для точки світу

            if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height)
            {
                return true; // За межами карти точка вважається заблокованою
            }

            if (_tiles[tileY, tileX] == 1)
            {
                return true; // Стіна завжди заблокована
            }

            if (_tiles[tileY, tileX] == 2)
            {
                Door? door = FindDoor(tileX, tileY); // Знаходимо двері, які займають цю клітинку

                return door == null || door.IsBlocking(worldX, worldY, TileSize); // Для дверей враховуємо напрямок заїзду в стіну
            }

            return false; // Підлога не блокує точку
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
                    int screenX = mapX + x * cellSize; // X клітинки на екрані
                    int screenY = mapY + y * cellSize; // Y клітинки на екрані

                    if (IsDoor(x, y))
                    {
                        Door? door = FindDoor(x, y); // Беремо конкретний об'єкт дверей для малювання

                        graphics.FillRectangle(floorBrush, screenX, screenY, cellSize, cellSize); // Спочатку малюємо підлогу під дверима
                        door?.RenderMiniMap(graphics, screenX, screenY, cellSize, doorBrush); // Двері самі малюють свій поточний прогрес і напрямок
                        graphics.DrawRectangle(gridPen, screenX, screenY, cellSize, cellSize); // Малюємо межу клітинки
                        continue;
                    }

                    Brush tileBrush = IsWall(x, y) ? wallBrush : floorBrush; // Вибираємо колір для стіни або підлоги
                    graphics.FillRectangle(tileBrush, screenX, screenY, cellSize, cellSize); // Малюємо клітинку
                    graphics.DrawRectangle(gridPen, screenX, screenY, cellSize, cellSize); // Малюємо межу клітинки
                }
            }
        }

        private void CreateDoors()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (_tiles[y, x] == 2)
                    {
                        DoorSlideDirection direction = ChooseDoorSlideDirection(x, y); // Визначаємо, у яку сусідню стіну мають заїжджати двері
                        _doors.Add(new Door(x, y, direction)); // Створюємо двері з власними координатами, станом і напрямком
                    }
                }
            }
        }

        private DoorSlideDirection ChooseDoorSlideDirection(int tileX, int tileY)
        {
            if (IsStaticWall(tileX + 1, tileY))
            {
                return DoorSlideDirection.Right; // Якщо справа є стіна, двері заїжджають праворуч
            }

            if (IsStaticWall(tileX - 1, tileY))
            {
                return DoorSlideDirection.Left; // Якщо зліва є стіна, двері заїжджають ліворуч
            }

            if (IsStaticWall(tileX, tileY - 1))
            {
                return DoorSlideDirection.Up; // Якщо зверху є стіна, двері заїжджають вгору
            }

            if (IsStaticWall(tileX, tileY + 1))
            {
                return DoorSlideDirection.Down; // Якщо знизу є стіна, двері заїжджають вниз
            }

            return DoorSlideDirection.Right; // Резервний варіант, якщо біля дверей випадково немає стіни
        }

        private bool IsStaticWall(int tileX, int tileY)
        {
            if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height)
            {
                return false; // За межами карти не шукаємо стіну для напрямку дверей
            }

            return _tiles[tileY, tileX] == 1; // Для напрямку відкривання враховуємо тільки справжні стіни
        }

        private Door? FindDoor(int tileX, int tileY)
        {
            foreach (Door door in _doors)
            {
                if (door.TileX == tileX && door.TileY == tileY)
                {
                    return door; // Повертаємо саме ті двері, які стоять у потрібній клітинці
                }
            }

            return null; // У цій клітинці об'єкта дверей немає
        }
    }
}