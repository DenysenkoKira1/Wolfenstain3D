using System; // Потрібно для MathF
using System.Collections.Generic; // Потрібно для List<Door>, IReadOnlyList<WallTorch>, IReadOnlyList<Knight>
using System.Drawing; // Потрібно для Graphics, Brush, Pen, Color

namespace Wolfenstain3D
{
    internal class GameMap
    {
        private const float PlayerDoorSafetyRadius = 12f; // Радіус безпеки гравця, щоб двері не закривалися на ньому

        private readonly int[,] _tiles; // Карта рівня у вигляді числових тайлів
        private readonly List<Door> _doors = new List<Door>(); // Усі двері рівня як окремі об'єкти
        private readonly List<WallTorch> _wallTorches = new List<WallTorch>(); // Усі факели, закріплені на стінах
        private readonly List<Knight> _knights = new List<Knight>(); // Усі рицарі, розставлені по кімнатах
        private readonly GameSound _sound; // Звуки гри, які використовує карта для дверей

        public GameMap(GameSound sound)
        {
            _sound = sound; // Запам'ятовуємо звук, щоб двері могли запускати DoorOpen і DoorClose

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

            for (int y = 0; y < layout.Length; y++) // Проходимо по всіх рядках текстової карти
            {
                for (int x = 0; x < layout[y].Length; x++) // Проходимо по всіх символах поточного рядка карти
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
            CreateWallTorches(); // Створюємо факели у місцях, позначених хрестиками на міні-мапі
            CreateKnights(); // Створюємо рицарів у місцях, позначених хрестиками на міні-мапі

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
        public IReadOnlyList<WallTorch> WallTorches => _wallTorches; // Факели, які raycaster малює поверх стін
        public IReadOnlyList<Knight> Knights => _knights; // Рицарі, яких raycaster малює як standing-спрайти

        public bool IsDoor(int tileX, int tileY)
        {
            if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height) // Перевіряємо, чи координати клітинки не вийшли за межі карти
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

        public void UpdateDoors(float playerX, float playerY)
        {
            foreach (Door door in _doors)
            {
                bool playerBlocksDoor = IsPlayerTouchingDoorCell(door, playerX, playerY); // Перевіряємо, чи гравець стоїть у клітинці цих дверей
                door.Update(playerBlocksDoor); // Оновлюємо двері з урахуванням безпеки гравця
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
            if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height) // Перевіряємо, чи координати стіни не вийшли за межі карти
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

            if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height) // Перевіряємо, чи точка світу не потрапила за межі карти
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

            for (int y = 0; y < Height; y++) // Проходимо по всіх рядках карти для міні-мапи
            {
                for (int x = 0; x < Width; x++) // Проходимо по всіх клітинках поточного рядка для міні-мапи
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

            RenderWallTorchesOnMiniMap(graphics, mapX, mapY, cellSize); // Малюємо факели на міні-мапі поверх сітки
            RenderKnightsOnMiniMap(graphics, mapX, mapY, cellSize); // Малюємо рицарів на міні-мапі
        }

        private void CreateWallTorches()
        {
            float topTorchX = 9.5f * TileSize; // Верхній факел, який ми вже додали раніше
            float topTorchY = 12.04f * TileSize; // Трохи перед стіною з боку кімнати, щоб факел не ховався в стіну
            float bottomTorchX = 9.5f * TileSize; // Нижній факел, який ми вже додали раніше
            float bottomTorchY = 16.96f * TileSize; // Трохи перед стіною з боку кімнати

            _wallTorches.Add(new WallTorch(topTorchX, topTorchY, 0f, 1f)); // Лишаємо вже доданий верхній факел
            _wallTorches.Add(new WallTorch(bottomTorchX, bottomTorchY, 0f, -1f)); // Лишаємо вже доданий нижній факел

            AddTorchOnWallFace(1, 0, 0f, 1f); // Верхня зовнішня стіна, факел дивиться вниз
            AddTorchOnWallFace(6, 0, 0f, 1f); // Верхня зовнішня стіна, факел дивиться вниз
            AddTorchOnWallFace(10, 0, 0f, 1f); // Верхня зовнішня стіна, факел дивиться вниз
            AddTorchOnWallFace(17, 0, 0f, 1f); // Верхня зовнішня стіна, факел дивиться вниз
            AddTorchOnWallFace(21, 0, 0f, 1f); // Верхня зовнішня стіна, факел дивиться вниз

            AddTorchOnWallFace(0, 3, 1f, 0f); // Ліва зовнішня стіна, факел дивиться праворуч
            AddTorchOnWallFace(0, 8, 1f, 0f); // Ліва зовнішня стіна, факел дивиться праворуч
            AddTorchOnWallFace(0, 14, 1f, 0f); // Ліва зовнішня стіна, факел дивиться праворуч
            AddTorchOnWallFace(0, 19, 1f, 0f); // Ліва зовнішня стіна, факел дивиться праворуч

            AddTorchOnWallFace(24, 7, -1f, 0f); // Права зовнішня стіна, факел дивиться ліворуч
            AddTorchOnWallFace(24, 11, -1f, 0f); // Права зовнішня стіна, факел дивиться ліворуч
            AddTorchOnWallFace(24, 15, -1f, 0f); // Права зовнішня стіна, факел дивиться ліворуч
            AddTorchOnWallFace(24, 19, -1f, 0f); // Права зовнішня стіна, факел дивиться ліворуч

            AddTorchOnWallFace(2, 22, 0f, -1f); // Нижня зовнішня стіна, факел дивиться вгору
            AddTorchOnWallFace(5, 22, 0f, -1f); // Нижня зовнішня стіна, факел дивиться вгору
            AddTorchOnWallFace(10, 22, 0f, -1f); // Нижня зовнішня стіна, факел дивиться вгору
            AddTorchOnWallFace(13, 22, 0f, -1f); // Нижня зовнішня стіна, факел дивиться вгору
            AddTorchOnWallFace(18, 22, 0f, -1f); // Нижня зовнішня стіна, факел дивиться вгору
            AddTorchOnWallFace(21, 22, 0f, -1f); // Нижня зовнішня стіна, факел дивиться вгору

            AddTorchOnWallFace(8, 5, 0f, 1f); // Внутрішня горизонтальна стіна біля верхньої лівої кімнати
            AddTorchOnWallFace(14, 3, 1f, 0f); // Внутрішня вертикальна стіна біля верхньої центральної кімнати
            AddTorchOnWallFace(15, 8, -1f, 0f); // Центральна вертикальна стіна, факел дивиться ліворуч
            AddTorchOnWallFace(20, 8, -1f, 0f); // Права внутрішня вертикальна стіна, факел дивиться ліворуч
            AddTorchOnWallFace(15, 15, -1f, 0f); // Центральна вертикальна стіна біля стартової кімнати
            AddTorchOnWallFace(6, 17, 0f, -1f); // Внутрішня горизонтальна стіна під стартовою кімнатою
        }

        private void AddTorchOnWallFace(int tileX, int tileY, float normalX, float normalY)
        {
            const float WallOffset = 0.04f; // Маленький зсув від стіни, щоб факел не ховався в текстурі стіни

            float torchX = (tileX + 0.5f + normalX * (0.5f + WallOffset)) * TileSize; // X факела біля потрібної сторони клітинки
            float torchY = (tileY + 0.5f + normalY * (0.5f + WallOffset)) * TileSize; // Y факела біля потрібної сторони клітинки

            _wallTorches.Add(new WallTorch(torchX, torchY, normalX, normalY)); // Додаємо факел з фіксованим напрямком стіни
        }

        private void CreateKnights()
        {
            AddKnightAtTile(3, 1); // Рицар у верхній лівій кімнаті
            AddKnightAtTile(12, 1); // Рицар у верхній центральній частині
            AddKnightAtTile(23, 2); // Рицар у верхній правій кімнаті
            AddKnightAtTile(8, 4); // Рицар біля верхнього проходу
            AddKnightAtTile(1, 6); // Рицар у лівій верхній кімнаті
            AddKnightAtTile(7, 6); // Рицар біля лівого проходу
            AddKnightAtTile(16, 6); // Рицар у правій верхній кімнаті
            AddKnightAtTile(5, 10); // Рицар у лівій середній кімнаті
            AddKnightAtTile(14, 10); // Рицар у центральній кімнаті
            AddKnightAtTile(5, 12); // Рицар біля стартової кімнати ліворуч
            AddKnightAtTile(7, 12); // Рицар біля стартової кімнати ліворуч
            AddKnightAtTile(14, 12); // Рицар біля стартової кімнати праворуч
            AddKnightAtTile(20, 12); // Рицар у правій середній кімнаті
            AddKnightAtTile(1, 16); // Рицар у нижній лівій кімнаті
            AddKnightAtTile(16, 16); // Рицар у нижній центральній кімнаті
            AddKnightAtTile(6, 18); // Рицар у нижній лівій кімнаті
            AddKnightAtTile(14, 18); // Рицар у нижній центральній кімнаті
            AddKnightAtTile(17, 18); // Рицар у нижній правій частині центру
            AddKnightAtTile(1, 21); // Рицар біля нижнього лівого кута
            AddKnightAtTile(23, 21); // Рицар біля нижнього правого кута
        }

        private void AddKnightAtTile(int tileX, int tileY)
        {
            float knightX = (tileX + 0.5f) * TileSize; // X-координата центру клітинки
            float knightY = (tileY + 0.5f) * TileSize; // Y-координата центру клітинки
            PointF facing = ChooseKnightFacing(tileX, tileY); // Вибираємо бік, куди рицар має дивитися в кімнату

            _knights.Add(new Knight(knightX, knightY, facing.X, facing.Y)); // Додаємо рицаря з фіксованим напрямком обличчя
        }

        private PointF ChooseKnightFacing(int tileX, int tileY)
        {
            int upDistance = DistanceToWall(tileX, tileY, 0, -1); // Найближча стіна зверху
            int downDistance = DistanceToWall(tileX, tileY, 0, 1); // Найближча стіна знизу
            int leftDistance = DistanceToWall(tileX, tileY, -1, 0); // Найближча стіна зліва
            int rightDistance = DistanceToWall(tileX, tileY, 1, 0); // Найближча стіна справа
            int nearestDistance = Math.Min(Math.Min(upDistance, downDistance), Math.Min(leftDistance, rightDistance)); // Шукаємо найближчу стіну

            if (nearestDistance == upDistance)
            {
                return new PointF(0f, 1f); // Якщо стіна зверху, рицар дивиться вниз у кімнату
            }

            if (nearestDistance == downDistance)
            {
                return new PointF(0f, -1f); // Якщо стіна знизу, рицар дивиться вгору у кімнату
            }

            if (nearestDistance == leftDistance)
            {
                return new PointF(1f, 0f); // Якщо стіна зліва, рицар дивиться праворуч у кімнату
            }

            return new PointF(-1f, 0f); // Якщо стіна справа, рицар дивиться ліворуч у кімнату
        }

        private int DistanceToWall(int tileX, int tileY, int stepX, int stepY)
        {
            int maxDistance = Math.Max(Width, Height); // Максимальна дистанція пошуку в межах карти

            for (int distance = 1; distance <= maxDistance; distance++) // Рухаємось від рицаря до найближчої стіни
            {
                int checkX = tileX + stepX * distance; // Клітинка перевірки по X
                int checkY = tileY + stepY * distance; // Клітинка перевірки по Y

                if (checkX < 0 || checkY < 0 || checkX >= Width || checkY >= Height) // Перевіряємо, чи пошук стіни не вийшов за межі карти
                {
                    return distance; // Край карти теж вважаємо найближчою межею кімнати
                }

                if (_tiles[checkY, checkX] == 1)
                {
                    return distance; // Знайшли справжню стіну
                }
            }

            return int.MaxValue; // Резервний варіант, якщо стіну не знайдено
        }

        private void RenderWallTorchesOnMiniMap(Graphics graphics, int mapX, int mapY, int cellSize)
        {
            float miniMapScale = (float)cellSize / TileSize; // Масштаб для переведення світових координат у міні-мапу

            foreach (WallTorch torch in _wallTorches)
            {
                float torchX = mapX + torch.X * miniMapScale; // X факела на міні-мапі
                float torchY = mapY + torch.Y * miniMapScale; // Y факела на міні-мапі

                using Brush torchBrush = new SolidBrush(torch.MiniMapColor); // Колір позначки факела
                graphics.FillEllipse(torchBrush, torchX - 3, torchY - 3, 6, 6); // Маленька точка факела на міні-мапі
            }
        }

        private void RenderKnightsOnMiniMap(Graphics graphics, int mapX, int mapY, int cellSize)
        {
            float miniMapScale = (float)cellSize / TileSize; // Масштаб для переведення світових координат у міні-мапу

            foreach (Knight knight in _knights)
            {
                float knightX = mapX + knight.X * miniMapScale; // X рицаря на міні-мапі
                float knightY = mapY + knight.Y * miniMapScale; // Y рицаря на міні-мапі

                using Brush knightBrush = new SolidBrush(knight.MiniMapColor); // Колір позначки рицаря
                graphics.FillRectangle(knightBrush, knightX - 3, knightY - 3, 6, 6); // Малюємо рицаря маленьким квадратом
            }
        }

        private void CreateDoors()
        {
            for (int y = 0; y < Height; y++) // Проходимо по всіх рядках карти, щоб знайти двері
            {
                for (int x = 0; x < Width; x++) // Перевіряємо кожну клітинку поточного рядка на двері
                {
                    if (_tiles[y, x] == 2)
                    {
                        DoorSlideDirection direction = ChooseDoorSlideDirection(x, y); // Визначаємо, у яку сусідню стіну мають заїжджати двері
                        Door door = new Door(x, y, direction); // Створюємо двері з власними координатами, станом і напрямком
                        door.OpeningStarted += _sound.PlayDoorOpen; // При старті відкривання вмикаємо звук DoorOpen
                        door.ClosingStarted += _sound.PlayDoorClose; // При старті закривання вмикаємо звук DoorClose
                        _doors.Add(door); // Додаємо двері у список карти
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
            if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height) // Перевіряємо, чи сусідня клітинка дверей існує на карті
            {
                return false; // За межами карти не шукаємо стіну для напрямку дверей
            }

            return _tiles[tileY, tileX] == 1; // Для напрямку відкривання враховуємо тільки справжні стіни
        }

        private bool IsPlayerTouchingDoorCell(Door door, float playerX, float playerY)
        {
            return IsWorldPointInsideDoorCell(door, playerX, playerY)
                || IsWorldPointInsideDoorCell(door, playerX - PlayerDoorSafetyRadius, playerY)
                || IsWorldPointInsideDoorCell(door, playerX + PlayerDoorSafetyRadius, playerY)
                || IsWorldPointInsideDoorCell(door, playerX, playerY - PlayerDoorSafetyRadius)
                || IsWorldPointInsideDoorCell(door, playerX, playerY + PlayerDoorSafetyRadius); // Перевіряємо центр і краї гравця, щоб двері не закрилися на ньому
        }

        private bool IsWorldPointInsideDoorCell(Door door, float worldX, float worldY)
        {
            int tileX = (int)(worldX / TileSize); // Клітинка точки по X
            int tileY = (int)(worldY / TileSize); // Клітинка точки по Y

            return tileX == door.TileX && tileY == door.TileY; // Точка знаходиться саме в клітинці цих дверей
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
