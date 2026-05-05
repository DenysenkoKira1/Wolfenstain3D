using System.Drawing; // Потрібно для Color

namespace Wolfenstain3D
{
    internal sealed class WallTorch
    {
        private static readonly SpriteTexture SharedTexture = SpriteTexture.FromAssets("wall_torch_transparent.png", 160); // Одна оптимізована текстура для всіх факелів

        public WallTorch(float x, float y, float normalX, float normalY)
        {
            X = x; // Позиція факела у світі по X
            Y = y; // Позиція факела у світі по Y
            NormalX = normalX; // Напрямок, куди дивиться стіна з факелом по X
            NormalY = normalY; // Напрямок, куди дивиться стіна з факелом по Y
            TangentX = -normalY; // Напрямок уздовж ширини факела по X
            TangentY = normalX; // Напрямок уздовж ширини факела по Y
        }

        public float X { get; } // Світова X-позиція
        public float Y { get; } // Світова Y-позиція
        public float NormalX { get; } // X-компонента напряму стіни
        public float NormalY { get; } // Y-компонента напряму стіни
        public float TangentX { get; } // X-компонента напряму вздовж стіни
        public float TangentY { get; } // Y-компонента напряму вздовж стіни
        public float WorldWidth { get; } = 22f; // Ширина факела у світі гри
        public float WorldHeight { get; } = 38f; // Висота факела у світі гри
        public float VerticalOffset { get; } = 0.32f; // Наскільки факел висить вище центру стіни
        public SpriteTexture Texture => SharedTexture; // Усі факели використовують одну спільну текстуру
        public Color MiniMapColor { get; } = Color.OrangeRed; // Колір факела на міні-мапі
    }
}
