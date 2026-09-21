using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public class PlayerInput
{
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

        Horizontal = ReadHorizontalInput(keyboardState);
        Vertical = ReadVerticalInput(keyboardState);
        IsJumpHeld = keyboardState.IsKeyDown(Keys.Space);
        JumpWasPressed = WasKeyPressed(keyboardState, Keys.Space);
        DashWasPressed = WasKeyPressed(keyboardState, Keys.LeftShift);

        _previousKeyboardState = keyboardState;
    }

    private int ReadHorizontalInput(KeyboardState keyboardState)
    {
        return ReadAxis(keyboardState, Keys.A, Keys.D, ref _lastHorizontalKeyPressed);
    }

    private int ReadVerticalInput(KeyboardState keyboardState)
    {
        return ReadAxis(keyboardState, Keys.W, Keys.S, ref _lastVerticalKeyPressed);
    }

    private int ReadAxis(
        KeyboardState keyboardState,
        Keys negativeKey,
        Keys positiveKey,
        ref int lastPressed)
    {
        bool negativeDown = keyboardState.IsKeyDown(negativeKey);
        bool positiveDown = keyboardState.IsKeyDown(positiveKey);
        bool negativePressed = WasKeyPressed(keyboardState, negativeKey);
        bool positivePressed = WasKeyPressed(keyboardState, positiveKey);

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

    private bool WasKeyPressed(KeyboardState keyboardState, Keys key)
    {
        return keyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }
}
