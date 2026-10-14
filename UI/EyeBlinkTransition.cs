using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

// Screen-space eyelids; applies the change once, while completely closed.
public sealed class EyeBlinkTransition
{
    private const double CloseSeconds = 0.26, HoldSeconds = 0.07, OpenSeconds = 0.32;
    private double _time;
    private Action _onClosed;
    private bool _applied;
    public bool Active { get; private set; }

    public void Start(Action onClosed)
    {
        if (Active) throw new InvalidOperationException("A blink is already active.");
        _time = 0;
        _applied = false;
        _onClosed = onClosed;
        Active = true;
    }

    public void Update(double seconds)
    {
        if (!Active) return;
        _time += Math.Max(0, seconds);
        if (!_applied && _time >= CloseSeconds)
        {
            // Keep a fully closed frame even after a long update.
            _time = CloseSeconds;
            _applied = true;
            Action action = _onClosed;
            _onClosed = null;
            action?.Invoke();
        }
        if (_time >= CloseSeconds + HoldSeconds + OpenSeconds) Active = false;
    }

    public void Draw(SpriteBatch batch, Texture2D pixel, int width, int height)
    {
        if (!Active) return;
        float closure = _time < CloseSeconds
            ? (float)(_time / CloseSeconds)
            : _time < CloseSeconds + HoldSeconds ? 1f
            : 1f - (float)((_time - CloseSeconds - HoldSeconds) / OpenSeconds);
        closure = MathHelper.SmoothStep(0, 1, MathHelper.Clamp(closure, 0, 1));
        batch.Begin();
        // Narrow strips describe an arched opening, with no fade or extra textures.
        for (int x = 0; x < width; x += 4)
        {
            float normalizedX = (x + 2f) / width * 2f - 1f;
            float arch = MathF.Sqrt(Math.Max(0, 1 - normalizedX * normalizedX));
            float openHalf = height * (1 - closure) * (0.5f + arch * 0.5f);
            int covered = Math.Clamp((int)MathF.Ceiling(height * 0.5f - openHalf), 0, (height + 1) / 2);
            int stripWidth = Math.Min(4, width - x);
            batch.Draw(pixel, new Rectangle(x, 0, stripWidth, covered), Color.Black);
            batch.Draw(pixel, new Rectangle(x, height - covered, stripWidth, covered), Color.Black);
        }
        batch.End();
    }
}
