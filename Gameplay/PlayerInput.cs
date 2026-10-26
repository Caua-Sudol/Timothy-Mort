using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public class PlayerInput
{
    // Two-hand friendly layouts, all active at once:
    // arrows + C/X (Celeste style), WASD + K/J (actions on the right hand), WASD + Space/Shift.
    private static readonly Keys[] LeftKeys = { Keys.A, Keys.Left };
    private static readonly Keys[] RightKeys = { Keys.D, Keys.Right };
    private static readonly Keys[] UpKeys = { Keys.W, Keys.Up };
    private static readonly Keys[] DownKeys = { Keys.S, Keys.Down };
    private static readonly Keys[] JumpKeys = { Keys.Space, Keys.C, Keys.K };
    private static readonly Keys[] DashKeys = { Keys.LeftShift, Keys.RightShift, Keys.X, Keys.J };

    private KeyboardState _previousKeyboardState;
    private int _lastHorizontalKeyPressed;
    private int _lastVerticalKeyPressed;

    public int Horizontal { get; private set; }
    public int Vertical { get; private set; }
    public bool IsJumpHeld { get; private set; }
    public bool JumpWasPressed { get; private set; }
    public bool DashWasPressed { get; private set; }

    public void Update()
    {
        KeyboardState keyboardState = Keyboard.GetState();

        Horizontal = ReadAxis(keyboardState, LeftKeys, RightKeys, ref _lastHorizontalKeyPressed);
        Vertical = ReadAxis(keyboardState, UpKeys, DownKeys, ref _lastVerticalKeyPressed);
        IsJumpHeld = JumpKeys.Any(keyboardState.IsKeyDown);
        JumpWasPressed = AnyWasPressed(keyboardState, JumpKeys);
        DashWasPressed = AnyWasPressed(keyboardState, DashKeys);

        _previousKeyboardState = keyboardState;
    }

    private int ReadAxis(
        KeyboardState keyboardState,
        Keys[] negativeKeys,
        Keys[] positiveKeys,
        ref int lastPressed)
    {
        bool negativeDown = negativeKeys.Any(keyboardState.IsKeyDown);
        bool positiveDown = positiveKeys.Any(keyboardState.IsKeyDown);
        bool negativePressed = AnyWasPressed(keyboardState, negativeKeys);
        bool positivePressed = AnyWasPressed(keyboardState, positiveKeys);

        if (negativePressed && positivePressed)
            lastPressed = 0; // Same update: neither direction has priority.
        else if (negativePressed)
            lastPressed = -1;
        else if (positivePressed)
            lastPressed = 1;

        if (negativeDown && positiveDown)
            return lastPressed;
        if (negativeDown)
            return -1;
        if (positiveDown)
            return 1;
        return 0;
    }

    // Treat keys that are already held as old presses (used when gameplay resumes).
    public void Resync() => _previousKeyboardState = Keyboard.GetState();

    private bool AnyWasPressed(KeyboardState keyboardState, Keys[] keys)
    {
        return keys.Any(key => keyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key));
    }
}
