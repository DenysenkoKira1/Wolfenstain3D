using System.Drawing; // Потрібно для Color

namespace Wolfenstain3D
{
    internal sealed class WallTorch : GameSprite
    {
        private static readonly SpriteTexture SharedTexture = SpriteTexture.FromAssets("wall_torch_transparent.png", 160); // Одна оптимізована текстура для всіх факелів

        public WallTorch(float x, float y, float normalX, float normalY)
            : base(x, y, 22f, 38f, SharedTexture, Color.OrangeRed)
        {
            NormalX = normalX; // Напрямок, куди дивиться стіна з факелом по X
            NormalY = normalY; // Напрямок, куди дивиться стіна з факелом по Y
            TangentX = -normalY; // Напрямок уздовж ширини факела по X
            TangentY = normalX; // Напрямок уздовж ширини факела по Y
        }

        public float NormalX { get; } // X-компонента напряму стіни
        public float NormalY { get; } // Y-компонента напряму стіни
        public float TangentX { get; } // X-компонента напряму вздовж стіни
        public float TangentY { get; } // Y-компонента напряму вздовж стіни
        public float VerticalOffset { get; } = 0.32f; // Наскільки факел висить вище центру стіни
    }
}
