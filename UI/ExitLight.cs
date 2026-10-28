using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

// Soft warm light coming from the room edge where an exit is. Draw inside an additive batch.
public static class ExitLight
{
    private const int TextureWidth = 64;
    private const int TextureHeight = 128;
    // How far the light reaches into the room, and how much wider than the exit it spreads.
    private const int DepthPixels = 280;
    private const int SpreadPixels = 64;
    private const float Intensity = 0.55f;
    private const float PulseAmount = 0.2f;
    private const double PulsePeriodSeconds = 2.4;
    private static readonly Color LightColor = new(255, 210, 140);
    // Blocked passages: the same shape as a shadow (drawn with alpha blending, not additive).
    private const float DarkIntensity = 0.7f;

    private static Texture2D _gradient;

    public static void Draw(SpriteBatch batch, Rectangle exit, int roomWidth, int roomHeight, double time,
        bool dark = false)
    {
        _gradient ??= CreateGradient(batch.GraphicsDevice);
        float pulse = 1f - PulseAmount * 0.5f * (1f + (float)Math.Sin(time / PulsePeriodSeconds * Math.PI * 2.0));
        var color = dark
            ? new Color((byte)0, (byte)0, (byte)0, (byte)(255 * DarkIntensity * pulse))
            : new Color(LightColor.R, LightColor.G, LightColor.B, (byte)(255 * Intensity * pulse));

        // The texture is brightest at its bottom row; rotate it so that row faces the room edge.
        int toRight = roomWidth - exit.Right, toLeft = exit.Left, toBottom = roomHeight - exit.Bottom, toTop = exit.Top;
        int nearest = Math.Min(Math.Min(toRight, toLeft), Math.Min(toBottom, toTop));
        Rectangle area;
        float rotation;
        if (nearest == toBottom)
        {
            area = new Rectangle(exit.X - SpreadPixels, Math.Min(exit.Bottom, roomHeight) - DepthPixels,
                exit.Width + SpreadPixels * 2, DepthPixels);
            rotation = 0f;
        }
        else if (nearest == toTop)
        {
            area = new Rectangle(exit.X - SpreadPixels, Math.Max(exit.Top, 0),
                exit.Width + SpreadPixels * 2, DepthPixels);
            rotation = MathHelper.Pi;
        }
        else if (nearest == toRight)
        {
            area = new Rectangle(Math.Min(exit.Right, roomWidth) - DepthPixels, exit.Y - SpreadPixels,
                DepthPixels, exit.Height + SpreadPixels * 2);
            rotation = -MathHelper.PiOver2;
        }
        else
        {
            area = new Rectangle(Math.Max(exit.Left, 0), exit.Y - SpreadPixels,
                DepthPixels, exit.Height + SpreadPixels * 2);
            rotation = MathHelper.PiOver2;
        }

        bool sideways = rotation == -MathHelper.PiOver2 || rotation == MathHelper.PiOver2;
        var scale = sideways
            ? new Vector2(area.Height / (float)TextureWidth, area.Width / (float)TextureHeight)
            : new Vector2(area.Width / (float)TextureWidth, area.Height / (float)TextureHeight);
        batch.Draw(_gradient, area.Center.ToVector2(), null, color, rotation,
            new Vector2(TextureWidth / 2f, TextureHeight / 2f), scale, SpriteEffects.None, 0f);
    }

    // White texture whose alpha fades from the bottom row upward and toward both sides.
    private static Texture2D CreateGradient(GraphicsDevice graphicsDevice)
    {
        var pixels = new Color[TextureWidth * TextureHeight];
        for (int y = 0; y < TextureHeight; y++)
        for (int x = 0; x < TextureWidth; x++)
        {
            float depth = 1f - (y + 0.5f) / TextureHeight;
            float across = (float)Math.Sin((x + 0.5f) / TextureWidth * Math.PI);
            float alpha = (1f - depth) * (1f - depth) * across * across;
            pixels[y * TextureWidth + x] = new Color(255, 255, 255, (int)(alpha * 255));
        }
        var texture = new Texture2D(graphicsDevice, TextureWidth, TextureHeight);
        texture.SetData(pixels);
        return texture;
    }
}
