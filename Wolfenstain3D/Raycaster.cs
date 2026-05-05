using System; // Потрібно для MathF
using System.Collections.Generic; // Потрібно для List<WallTorch>, List<Knight>
using System.Drawing; // Потрібно для Graphics, Pen, Color, Rectangle, PointF, Bitmap
using System.Drawing.Imaging; // Потрібно для PixelFormat, BitmapData, ImageLockMode
using System.Runtime.InteropServices; // Потрібно для швидкого копіювання масиву пікселів у Bitmap

namespace Wolfenstain3D
{
    internal class Raycaster
    {
        private const int MiniMapRayCount = 31; // Кількість променів для debug-відображення на міні-мапі
        private const float FieldOfView = MathF.PI / 3; // Кут огляду 60 градусів
        private const float MaxRayDistance = 500f; // Максимальна довжина променя
        private const float RayStep = 2f; // Крок перевірки променя

        private readonly WallTexture _wallTexture = new WallTexture(); // Текстура для звичайних стін
        private readonly DoorTexture _doorTexture = new DoorTexture(); // Текстура для бірюзових дверей із золотою емблемою
        private readonly FloorTexture _floorTexture = new FloorTexture(); // Текстура для простої сірої підлоги
        private readonly CeilingTexture _ceilingTexture = new CeilingTexture(); // Текстура для темно-сірої стелі

        public void Render3D(Graphics graphics, Player player, GameMap map, Rectangle viewport)
        {
            int[] framePixels = new int[viewport.Width * viewport.Height]; // Масив пікселів усього кадру
            float[] wallDistances = new float[viewport.Width]; // Z-buffer: відстань до стіни для кожної колонки

            float projectionDistance = viewport.Width / (2f * MathF.Tan(FieldOfView / 2f)); // Відстань до уявної площини проєкції
            float angleStep = FieldOfView / viewport.Width; // Один промінь на кожну вертикальну колонку екрана
            float startAngle = player.Angle - FieldOfView / 2f; // Початковий кут лівого краю огляду

            RenderCeiling(framePixels, viewport.Width, viewport.Height, player, map, projectionDistance, startAngle, angleStep); // Малюємо стелю в масив кадру
            RenderFloor(framePixels, viewport.Width, viewport.Height, player, map, projectionDistance, startAngle, angleStep); // Малюємо підлогу в масив кадру
            RenderWalls(framePixels, viewport.Width, viewport.Height, player, map, projectionDistance, startAngle, angleStep, wallDistances); // Малюємо стіни і двері та запам'ятовуємо їхню глибину
            FillHorizonLine(framePixels, viewport.Width, viewport.Height); // Закриваємо лінію горизонту, яку не малює ні стеля, ні підлога
            RenderWallTorches(framePixels, viewport.Width, viewport.Height, player, map, projectionDistance, wallDistances); // Малюємо настінні факели поверх сцени
            RenderKnights(framePixels, viewport.Width, viewport.Height, player, map, projectionDistance, wallDistances); // Малюємо рицарів як standing-спрайти

            using Bitmap sceneBitmap = new Bitmap(viewport.Width, viewport.Height, PixelFormat.Format32bppArgb); // Bitmap потрібен тільки для показу готового кадру
            CopyFrameToBitmap(framePixels, sceneBitmap); // Швидко копіюємо масив пікселів у Bitmap одним блоком

            graphics.DrawImageUnscaled(sceneBitmap, viewport.X, viewport.Y); // Накладаємо готову 3D-сцену на форму
        }

        public void RenderMiniMapRays(Graphics graphics, Player player, GameMap map, int mapX, int mapY, float miniMapScale)
        {
            using Pen rayPen = new Pen(Color.FromArgb(140, Color.Yellow), 1); // Колір променів на міні-мапі

            float startAngle = player.Angle - FieldOfView / 2; // Початковий кут лівого променя
            float angleStep = FieldOfView / (MiniMapRayCount - 1); // Відстань між променями за кутом

            for (int i = 0; i < MiniMapRayCount; i++)
            {
                float rayAngle = startAngle + angleStep * i; // Поточний кут променя
                RayHit hit = CastRay(player.X, player.Y, rayAngle, map); // Точка, де промінь зустрів перешкоду

                float startX = mapX + player.X * miniMapScale; // Початок променя на міні-мапі по X
                float startY = mapY + player.Y * miniMapScale; // Початок променя на міні-мапі по Y
                float endX = mapX + hit.Point.X * miniMapScale; // Кінець променя на міні-мапі по X
                float endY = mapY + hit.Point.Y * miniMapScale; // Кінець променя на міні-мапі по Y

                graphics.DrawLine(rayPen, startX, startY, endX, endY); // Малюємо промінь
            }
        }

        private void RenderCeiling(int[] framePixels, int frameWidth, int frameHeight, Player player, GameMap map, float projectionDistance, float startAngle, float angleStep)
        {
            int horizon = frameHeight / 2; // Лінія горизонту, вище якої починається стеля
            float cameraHeight = map.TileSize / 2f; // Умовна висота очей гравця відносно стелі та підлоги

            for (int screenY = 0; screenY < horizon; screenY++)
            {
                float yOffset = horizon - screenY; // Відстань пікселя стелі від горизонту
                float correctedDistance = cameraHeight * projectionDistance / yOffset; // Перпендикулярна відстань до точки стелі
                int shade = CalculateCeilingShade(correctedDistance); // Затемнення стелі залежно від відстані
                int rowStart = screenY * frameWidth; // Початок рядка в масиві кадру

                for (int column = 0; column < frameWidth; column++)
                {
                    float rayAngle = startAngle + column * angleStep; // Кут променя для цього пікселя стелі
                    float rayDistance = correctedDistance / MathF.Cos(rayAngle - player.Angle); // Реальна відстань уздовж променя без риб'ячого ока
                    float worldX = player.X + MathF.Cos(rayAngle) * rayDistance; // X точки стелі у світі
                    float worldY = player.Y + MathF.Sin(rayAngle) * rayDistance; // Y точки стелі у світі

                    int textureX = PositiveModulo((int)worldX, CeilingTexture.Size); // X у повторюваній текстурі стелі
                    int textureY = PositiveModulo((int)worldY, CeilingTexture.Size); // Y у повторюваній текстурі стелі
                    Color textureColor = _ceilingTexture.GetPixel(textureX, textureY); // Колір стелі в цій точці
                    Color ceilingColor = ApplyShade(textureColor, shade); // Затемнюємо стелю на відстані

                    framePixels[rowStart + column] = ceilingColor.ToArgb(); // Записуємо піксель стелі в масив кадру
                }
            }
        }

        private void RenderFloor(int[] framePixels, int frameWidth, int frameHeight, Player player, GameMap map, float projectionDistance, float startAngle, float angleStep)
        {
            int horizon = frameHeight / 2; // Лінія горизонту, нижче якої починається підлога
            float cameraHeight = map.TileSize / 2f; // Умовна висота очей гравця над підлогою

            for (int screenY = horizon + 1; screenY < frameHeight; screenY++)
            {
                float yOffset = screenY - horizon; // Відстань пікселя підлоги від горизонту
                float correctedDistance = cameraHeight * projectionDistance / yOffset; // Перпендикулярна відстань до точки підлоги
                int shade = CalculateFloorShade(correctedDistance); // Затемнення підлоги залежно від відстані
                int rowStart = screenY * frameWidth; // Початок рядка в масиві кадру

                for (int column = 0; column < frameWidth; column++)
                {
                    float rayAngle = startAngle + column * angleStep; // Кут променя для цього пікселя підлоги
                    float rayDistance = correctedDistance / MathF.Cos(rayAngle - player.Angle); // Реальна відстань уздовж променя без риб'ячого ока
                    float worldX = player.X + MathF.Cos(rayAngle) * rayDistance; // X точки підлоги у світі
                    float worldY = player.Y + MathF.Sin(rayAngle) * rayDistance; // Y точки підлоги у світі

                    int textureX = PositiveModulo((int)worldX, FloorTexture.Size); // X у повторюваній текстурі підлоги
                    int textureY = PositiveModulo((int)worldY, FloorTexture.Size); // Y у повторюваній текстурі підлоги
                    Color textureColor = _floorTexture.GetPixel(textureX, textureY); // Колір підлоги в цій точці
                    Color floorColor = ApplyShade(textureColor, shade); // Затемнюємо підлогу на відстані

                    framePixels[rowStart + column] = floorColor.ToArgb(); // Записуємо піксель підлоги в масив кадру
                }
            }
        }

        private void RenderWalls(int[] framePixels, int frameWidth, int frameHeight, Player player, GameMap map, float projectionDistance, float startAngle, float angleStep, float[] wallDistances)
        {
            for (int column = 0; column < frameWidth; column++)
            {
                float rayAngle = startAngle + column * angleStep; // Кут поточного променя
                RayHit hit = CastRay(player.X, player.Y, rayAngle, map); // Шукаємо точку зіткнення зі стіною або дверима

                float correctedDistance = hit.Distance * MathF.Cos(rayAngle - player.Angle); // Прибираємо ефект "риб’ячого ока"
                correctedDistance = MathF.Max(correctedDistance, 1f); // Захист від ділення на нуль
                wallDistances[column] = correctedDistance; // Запам'ятовуємо глибину стіни для факелів

                float wallHeight = map.TileSize * projectionDistance / correctedDistance; // Висота стіни на екрані
                float wallTop = (frameHeight - wallHeight) / 2f; // Верхня точка стіни всередині кадру
                float wallBottom = wallTop + wallHeight; // Нижня точка стіни всередині кадру

                int textureX = hit.TextureX; // Горизонтальна координата текстури для цієї колонки
                int shade = CalculateWallShade(correctedDistance); // Яскравість залежно від відстані

                int drawStart = Math.Max(0, (int)wallTop); // Верхня межа малювання в межах кадру
                int drawEnd = Math.Min(frameHeight, (int)wallBottom); // Нижня межа малювання в межах кадру

                for (int screenY = drawStart; screenY < drawEnd; screenY++)
                {
                    float texturePercentY = (screenY - wallTop) / wallHeight; // Позиція пікселя по висоті стіни від 0 до 1
                    int textureY = Math.Clamp((int)(texturePercentY * WallTexture.Size), 0, WallTexture.Size - 1); // Вертикальна координата текстури

                    Color textureColor = hit.IsDoor
                        ? _doorTexture.GetPixel(textureX, textureY) // Для дверей беремо бірюзову текстуру із золотою емблемою
                        : _wallTexture.GetPixel(textureX, textureY); // Для стіни беремо справжній піксель текстури

                    Color wallColor = ApplyShade(textureColor, shade); // Затемнюємо текстуру на відстані
                    framePixels[screenY * frameWidth + column] = wallColor.ToArgb(); // Записуємо піксель стіни або дверей у масив кадру
                }
            }
        }

        private void RenderWallTorches(int[] framePixels, int frameWidth, int frameHeight, Player player, GameMap map, float projectionDistance, float[] wallDistances)
        {
            List<WallTorch> torches = new List<WallTorch>(map.WallTorches); // Копія списку, щоб сортувати без зміни карти
            torches.Sort((left, right) => CalculateDistanceSquared(right, player).CompareTo(CalculateDistanceSquared(left, player))); // Дальні факели малюємо першими

            float startAngle = player.Angle - FieldOfView / 2f; // Кут лівого краю камери
            float angleStep = FieldOfView / frameWidth; // Один промінь на кожну колонку екрана

            foreach (WallTorch torch in torches)
            {
                for (int screenX = 0; screenX < frameWidth; screenX++)
                {
                    float rayAngle = startAngle + screenX * angleStep; // Кут променя для поточної колонки
                    float rayDirectionX = MathF.Cos(rayAngle); // Напрямок променя по X
                    float rayDirectionY = MathF.Sin(rayAngle); // Напрямок променя по Y
                    float denominator = rayDirectionX * torch.NormalX + rayDirectionY * torch.NormalY; // Перевіряємо перетин з площиною факела

                    if (denominator >= -0.01f)
                    {
                        continue; // Зворотний бік факела або майже паралельний промінь не малюємо
                    }

                    float deltaX = torch.X - player.X; // Вектор від гравця до факела по X
                    float deltaY = torch.Y - player.Y; // Вектор від гравця до факела по Y
                    float rayDistance = (deltaX * torch.NormalX + deltaY * torch.NormalY) / denominator; // Відстань до площини факела

                    if (rayDistance <= 1f || rayDistance > MaxRayDistance)
                    {
                        continue; // Факел позаду, занадто близько або занадто далеко не малюємо
                    }

                    float hitX = player.X + rayDirectionX * rayDistance; // X-точка перетину променя з площиною факела
                    float hitY = player.Y + rayDirectionY * rayDistance; // Y-точка перетину променя з площиною факела
                    float localX = (hitX - torch.X) * torch.TangentX + (hitY - torch.Y) * torch.TangentY; // Горизонтальна позиція всередині факела
                    float halfWidth = torch.WorldWidth / 2f; // Половина ширини факела у світі

                    if (localX < -halfWidth || localX > halfWidth)
                    {
                        continue; // Промінь влучив у стіну поруч, але не у факел
                    }

                    float correctedDistance = rayDistance * MathF.Cos(rayAngle - player.Angle); // Прибираємо риб'яче око для висоти факела

                    if (correctedDistance <= 1f || correctedDistance >= wallDistances[screenX])
                    {
                        continue; // Якщо перед факелом є стіна, не малюємо цю колонку
                    }

                    float spriteHeight = torch.WorldHeight * projectionDistance / correctedDistance; // Висота факела на екрані
                    float spriteCenterY = frameHeight / 2f - spriteHeight * torch.VerticalOffset; // Піднімаємо факел на стіні
                    int drawStartY = Math.Max(0, (int)(spriteCenterY - spriteHeight / 2f)); // Верхня межа малювання
                    int drawEndY = Math.Min(frameHeight, (int)(spriteCenterY + spriteHeight / 2f)); // Нижня межа малювання
                    int textureX = Math.Clamp((int)(((localX + halfWidth) / torch.WorldWidth) * torch.Texture.Width), 0, torch.Texture.Width - 1); // X-піксель PNG
                    int shade = CalculateTorchShade(correctedDistance); // Факел трохи затемнюється на відстані

                    for (int screenY = drawStartY; screenY < drawEndY; screenY++)
                    {
                        float texturePercentY = (screenY - (spriteCenterY - spriteHeight / 2f)) / spriteHeight; // Позиція по вертикалі PNG
                        int textureY = Math.Clamp((int)(texturePercentY * torch.Texture.Height), 0, torch.Texture.Height - 1); // Y-піксель PNG
                        int textureArgb = torch.Texture.GetArgb(textureX, textureY); // Колір PNG з прозорістю
                        int alpha = (textureArgb >> 24) & 255; // Прозорість пікселя

                        if (alpha < 15)
                        {
                            continue; // Прозорий фон факела не малюємо
                        }

                        Color shadedColor = ApplyShade(Color.FromArgb(textureArgb), shade); // Затемнюємо факел без втрати alpha
                        int frameIndex = screenY * frameWidth + screenX; // Індекс пікселя у кадрі
                        framePixels[frameIndex] = BlendArgb(framePixels[frameIndex], shadedColor.ToArgb()); // Накладаємо факел поверх сцени
                    }
                }
            }
        }

        private void RenderKnights(int[] framePixels, int frameWidth, int frameHeight, Player player, GameMap map, float projectionDistance, float[] wallDistances)
        {
            List<Knight> knights = new List<Knight>(map.Knights); // Копія списку, щоб сортувати без зміни карти
            knights.Sort((left, right) => CalculateDistanceSquared(right, player).CompareTo(CalculateDistanceSquared(left, player))); // Дальних рицарів малюємо першими

            float startAngle = player.Angle - FieldOfView / 2f; // Кут лівого краю камери
            float angleStep = FieldOfView / frameWidth; // Один промінь на кожну колонку екрана
            float cameraHeight = map.TileSize / 2f; // Висота очей гравця над підлогою
            float horizon = frameHeight / 2f; // Лінія горизонту сцени

            foreach (Knight knight in knights)
            {
                for (int screenX = 0; screenX < frameWidth; screenX++)
                {
                    float rayAngle = startAngle + screenX * angleStep; // Кут променя для поточної колонки
                    float rayDirectionX = MathF.Cos(rayAngle); // Напрямок променя по X
                    float rayDirectionY = MathF.Sin(rayAngle); // Напрямок променя по Y
                    float denominator = rayDirectionX * knight.NormalX + rayDirectionY * knight.NormalY; // Перетин з фіксованою площиною рицаря

                    if (denominator >= -0.01f)
                    {
                        continue; // Рицар не розвертається до гравця: зворотний бік не малюємо
                    }

                    float deltaX = knight.X - player.X; // Вектор від гравця до площини рицаря по X
                    float deltaY = knight.Y - player.Y; // Вектор від гравця до площини рицаря по Y
                    float rayDistance = (deltaX * knight.NormalX + deltaY * knight.NormalY) / denominator; // Дистанція до площини рицаря

                    if (rayDistance <= 1f || rayDistance > MaxRayDistance)
                    {
                        continue; // Рицар позаду, занадто близько або занадто далеко не малюється
                    }

                    float hitX = player.X + rayDirectionX * rayDistance; // X-точка перетину променя з площиною рицаря
                    float hitY = player.Y + rayDirectionY * rayDistance; // Y-точка перетину променя з площиною рицаря
                    float localX = (hitX - knight.X) * knight.TangentX + (hitY - knight.Y) * knight.TangentY; // Горизонтальна позиція всередині рицаря
                    float halfWidth = knight.WorldWidth / 2f; // Половина ширини рицаря у світі

                    if (localX < -halfWidth || localX > halfWidth)
                    {
                        continue; // Промінь пройшов поруч із рицарем
                    }

                    float correctedDistance = rayDistance * MathF.Cos(rayAngle - player.Angle); // Прибираємо риб'яче око для розміру рицаря

                    if (correctedDistance <= 1f || correctedDistance >= wallDistances[screenX])
                    {
                        continue; // Якщо перед рицарем є стіна, ця колонка не видима
                    }

                    float spriteHeight = knight.WorldHeight * projectionDistance / correctedDistance; // Висота рицаря після перспективи
                    float floorY = horizon + cameraHeight * projectionDistance / correctedDistance; // Підлога в точці, де стоїть рицар
                    float spriteTop = floorY - spriteHeight; // Верх рицаря, коли ступні стоять на підлозі
                    float spriteBottom = floorY; // Низ рицаря прив'язаний до підлоги
                    int drawStartY = Math.Max(0, (int)spriteTop); // Верхня межа рицаря в кадрі
                    int drawEndY = Math.Min(frameHeight, (int)spriteBottom); // Нижня межа рицаря в кадрі
                    int textureX = Math.Clamp((int)(((localX + halfWidth) / knight.WorldWidth) * knight.Texture.Width), 0, knight.Texture.Width - 1); // X-піксель PNG
                    int shade = CalculateKnightShade(correctedDistance); // Рицар темнішає залежно від відстані

                    for (int screenY = drawStartY; screenY < drawEndY; screenY++)
                    {
                        float texturePercentY = (screenY - spriteTop) / spriteHeight; // Позиція всередині PNG по Y
                        int textureY = Math.Clamp((int)(texturePercentY * knight.Texture.Height), 0, knight.Texture.Height - 1); // Y-піксель PNG
                        int textureArgb = knight.Texture.GetArgb(textureX, textureY); // Колір PNG з прозорістю
                        int alpha = (textureArgb >> 24) & 255; // Прозорість пікселя

                        if (alpha < 15)
                        {
                            continue; // Прозорий фон рицаря не малюємо
                        }

                        Color shadedColor = ApplyShade(Color.FromArgb(textureArgb), shade); // Затемнюємо рицаря без втрати alpha
                        int frameIndex = screenY * frameWidth + screenX; // Індекс пікселя у кадрі
                        framePixels[frameIndex] = BlendArgb(framePixels[frameIndex], shadedColor.ToArgb()); // Накладаємо рицаря поверх сцени
                    }
                }
            }
        }

        private RayHit CastRay(float startX, float startY, float angle, GameMap map)
        {
            float rayX = startX; // Поточна X-позиція променя
            float rayY = startY; // Поточна Y-позиція променя

            for (float distance = 0; distance < MaxRayDistance; distance += RayStep)
            {
                rayX = startX + MathF.Cos(angle) * distance; // Рух променя по X
                rayY = startY + MathF.Sin(angle) * distance; // Рух променя по Y

                int tileX = (int)(rayX / map.TileSize); // Клітинка карти по X
                int tileY = (int)(rayY / map.TileSize); // Клітинка карти по Y

                if (map.IsBlocking(rayX, rayY))
                {
                    bool isDoor = map.IsDoor(tileX, tileY) && !map.IsDoorOpen(tileX, tileY); // Визначаємо, чи це ще не відкрита повністю дверна панель
                    int textureX = CalculateTextureX(rayX, rayY, map.TileSize); // Визначаємо X-координату текстури
                    return new RayHit(new PointF(rayX, rayY), distance, isDoor, textureX); // Повертаємо hit з координатою текстури
                }
            }

            return new RayHit(new PointF(rayX, rayY), MaxRayDistance, false, 0); // Якщо перешкоду не знайдено, повертаємо максимальну відстань
        }

        private static void FillHorizonLine(int[] framePixels, int frameWidth, int frameHeight)
        {
            int horizon = frameHeight / 2; // Рядок між стелею і підлогою
            int rowStart = horizon * frameWidth; // Початок рядка горизонту в масиві
            int horizonColor = Color.FromArgb(55, 55, 55).ToArgb(); // Нейтральний темно-сірий колір горизонту

            for (int column = 0; column < frameWidth; column++)
            {
                framePixels[rowStart + column] = horizonColor; // Заповнюємо рядок, щоб не лишати чорну лінію
            }
        }

        private static void CopyFrameToBitmap(int[] framePixels, Bitmap bitmap)
        {
            Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height); // Область усього Bitmap
            BitmapData bitmapData = bitmap.LockBits(rectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb); // Блокуємо пам'ять Bitmap для швидкого запису

            try
            {
                int expectedStride = bitmap.Width * 4; // Кількість байтів в одному рядку без додаткового вирівнювання

                if (bitmapData.Stride == expectedStride)
                {
                    Marshal.Copy(framePixels, 0, bitmapData.Scan0, framePixels.Length); // Найшвидший варіант: копіюємо весь кадр одним блоком
                    return;
                }

                for (int row = 0; row < bitmap.Height; row++)
                {
                    IntPtr rowPointer = IntPtr.Add(bitmapData.Scan0, row * bitmapData.Stride); // Початок рядка в пам'яті Bitmap
                    Marshal.Copy(framePixels, row * bitmap.Width, rowPointer, bitmap.Width); // Копіюємо рядок, якщо Bitmap має нестандартний stride
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData); // Завжди розблоковуємо Bitmap після запису
            }
        }

        private static float CalculateDistanceSquared(WallTorch torch, Player player)
        {
            float deltaX = torch.X - player.X; // Відстань до факела по X
            float deltaY = torch.Y - player.Y; // Відстань до факела по Y

            return deltaX * deltaX + deltaY * deltaY; // Квадрат відстані достатній для сортування
        }

        private static float CalculateDistanceSquared(Knight knight, Player player)
        {
            float deltaX = knight.X - player.X; // Відстань до рицаря по X
            float deltaY = knight.Y - player.Y; // Відстань до рицаря по Y

            return deltaX * deltaX + deltaY * deltaY; // Квадрат відстані достатній для сортування
        }

        private static int CalculateWallShade(float distance)
        {
            int shade = 255 - (int)(distance * 0.35f); // Чим далі стіна, тим темніша
            return Math.Clamp(shade, 45, 220); // Обмежуємо яскравість, щоб стіни не зникали
        }

        private static int CalculateFloorShade(float distance)
        {
            int shade = 230 - (int)(distance * 0.25f); // Підлога темнішає з відстанню
            return Math.Clamp(shade, 35, 190); // Обмежуємо яскравість, щоб підлога не стала повністю чорною
        }

        private static int CalculateCeilingShade(float distance)
        {
            int shade = 255 - (int)(distance * 0.18f); // Стеля теж має легке затемнення на відстані
            return Math.Clamp(shade, 95, 235); // Тримаємо стелю читабельною
        }

        private static int CalculateKnightShade(float distance)
        {
            int shade = 255 - (int)(distance * 0.25f); // Рицар темнішає з відстанню
            return Math.Clamp(shade, 65, 230); // Не даємо рицарю стати повністю чорним
        }

        private static int CalculateTorchShade(float distance)
        {
            int shade = 255 - (int)(distance * 0.2f); // Факел темнішає м'якше, щоб полум'я лишалося помітним
            return Math.Clamp(shade, 90, 245); // Не робимо факел занадто темним
        }

        private static int CalculateTextureX(float rayX, float rayY, int tileSize)
        {
            float localX = rayX % tileSize; // X всередині клітинки карти
            float localY = rayY % tileSize; // Y всередині клітинки карти

            float distanceToVerticalEdge = MathF.Min(localX, tileSize - localX); // Відстань до вертикальної межі стіни
            float distanceToHorizontalEdge = MathF.Min(localY, tileSize - localY); // Відстань до горизонтальної межі стіни

            float texturePosition = distanceToVerticalEdge < distanceToHorizontalEdge
                ? localY
                : localX; // Для вертикальної стіни беремо Y, для горизонтальної - X

            return Math.Clamp((int)texturePosition, 0, WallTexture.Size - 1); // Повертаємо координату текстури 0..63
        }

        private static int PositiveModulo(int value, int modulo)
        {
            int result = value % modulo; // Звичайний залишок від ділення
            return result < 0 ? result + modulo : result; // Робимо результат додатним навіть для від'ємних координат
        }

        private static Color ApplyShade(Color color, int shade)
        {
            float factor = shade / 255f; // Коефіцієнт затемнення

            int red = (int)(color.R * factor); // Затемнюємо червоний канал
            int green = (int)(color.G * factor); // Затемнюємо зелений канал
            int blue = (int)(color.B * factor); // Затемнюємо синій канал

            return Color.FromArgb(color.A, red, green, blue); // Повертаємо фінальний колір зі збереженою прозорістю
        }

        private static int BlendArgb(int backgroundArgb, int foregroundArgb)
        {
            int alpha = (foregroundArgb >> 24) & 255; // Прозорість пікселя факела

            if (alpha >= 255)
            {
                return foregroundArgb; // Непрозорий піксель замінює фон
            }

            int inverseAlpha = 255 - alpha; // Частка фонового пікселя після накладання
            int foregroundRed = (foregroundArgb >> 16) & 255; // Червоний канал факела
            int foregroundGreen = (foregroundArgb >> 8) & 255; // Зелений канал факела
            int foregroundBlue = foregroundArgb & 255; // Синій канал факела
            int backgroundRed = (backgroundArgb >> 16) & 255; // Червоний канал фону
            int backgroundGreen = (backgroundArgb >> 8) & 255; // Зелений канал фону
            int backgroundBlue = backgroundArgb & 255; // Синій канал фону

            int red = (foregroundRed * alpha + backgroundRed * inverseAlpha) / 255; // Змішаний червоний канал
            int green = (foregroundGreen * alpha + backgroundGreen * inverseAlpha) / 255; // Змішаний зелений канал
            int blue = (foregroundBlue * alpha + backgroundBlue * inverseAlpha) / 255; // Змішаний синій канал

            return Color.FromArgb(255, red, green, blue).ToArgb(); // Кадр залишається повністю непрозорим
        }

        private readonly struct RayHit
        {
            public RayHit(PointF point, float distance, bool isDoor, int textureX)
            {
                Point = point; // Точка зіткнення променя зі стіною або дверима
                Distance = distance; // Відстань від гравця до перешкоди
                IsDoor = isDoor; // Чи є перешкода дверима
                TextureX = textureX; // X-координата в текстурі
            }

            public PointF Point { get; } // Координата зіткнення
            public float Distance { get; } // Довжина променя до перешкоди
            public bool IsDoor { get; } // Ознака, що промінь влучив у двері
            public int TextureX { get; } // X-координата в текстурі
        }
    }
}
