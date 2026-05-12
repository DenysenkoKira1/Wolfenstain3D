using System.Drawing; // Потрібно для Color

namespace Wolfenstain3D
{
    internal sealed class Knight : GameSprite
    {
        private static readonly SpriteTexture SharedTexture = SpriteTexture.FromAssets("knight_transparent.png", 220); // Одна оптимізована текстура для всіх рицарів

        public Knight(float x, float y, float normalX, float normalY)
            : base(x, y, 30f, 58f, SharedTexture, Color.RoyalBlue)
        {
            NormalX = normalX; // Напрямок, куди рицар дивиться по X
            NormalY = normalY; // Напрямок, куди рицар дивиться по Y
            TangentX = -normalY; // Горизонтальна вісь картинки рицаря по X
            TangentY = normalX; // Горизонтальна вісь картинки рицаря по Y
        }

        public float NormalX { get; } // Фіксований напрямок обличчя рицаря по X
        public float NormalY { get; } // Фіксований напрямок обличчя рицаря по Y
        public float TangentX { get; } // Ліва-права вісь рицаря по X
        public float TangentY { get; } // Ліва-права вісь рицаря по Y
    }
}
