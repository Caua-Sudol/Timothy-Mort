using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class ExitDoor
{
    private const float RiseSpeed = 768.0f;
    private Rectangle? _bounds;
    private Vector2 _destination;

    public bool IsOpen { get; private set; }
    public Vector2 Position { get; private set; }

    public Rectangle? Collider => IsOpen || !_bounds.HasValue
        ? null
        : new Rectangle(
            (int)Position.X,
            (int)Position.Y,
            _bounds.Value.Width,
            _bounds.Value.Height);

    public void Configure(Rectangle? bounds, Vector2 destination)
    {
        _bounds = bounds;
        _destination = destination;
    }

    public void Reset(bool verticalRouteRevealed, bool isOpen)
    {
        IsOpen = isOpen;
        Position = verticalRouteRevealed ? _destination : StartPosition;
    }

    public void Update(bool shouldRise, double deltaTime)
    {
        if (!shouldRise || IsOpen) return;

        Vector2 delta = _destination - Position;
        float distance = delta.Length();
        if (distance <= 0.0f) return;

        float step = RiseSpeed * (float)deltaTime;
        Position = distance <= step
            ? _destination
            : Position + delta / distance * step;
    }

    public void SetOpen(bool isOpen) => IsOpen = isOpen;

    public void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        if (Collider.HasValue)
            spriteBatch.Draw(texture, Collider.Value, Color.Red);
    }

    private Vector2 StartPosition => !_bounds.HasValue
        ? Vector2.Zero
        : new Vector2(_bounds.Value.X, _bounds.Value.Y);
}
