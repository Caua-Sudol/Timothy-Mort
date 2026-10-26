using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

// Shared menu keys: WASD and arrows navigate; Enter or Space confirm.
public static class MenuInput
{
    public static bool Up(KeyboardState current, KeyboardState previous) =>
        Pressed(current, previous, Keys.W, Keys.Up);

    public static bool Down(KeyboardState current, KeyboardState previous) =>
        Pressed(current, previous, Keys.S, Keys.Down);

    public static bool Left(KeyboardState current, KeyboardState previous) =>
        Pressed(current, previous, Keys.A, Keys.Left);

    public static bool Right(KeyboardState current, KeyboardState previous) =>
        Pressed(current, previous, Keys.D, Keys.Right);

    public static bool Confirm(KeyboardState current, KeyboardState previous) =>
        Pressed(current, previous, Keys.Enter, Keys.Space);

    private static bool Pressed(KeyboardState current, KeyboardState previous, params Keys[] keys) =>
        keys.Any(key => current.IsKeyDown(key) && previous.IsKeyUp(key));
}
