using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace DontLikePoetry;

// Authored as rectangles in the Tiled layers plataformaMovel / plataformaQuebravel.
public sealed class DynamicPlatform
{
    private readonly Rectangle _start;
    private readonly Vector2 _travel;
    private readonly float _period;
    private readonly float _breakDelay;
    private readonly bool _breakable;
    private float _time;
    private float _remaining;
    private bool _triggered;
    public Rectangle Bounds { get; private set; }
    public bool Active { get; private set; } = true;
    public bool IsBreakable => _breakable;
    public float BreakProgress => _triggered ? 1 - _remaining / _breakDelay : 0;

    public DynamicPlatform(Rectangle bounds, IDictionary<string, string> properties, bool breakable)
    {
        _start = bounds;
        _travel = new Vector2(Read(properties, "deslocamentoX", 128), Read(properties, "deslocamentoY", 0));
        _period = Math.Max(1, Read(properties, "periodo", 3));
        _breakDelay = Math.Max(0.2f, Read(properties, "tempoQuebra", 0.65f));
        _breakable = breakable;
        Reset();
    }

    private static float Read(IDictionary<string, string> properties, string name, float fallback) =>
        properties.TryGetValue(name, out string value) &&
        float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float result) &&
        float.IsFinite(result) ? result : fallback;

    public void Reset()
    {
        Bounds = _start;
        Active = true;
        _time = 0;
        _remaining = _breakDelay;
        _triggered = false;
    }

    public void Update(Player player, double deltaTime)
    {
        if (!Active) return;
        bool riding = player.Velocity.Y >= 0 && player.HitBox.Right > Bounds.Left &&
            player.HitBox.Left < Bounds.Right && Math.Abs(player.HitBox.Bottom - Bounds.Top) <= 2;
        float dt = (float)deltaTime;
        if (_breakable)
        {
            if (riding) _triggered = true;
            if (_triggered)
            {
                _remaining -= dt;
                if (_remaining <= 0) Active = false;
            }
            return;
        }
        Rectangle previous = Bounds;
        _time = (_time + dt) % _period;
        float amount = (1 - MathF.Cos(_time / _period * MathHelper.TwoPi)) * 0.5f;
        Bounds = new Rectangle(_start.X + (int)(_travel.X * amount),
            _start.Y + (int)(_travel.Y * amount), _start.Width, _start.Height);
        if (riding)
            player.Position += new Vector2(Bounds.X - previous.X, Bounds.Y - previous.Y);
    }
}
