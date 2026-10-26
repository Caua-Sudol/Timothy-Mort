using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

// Mouse state for menus, in the logical 1920x1080 space. Updated once per frame by Game1.
public static class UiPointer
{
    private static MouseState _previousMouseState;

    public static Vector2 Position { get; private set; }
    public static bool Moved { get; private set; }
    public static bool LeftClicked { get; private set; }

    public static void Update(MouseState mouseState, Rectangle presentation, bool isActive)
    {
        if (presentation.Width > 0 && presentation.Height > 0)
        {
            Position = new Vector2(
                (mouseState.X - presentation.X) * GameViewport.LogicalWidth / (float)presentation.Width,
                (mouseState.Y - presentation.Y) * GameViewport.LogicalHeight / (float)presentation.Height);
        }

        Moved = isActive &&
                (mouseState.X != _previousMouseState.X || mouseState.Y != _previousMouseState.Y);
        LeftClicked = isActive &&
                      mouseState.LeftButton == ButtonState.Pressed &&
                      _previousMouseState.LeftButton == ButtonState.Released;
        _previousMouseState = mouseState;
    }

    // Moving over an option selects it; clicking it selects it and returns true (same as Enter).
    public static bool UpdateOption(Rectangle bounds, int index, ref int selectedIndex)
    {
        if (!bounds.Contains(Position))
            return false;

        if (Moved || LeftClicked)
            selectedIndex = index;

        return LeftClicked;
    }
}
