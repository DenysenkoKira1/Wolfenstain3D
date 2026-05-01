using System.Collections.Generic; // Потрібно для HashSet
using System.Windows.Forms; // Потрібно для Keys

namespace Wolfenstain3D
{
    internal class InputController
    {
        private readonly HashSet<Keys> _pressedKeys = new HashSet<Keys>(); // Клавіші, які зараз натиснуті

        public bool IsKeyPressed(Keys key)
        {
            return _pressedKeys.Contains(key); // Перевіряємо, чи конкретна клавіша натиснута
        }

        public void KeyDown(Keys key)
        {
            _pressedKeys.Add(key); // Додаємо клавішу в список натиснутих
        }

        public void KeyUp(Keys key)
        {
            _pressedKeys.Remove(key); // Прибираємо клавішу зі списку натиснутих
        }
    }
}
