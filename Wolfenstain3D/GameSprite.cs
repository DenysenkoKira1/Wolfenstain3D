using System.Drawing; // Потрібно для Color

namespace Wolfenstain3D
{
    internal abstract class GameSprite
    {
        protected GameSprite(float x, float y, float worldWidth, float worldHeight, SpriteTexture texture, Color miniMapColor)
        {
            X = x; // Позиція об'єкта у світі по X
            Y = y; // Позиція об'єкта у світі по Y
            WorldWidth = worldWidth; // Ширина спрайта у світі гри
            WorldHeight = worldHeight; // Висота спрайта у світі гри
            Texture = texture; // Текстура, якою цей об'єкт малюється у 3D
            MiniMapColor = miniMapColor; // Колір позначки об'єкта на міні-мапі
        }

        public float X { get; } // Світова X-позиція спрайта
        public float Y { get; } // Світова Y-позиція спрайта
        public float WorldWidth { get; } // Ширина спрайта у світі гри
        public float WorldHeight { get; } // Висота спрайта у світі гри
        public SpriteTexture Texture { get; } // Текстура спрайта
        public Color MiniMapColor { get; } // Колір спрайта на міні-мапі
    }
}
