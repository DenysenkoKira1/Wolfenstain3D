using System; // Потрібно для IDisposable і AppContext
using System.IO; // Потрібно для Path
using System.Media; // Потрібно для SoundPlayer

namespace Wolfenstain3D
{
    internal sealed class GameSound : IDisposable
    {
        private readonly SoundPlayer _doorOpenPlayer; // Програвач звуку відкривання дверей
        private readonly SoundPlayer _doorClosePlayer; // Програвач звуку закривання дверей
        private readonly SoundPlayer _levelMusicPlayer; // Програвач фонової музики рівня

        public GameSound()
        {
            string soundsFolder = Path.Combine(AppContext.BaseDirectory, "Assets", "Sounds"); // Папка зі звуками після build

            _doorOpenPlayer = CreatePlayer(Path.Combine(soundsFolder, "DoorOpen.wav")); // Оригінальний звук відкривання
            _doorClosePlayer = CreatePlayer(Path.Combine(soundsFolder, "DoorClose.wav")); // Оригінальний звук закривання
            _levelMusicPlayer = CreatePlayer(Path.Combine(soundsFolder, "Level1.wav")); // Оригінальна музика першого рівня
        }

        public void StartLevelMusic()
        {
            TryPlayLooping(_levelMusicPlayer); // Запускаємо музику рівня в циклі
        }

        public void PlayDoorOpen()
        {
            TryPlayOnce(_doorOpenPlayer); // Програємо звук відкривання дверей один раз
        }

        public void PlayDoorClose()
        {
            TryPlayOnce(_doorClosePlayer); // Програємо звук закривання дверей один раз
        }

        public void Dispose()
        {
            _doorOpenPlayer.Dispose(); // Звільняємо ресурс звуку відкривання
            _doorClosePlayer.Dispose(); // Звільняємо ресурс звуку закривання
            _levelMusicPlayer.Dispose(); // Звільняємо ресурс музики
        }

        private static SoundPlayer CreatePlayer(string filePath)
        {
            SoundPlayer player = new SoundPlayer(filePath); // Створюємо стандартний WAV-програвач
            player.LoadAsync(); // Завантажуємо звук у фоні, щоб не підвисала гра

            return player;
        }

        private static void TryPlayOnce(SoundPlayer player)
        {
            try
            {
                player.Stop(); // Якщо цей самий звук ще грає, починаємо його заново
                player.Play(); // Play не блокує ігровий цикл
            }
            catch
            {
                // Якщо звук не знайдено або аудіопристрій недоступний, гра не повинна падати.
            }
        }

        private static void TryPlayLooping(SoundPlayer player)
        {
            try
            {
                player.PlayLooping(); // Фонова музика повторюється постійно
            }
            catch
            {
                // Якщо музику не вдалося запустити, гра просто продовжує працювати без звуку.
            }
        }
    }
}