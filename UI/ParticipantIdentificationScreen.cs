using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public enum SessionStartAction
{
    NONE = 0,
    CONTINUE = 1,
    NEW_SESSION = 2,
    BACK = 3
}

public sealed class SessionStartScreen
{
    private const string TitleText = "Sessão em andamento";
    private const string InstructionText =
        "Foi encontrada uma sessão incompleta nesta máquina";
    private const string ReplacementWarningText =
        "Uma nova sessão encerra a retomada da sessão atual";
    private const string NavigationText =
        "W/S ou setas para navegar    Enter para selecionar    Esc para sair";

    private readonly SpriteFont _font;
    private readonly Vector2 _center;
    private string _activeSessionCode;
    private string _errorMessage;
    private int _selectedOptionIndex;

    public SessionStartScreen(SpriteFont font, Vector2 center)
    {
        _font = font;
        _center = center;
    }

    public void Open(string activeSessionCode)
    {
        _activeSessionCode = activeSessionCode;
        _errorMessage = null;
        _selectedOptionIndex = 0;
    }

    public void SetError(string message)
    {
        _errorMessage = message;
    }

    public SessionStartAction Update(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState)
    {
        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.Escape))
        {
            return SessionStartAction.BACK;
        }

        if (string.IsNullOrEmpty(_activeSessionCode))
        {
            return SessionStartAction.NONE;
        }

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.W) ||
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.Up) ||
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.S) ||
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.Down))
        {
            _selectedOptionIndex = 1 - _selectedOptionIndex;
        }

        if (!KeyWasPressed(keyboardState, previousKeyboardState, Keys.Enter))
        {
            return SessionStartAction.NONE;
        }

        return _selectedOptionIndex == 0
            ? SessionStartAction.CONTINUE
            : SessionStartAction.NEW_SESSION;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        DrawCenteredText(TitleText, _center.Y - 180.0f, Color.White, spriteBatch);
        DrawCenteredText(InstructionText, _center.Y - 100.0f, Color.LightGray, spriteBatch);

        if (!string.IsNullOrEmpty(_activeSessionCode))
        {
            DrawCenteredText(
                $"Sessão: {_activeSessionCode}",
                _center.Y - 30.0f,
                Color.LightGray,
                spriteBatch);
            DrawOption("Continuar", 0, _center.Y + 60.0f, spriteBatch);
            DrawOption("Nova sessão", 1, _center.Y + 120.0f, spriteBatch);
            DrawCenteredText(
                ReplacementWarningText,
                _center.Y + 190.0f,
                Color.Gray,
                spriteBatch);
        }

        if (!string.IsNullOrEmpty(_errorMessage))
        {
            DrawCenteredText(_errorMessage, _center.Y + 250.0f, Color.OrangeRed, spriteBatch);
        }

        DrawCenteredText(NavigationText, _center.Y + 330.0f, Color.LightGray, spriteBatch);
        spriteBatch.End();
    }

    private void DrawOption(
        string text,
        int optionIndex,
        float y,
        SpriteBatch spriteBatch)
    {
        bool isSelected = optionIndex == _selectedOptionIndex;
        DrawCenteredText(
            isSelected ? $"> {text} <" : text,
            y,
            isSelected ? Color.Yellow : Color.White,
            spriteBatch);
    }

    private void DrawCenteredText(
        string text,
        float y,
        Color color,
        SpriteBatch spriteBatch)
    {
        Vector2 textSize = _font.MeasureString(text);
        spriteBatch.DrawString(
            _font,
            text,
            new Vector2(_center.X - textSize.X / 2.0f, y),
            color);
    }

    private static bool KeyWasPressed(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState,
        Keys key)
    {
        return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }
}
