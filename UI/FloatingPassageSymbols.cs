using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

// Smoke of symbols rising from a passage. Each particle is derived from time only,
// so no per-passage state is needed.
public static class FloatingPassageSymbols
{
    private const int FrameSize = 128;
    private const int FrameCount = 9;
    private const int ParticleCount = 24;
    private const float LifetimeSeconds = 2.6f;
    // How far above the passage a symbol travels, relative to the passage height.
    private const float RiseHeightFactor = 1.15f;
    private const float SwayPixels = 14f;
    private const float MaxSymbolSize = 80f;
    // Symbols may start this far beyond each side of the passage, relative to its width.
    private const float SideSpreadFactor = 0.25f;
    private const float MaxAlpha = 0.9f;
    private static readonly Color OutlineColor = new(17, 20, 26);
    private static readonly Color CoreColor = new(255, 237, 182);
    private static readonly Vector2[] OutlineOffsets =
    {
        new(-3, 0), new(3, 0), new(0, -3), new(0, 3),
        new(-2, -2), new(2, -2), new(-2, 2), new(2, 2)
    };

    public static void Draw(SpriteBatch batch, Texture2D atlas, Rectangle bounds, double time)
    {
        int columns = atlas.Width / FrameSize;
        if (columns < 1 || bounds.Width <= 0 || bounds.Height <= 0) return;
        float maxSize = MaxSymbolSize;
        float spread = bounds.Width * SideSpreadFactor;
        Vector2 origin = new(FrameSize / 2f);

        for (int i = 0; i < ParticleCount; i++)
        {
            // Spread the particles evenly along the cycle so the smoke is continuous.
            double localTime = time + i * LifetimeSeconds / ParticleCount;
            int cycle = (int)Math.Floor(localTime / LifetimeSeconds);
            float t = (float)(localTime / LifetimeSeconds - cycle);
            float randomX = Hash(i, cycle, 1);
            float randomSize = Hash(i, cycle, 2);
            float randomSpin = Hash(i, cycle, 3) * 2f - 1f;
            int frame = (int)(Hash(i, cycle, 4) * FrameCount) % FrameCount;

            float fadeIn = MathHelper.Clamp(t / 0.2f, 0f, 1f);
            float fadeOut = MathHelper.Clamp((1f - t) / 0.5f, 0f, 1f);
            float alpha = fadeIn * fadeOut * MaxAlpha * (0.55f + 0.45f * randomSize);
            if (alpha <= 0.01f) continue;

            float size = maxSize * (0.45f + 0.35f * randomSize) * (0.75f + 0.45f * t);
            float sway = MathF.Sin(t * MathF.PI * 2f + i * 1.7f) * SwayPixels * t;
            Vector2 position = new(
                bounds.X - spread + randomX * (bounds.Width + spread * 2f) + sway,
                bounds.Bottom - size / 2f - t * bounds.Height * RiseHeightFactor);
            float rotation = randomSpin * 0.6f * t;
            float scale = size / FrameSize;
            Rectangle source = new(frame % columns * FrameSize, frame / columns * FrameSize, FrameSize, FrameSize);

            foreach (Vector2 offset in OutlineOffsets)
                batch.Draw(atlas, position + offset, source, OutlineColor * alpha,
                    rotation, origin, scale, SpriteEffects.None, 0);
            batch.Draw(atlas, position, source, CoreColor * alpha,
                rotation, origin, scale, SpriteEffects.None, 0);
        }
    }

    // Cheap deterministic random in [0, 1) for a particle, its cycle and a channel.
    private static float Hash(int particle, int cycle, int channel)
    {
        unchecked
        {
            uint h = (uint)(particle * 73856093) ^ (uint)(cycle * 19349663) ^ (uint)(channel * 83492791);
            h ^= h >> 13;
            h *= 0x5bd1e995;
            h ^= h >> 15;
            return (h & 0xFFFFFF) / (float)0x1000000;
        }
    }
}
