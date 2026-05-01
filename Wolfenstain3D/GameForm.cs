using System.Drawing; // Потрібно для Color і Size
using System.Windows.Forms; // Потрібно для Form та PaintEventArgs

namespace Wolfenstain3D
{
    public partial class GameForm : Form
    {
        private readonly Game _game; // Основний об’єкт гри

        public GameForm()
        {
            InitializeComponent(); // Ініціалізація компонентів форми

            Text = "Wolfenstain3D"; // Заголовок вікна
            StartPosition = FormStartPosition.CenterScreen; // Запуск по центру екрана
            BackColor = Color.Black; // Чорний фон
            FormBorderStyle = FormBorderStyle.FixedSingle; // Фіксований розмір вікна
            MaximizeBox = false; // Вимикаємо розгортання
            MinimizeBox = true; // Дозволяємо згортання
            KeyPreview = true; // Форма ловить натискання клавіш
            ClientSize = new Size(1100, 640); // Збільшений розмір, щоб вмістити 3D-вид і міні-мапу
            DoubleBuffered = true; // Менше мерехтіння

            _game = new Game(this); // Створюємо об’єкт гри
            Paint += GameForm_Paint; // Підписуємося на подію малювання
            KeyDown += GameForm_KeyDown; // Підписуємося на натискання клавіш
            KeyUp += GameForm_KeyUp; // Підписуємося на відпускання клавіш

            _game.Start(); // Запускаємо гру
        }

        private void GameForm_Paint(object? sender, PaintEventArgs e)
        {
            _game.Render(e.Graphics); // Передаємо малювання в клас Game
        }

        private void GameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            _game.KeyDown(e.KeyCode); // Передаємо натиснуту клавішу в гру
        }

        private void GameForm_KeyUp(object? sender, KeyEventArgs e)
        {
            _game.KeyUp(e.KeyCode); // Передаємо відпущену клавішу в гру
        }
    }
}