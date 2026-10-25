using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class DamageHud
{
    private const int Margin = 32;
    private const int HeartSize = 52;
    private const int EmphasizedHeartSize = 64;
    private const int OutlinePixels = 3;
    private static readonly Color FullFillColor = new(214, 38, 52);
    private static readonly Color FullStrokeColor = new(60, 8, 16);
    private static readonly Color EmptyFillColor = new Color(24, 22, 28) * 0.7f;
    private static readonly Color EmptyStrokeColor = new(190, 182, 170);
    private static readonly Color OutlineColor = new(12, 12, 16);

    private Texture2D _fullHeart;
    private Texture2D _emptyHeart;
    private Texture2D _heartSilhouette;

    public void LoadContent(ContentManager content)
    {
        Texture2D icon = content.Load<Texture2D>("ui/icons/coracao");
        var pixels = new Color[icon.Width * icon.Height];
        icon.GetData(pixels);
        bool[] interior = FindInterior(pixels, icon.Width, icon.Height);

        _fullHeart = BuildHeart(icon, pixels, interior, FullFillColor, FullStrokeColor);
        _emptyHeart = BuildHeart(icon, pixels, interior, EmptyFillColor, EmptyStrokeColor);
        _heartSilhouette = BuildHeart(icon, pixels, interior, Color.White, Color.White);
    }

    public void Draw(SpriteBatch spriteBatch, int currentHealth, int maximumHealth,
        SolutionVisualModule visualModules)
    {
        if (_fullHeart == null || maximumHealth <= 0) return;
        bool emphasizeHud = visualModules.HasFlag(SolutionVisualModule.DAMAGE_HUD_EMPHASIS);
        int size = emphasizeHud ? EmphasizedHeartSize : HeartSize;
        int spacing = emphasizeHud ? 16 : 12;
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        for (int index = 0; index < maximumHealth; index++)
        {
            var bounds = new Rectangle(Margin + index * (size + spacing), Margin, size, size);
            // Dark outline around the whole heart keeps it readable on any background.
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                if (dx != 0 || dy != 0)
                    spriteBatch.Draw(_heartSilhouette,
                        new Rectangle(bounds.X + dx * OutlinePixels, bounds.Y + dy * OutlinePixels,
                            size, size), OutlineColor);
            spriteBatch.Draw(index < currentHealth ? _fullHeart : _emptyHeart, bounds, Color.White);
        }
        spriteBatch.End();
    }

    // Transparent pixels that cannot be reached from the image border are inside the heart.
    // If the stroke has a gap, nothing is marked and the heart simply stays unfilled.
    private static bool[] FindInterior(Color[] pixels, int width, int height)
    {
        var outside = new bool[pixels.Length];
        var queue = new Queue<int>();
        for (int x = 0; x < width; x++)
        {
            queue.Enqueue(x);
            queue.Enqueue((height - 1) * width + x);
        }
        for (int y = 0; y < height; y++)
        {
            queue.Enqueue(y * width);
            queue.Enqueue(y * width + width - 1);
        }

        while (queue.Count > 0)
        {
            int index = queue.Dequeue();
            if (outside[index] || IsStroke(pixels[index])) continue;
            outside[index] = true;
            int x = index % width;
            int y = index / width;
            if (x > 0) queue.Enqueue(index - 1);
            if (x < width - 1) queue.Enqueue(index + 1);
            if (y > 0) queue.Enqueue(index - width);
            if (y < height - 1) queue.Enqueue(index + width);
        }

        var interior = new bool[pixels.Length];
        for (int index = 0; index < pixels.Length; index++)
            interior[index] = !outside[index] && !IsStroke(pixels[index]);
        return interior;
    }

    private static bool IsStroke(Color pixel) => pixel.A >= 128;

    private static Texture2D BuildHeart(Texture2D icon, Color[] source, bool[] interior,
        Color fill, Color stroke)
    {
        var pixels = new Color[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            float alpha = source[index].A / 255f;
            // Stroke over fill inside the heart; plain stroke on the outer anti-aliased edge.
            pixels[index] = interior[index] || (IsStroke(source[index]) && TouchesInterior(index, interior, icon.Width))
                ? Color.Lerp(fill, stroke, alpha)
                : stroke * alpha;
        }

        var texture = new Texture2D(icon.GraphicsDevice, icon.Width, icon.Height);
        texture.SetData(pixels);
        return texture;
    }

    private static bool TouchesInterior(int index, bool[] interior, int width)
    {
        int x = index % width;
        return x > 0 && interior[index - 1] ||
            x < width - 1 && interior[index + 1] ||
            index >= width && interior[index - width] ||
            index + width < interior.Length && interior[index + width];
    }
}
