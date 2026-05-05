using System; // Потрібно для DateTime і MathF
using System.Drawing; // Потрібно для Graphics і Brush

namespace Wolfenstain3D
{
    internal enum DoorSlideDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    internal class Door
    {
        private enum DoorState
        {
            Closed,
            Opening,
            Open,
            Closing
        }

        private const float OpenSpeed = 0.04f; // Швидкість відкривання дверей за один кадр
        private const float CloseSpeed = 0.04f; // Швидкість закривання дверей за один кадр
        private const int AutoCloseDelayMilliseconds = 1000; // Через скільки мілісекунд відкриті двері пробують закритися

        private readonly DoorSlideDirection _slideDirection; // Напрямок, у який двері заїжджають у сусідню стіну
        private DoorState _state = DoorState.Closed; // Поточний стан конкретних дверей
        private float _openProgress; // Прогрес відкривання від 0 до 1
        private DateTime _openedAt; // Час, коли двері повністю відкрилися

        public Door(int tileX, int tileY, DoorSlideDirection slideDirection)
        {
            TileX = tileX; // Координата дверей у карті по X
            TileY = tileY; // Координата дверей у карті по Y
            _slideDirection = slideDirection; // Запам'ятовуємо, у який бік двері мають зникати
        }

        public int TileX { get; } // Позиція дверей у клітинках карти по X
        public int TileY { get; } // Позиція дверей у клітинках карти по Y
        public bool IsOpen => _state == DoorState.Open; // Чи двері вже повністю відкриті

        public void StartOpening()
        {
            if (_state == DoorState.Closed || _state == DoorState.Closing)
            {
                _state = DoorState.Opening; // Запускаємо відкривання тільки для закритих дверей або дверей, які ще закривалися
            }
        }

        public void Update(bool playerBlocksDoor)
        {
            if (_state == DoorState.Opening)
            {
                UpdateOpening(); // Плавно відкриваємо двері
                return;
            }

            if (_state == DoorState.Open)
            {
                TryStartClosing(playerBlocksDoor); // Після паузи пробуємо закрити двері, якщо гравець не стоїть у клітинці
                return;
            }

            if (_state == DoorState.Closing)
            {
                UpdateClosing(); // Плавно закриваємо двері
            }
        }

        public bool IsBlocking(float worldX, float worldY, int tileSize)
        {
            if (_state == DoorState.Open)
            {
                return false; // Повністю відкриті двері більше не блокують рух і промені
            }

            float localX = worldX - TileX * tileSize; // X-координата точки всередині клітинки дверей
            float localY = worldY - TileY * tileSize; // Y-координата точки всередині клітинки дверей
            float openOffset = tileSize * _openProgress; // Наскільки двері вже заїхали у стіну

            return _slideDirection switch
            {
                DoorSlideDirection.Left => localX <= tileSize - openOffset, // Двері зникають ліворуч, видима частина лишається зліва
                DoorSlideDirection.Right => localX >= openOffset, // Двері зникають праворуч, видима частина лишається справа
                DoorSlideDirection.Up => localY <= tileSize - openOffset, // Двері зникають вгору, видима частина лишається зверху
                DoorSlideDirection.Down => localY >= openOffset, // Двері зникають вниз, видима частина лишається знизу
                _ => true // Захист на випадок невідомого напрямку
            };
        }

        public void RenderMiniMap(Graphics graphics, int screenX, int screenY, int cellSize, Brush doorBrush)
        {
            int doorOffset = (int)MathF.Floor(cellSize * _openProgress); // Зсув дверей на міні-мапі
            int visibleDoorSize = cellSize - doorOffset; // Розмір частини дверей, яка ще видима

            if (visibleDoorSize <= 0)
            {
                return; // Якщо двері повністю зникли, малювати вже нічого
            }

            Rectangle visibleDoorRectangle = _slideDirection switch
            {
                DoorSlideDirection.Left => new Rectangle(screenX, screenY, visibleDoorSize, cellSize), // Двері заїжджають у стіну ліворуч
                DoorSlideDirection.Right => new Rectangle(screenX + doorOffset, screenY, visibleDoorSize, cellSize), // Двері заїжджають у стіну праворуч
                DoorSlideDirection.Up => new Rectangle(screenX, screenY, cellSize, visibleDoorSize), // Двері заїжджають у стіну зверху
                DoorSlideDirection.Down => new Rectangle(screenX, screenY + doorOffset, cellSize, visibleDoorSize), // Двері заїжджають у стіну знизу
                _ => new Rectangle(screenX, screenY, cellSize, cellSize) // Резервний варіант для безпечного малювання
            };

            graphics.FillRectangle(doorBrush, visibleDoorRectangle); // Малюємо тільки ту частину дверей, яка ще не заїхала у стіну
        }

        private void UpdateOpening()
        {
            _openProgress = MathF.Min(1f, _openProgress + OpenSpeed); // Плавно зсуваємо двері в сторону стіни

            if (_openProgress >= 1f)
            {
                _state = DoorState.Open; // Коли двері повністю зникли, вони відкриті
                _openedAt = DateTime.UtcNow; // Запам'ятовуємо час відкриття для автозакривання
            }
        }

        private void TryStartClosing(bool playerBlocksDoor)
        {
            double openedMilliseconds = (DateTime.UtcNow - _openedAt).TotalMilliseconds; // Скільки часу двері вже відкриті

            if (openedMilliseconds >= AutoCloseDelayMilliseconds && !playerBlocksDoor)
            {
                _state = DoorState.Closing; // Закриваємо тільки якщо минуло 5 секунд і гравець не стоїть у клітинці дверей
            }
        }

        private void UpdateClosing()
        {
            _openProgress = MathF.Max(0f, _openProgress - CloseSpeed); // Плавно повертаємо двері назад у клітинку

            if (_openProgress <= 0f)
            {
                _state = DoorState.Closed; // Коли двері повністю повернулися, вони закриті
            }
        }
    }
}