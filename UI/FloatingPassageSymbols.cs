using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public static class FloatingPassageSymbols
{
    public static void Draw(SpriteBatch batch, Texture2D atlas, Rectangle bounds, double time)
    {
        const int frameSize = 128;
        int columns = atlas.Width / frameSize;
        if (columns < 1 || bounds.Width <= 0 || bounds.Height <= 0) return;
        float size = Math.Min(56f, Math.Min(bounds.Width, bounds.Height) * 0.65f);
        for (int i = 0; i < 3; i++)
        {
            float phase = (float)time * 0.9f + i * 2.1f;
            float u = 0.5f + MathF.Sin(phase) * 0.12f;
            float v = 0.08f + i * 0.42f + MathF.Cos(phase * 0.8f) * 0.025f;
            Vector2 position = new(
                bounds.X + size / 2 + u * (bounds.Width - size),
                bounds.Y + size / 2 + v * (bounds.Height - size));
            int frame = (i * 2 + (int)(time / 1.5)) % 9;
            Rectangle source = new(frame % columns * frameSize, frame / columns * frameSize, frameSize, frameSize);
            // Dark outer stroke and thicker ivory core stay legible on both sky and forest.
            for (int dy = -3; dy <= 3; dy++)
            for (int dx = -3; dx <= 3; dx++)
                batch.Draw(atlas, position + new Vector2(dx, dy), source,
                    new Color(17, 20, 26), 0, new Vector2(64),
                    size / frameSize, SpriteEffects.None, 0);
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                batch.Draw(atlas, position + new Vector2(dx, dy), source,
                    new Color(255, 237, 182), 0, new Vector2(64),
                    size / frameSize, SpriteEffects.None, 0);
        }
    }
}
