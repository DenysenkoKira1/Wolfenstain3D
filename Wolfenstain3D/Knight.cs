using System.Drawing; // Потрібно для Color

namespace Wolfenstain3D
{
    internal sealed class Knight
    {
        private static readonly SpriteTexture SharedTexture = SpriteTexture.FromAssets("knight_transparent.png", 220); // Одна оптимізована текстура для всіх рицарів

        public Knight(float x, float y, float normalX, float normalY)
        {
            X = x; // Позиція рицаря у світі по X
            Y = y; // Позиція рицаря у світі по Y
            NormalX = normalX; // Напрямок, куди рицар дивиться по X
            NormalY = normalY; // Напрямок, куди рицар дивиться по Y
            TangentX = -normalY; // Горизонтальна вісь картинки рицаря по X
            TangentY = normalX; // Горизонтальна вісь картинки рицаря по Y
        }

        public float X { get; } // Світова X-позиція рицаря
        public float Y { get; } // Світова Y-позиція рицаря
        public float NormalX { get; } // Фіксований напрямок обличчя рицаря по X
        public float NormalY { get; } // Фіксований напрямок обличчя рицаря по Y
        public float TangentX { get; } // Ліва-права вісь рицаря по X
        public float TangentY { get; } // Ліва-права вісь рицаря по Y
        public float WorldWidth { get; } = 30f; // Ширина рицаря у світі гри
        public float WorldHeight { get; } = 58f; // Висота рицаря у світі гри
        public SpriteTexture Texture => SharedTexture; // Усі рицарі використовують одну спільну текстуру
        public Color MiniMapColor { get; } = Color.RoyalBlue; // Колір рицаря на міні-мапі
    }
}
