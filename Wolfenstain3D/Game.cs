using System; // Потрібно для EventArgs
using System.Drawing; // Потрібно для Graphics, Color, Rectangle
using System.Windows.Forms; // Потрібно для Form і Timer

namespace Wolfenstain3D
{
    internal class Game
    {
        private readonly Form _hostForm; // Форма, у якій буде відображатися гра
        private readonly System.Windows.Forms.Timer _gameTimer; // Таймер для ігрового циклу
        private readonly GameSound _sound; // Звуки гри: двері та музика рівня
        private readonly Player _player; // Гравець, яким ми будемо керувати
        private readonly InputController _input; // Обробник натиснутих клавіш
        private readonly GameMap _map; // Карта рівня
        private readonly Raycaster _raycaster; // Об’єкт для запуску raycasting-променів

        public Game(Form hostForm)
        {
            _hostForm = hostForm; // Запам’ятовуємо форму
            _gameTimer = new System.Windows.Forms.Timer(); // Створюємо WinForms-таймер
            _gameTimer.Interval = 16; // Приблизно 60 FPS
            _gameTimer.Tick += GameLoop; // Підписуємося на виклик ігрового циклу
            _input = new InputController(); // Створюємо обробник клавіш
            _sound = new GameSound(); // Готуємо звуки гри
            _map = new GameMap(_sound); // Створюємо карту рівня і передаємо їй звук дверей
            _player = new Player(_map.PlayerStartX, _map.PlayerStartY, _map.PlayerStartAngle); // Створюємо гравця у стартовій позиції з карти
            _raycaster = new Raycaster(); // Створюємо raycaster для променів
        }

        public void Start()
        {
            _sound.StartLevelMusic(); // Запускаємо музику першого рівня
            _gameTimer.Start(); // Запускаємо таймер
        }

        public void Stop()
        {
            _gameTimer.Stop(); // Зупиняємо ігровий цикл
            _sound.Dispose(); // Зупиняємо та звільняємо звуки
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            Update(); // Оновлюємо логіку гри
            _hostForm.Invalidate(); // Просимо форму перемалюватися
        }

        private void Update()
        {
            _map.UpdateDoors(_player.X, _player.Y); // Плавно оновлюємо двері й не даємо їм закриватися на гравцеві
            _player.Update(_input, _map); // Оновлюємо гравця з урахуванням карти та collision
        }

        public void TryOpenDoor()
        {
            _map.TryOpenDoor(_player.X, _player.Y, _player.Angle); // Просимо карту відкрити двері перед гравцем
        }

        public void Render(Graphics graphics)
        {
            graphics.Clear(Color.Black); // Очищаємо фон перед новим кадром

            Rectangle gameViewport = new Rectangle(20, 20, 660, 560); // Основна область для 3D-вигляду
            _raycaster.Render3D(graphics, _player, _map, gameViewport); // Малюємо псевдо-3D сцену

            int miniMapX = 710; // X-позиція міні-мапи справа
            int miniMapY = 20; // Y-позиція міні-мапи зверху
            int miniMapCellSize = Math.Max(10, Math.Min(14, Math.Min(350 / _map.Width, 350 / _map.Height))); // Підбираємо розмір клітинки, щоб карта вмістилася
            float miniMapScale = (float)miniMapCellSize / _map.TileSize; // Масштаб зі світу гри в міні-мапу

            _map.RenderMiniMap(graphics, miniMapX, miniMapY, miniMapCellSize); // Малюємо карту поверх 3D як debug overlay
            _raycaster.RenderMiniMapRays(graphics, _player, _map, miniMapX, miniMapY, miniMapScale); // Малюємо промені до стін
            _player.RenderMiniMap(graphics, miniMapX, miniMapY, miniMapScale); // Малюємо гравця поверх карти
            _player.RenderDebugInfo(graphics, miniMapX, miniMapY + _map.Height * miniMapCellSize + 16); // Малюємо короткий debug-блок
        }

        public void KeyDown(Keys key)
        {
            _input.KeyDown(key); // Передаємо натиснуту клавішу в InputController

            if (key == Keys.Space)
            {
                TryOpenDoor(); // Якщо натиснули Space, пробуємо відкрити двері перед гравцем
            }
        }

        public void KeyUp(Keys key)
        {
            _input.KeyUp(key); // Передаємо відпущену клавішу в InputController
        }
    }
}