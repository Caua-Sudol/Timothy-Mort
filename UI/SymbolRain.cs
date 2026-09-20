using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

// Shared decorative effect. The caller owns SpriteBatch.Begin/End and the texture.
public sealed class SymbolRain
{
    private const int FrameSize = 128;
    private const int FrameCount = 9;
    private readonly Texture2D _atlas;
    private readonly Vector2 _dimensions;
    private readonly Particle[] _particles;
    private readonly Random _random = new Random();
    private readonly float _opacity;
    private readonly int _columns;

    private struct Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size, Rotation, Spin, Phase, Sway, Frequency;
        public float FrameTime, FrameDuration, Opacity;
        public int Frame;
    }

    public SymbolRain(Texture2D atlas, Vector2 dimensions, int count = 160, float opacity = 0.65f)
    {
        if (atlas.Width % FrameSize != 0 || atlas.Height % FrameSize != 0 ||
            (atlas.Width / FrameSize) * (atlas.Height / FrameSize) != FrameCount)
            throw new ArgumentException("icons.png deve conter nove células de 128 x 128.", nameof(atlas));
        _atlas = atlas;
        _columns = atlas.Width / FrameSize;
        _dimensions = dimensions;
        _opacity = opacity;
        _particles = new Particle[count];
        Reset();
    }

    public void Reset()
    {
        for (int i = 0; i < _particles.Length; i++)
            _particles[i] = CreateParticle(true);
    }

    private Particle CreateParticle(bool spread)
    {
        return new Particle
        {
            Position = new Vector2(Next(0, _dimensions.X), spread ? Next(0, _dimensions.Y) : -64),
            Velocity = new Vector2(Next(-12, 12), Next(18, 48)),
            Size = Next(24, 44),
            Rotation = Next(-0.4f, 0.4f),
            Spin = Next(-0.15f, 0.15f),
            Phase = Next(0, MathHelper.TwoPi),
            Sway = Next(8, 24),
            Frequency = Next(0.4f, 1.1f),
            Frame = _random.Next(FrameCount),
            FrameTime = Next(0, 0.5f),
            FrameDuration = Next(0.65f, 1.2f),
            Opacity = Next(0.65f, 1)
        };
    }

    public void Update(double deltaTime)
    {
        float elapsed = (float)deltaTime;
        if (!float.IsFinite(elapsed) || elapsed <= 0) return;
        for (int i = 0; i < _particles.Length; i++)
        {
            ref Particle particle = ref _particles[i];
            particle.Phase = (particle.Phase + elapsed * particle.Frequency) % MathHelper.TwoPi;
            particle.Position += (particle.Velocity +
                new Vector2(MathF.Sin(particle.Phase) * particle.Sway, 0)) * elapsed;
            particle.Rotation += particle.Spin * elapsed;
            particle.FrameTime += elapsed;
            int steps = (int)(particle.FrameTime / particle.FrameDuration);
            particle.Frame = (particle.Frame + steps) % FrameCount;
            particle.FrameTime %= particle.FrameDuration;
            if (particle.Position.Y > _dimensions.Y + particle.Size)
                particle = CreateParticle(false);
            else if (particle.Position.X < -particle.Size)
                particle.Position.X = _dimensions.X + particle.Size;
            else if (particle.Position.X > _dimensions.X + particle.Size)
                particle.Position.X = -particle.Size;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (Particle particle in _particles)
        {
            var source = new Rectangle(
                particle.Frame % _columns * FrameSize,
                particle.Frame / _columns * FrameSize, FrameSize, FrameSize);
            spriteBatch.Draw(_atlas, particle.Position, source,
                new Color(166, 155, 174) * (_opacity * particle.Opacity), particle.Rotation,
                new Vector2(FrameSize / 2f), particle.Size / FrameSize, SpriteEffects.None, 0);
        }
    }

    private float Next(float minimum, float maximum) =>
        minimum + (float)_random.NextDouble() * (maximum - minimum);
}
