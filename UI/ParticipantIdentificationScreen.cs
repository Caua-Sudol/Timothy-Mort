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
    private const float ContinueOptionY = -5.0f;
    private const float NewSessionOptionY = 125.0f;
    private const int OptionHitWidth = 480;
    private const int OptionHitHeight = 115;

    private readonly SpriteFont _font;
    private readonly SpriteFont _titleFont;
    private readonly Texture2D _selector;
    private static readonly Color BackgroundColor = new Color(21, 25, 31);
    private static readonly Color PrimaryColor = new Color(216, 208, 187);
    private static readonly Color SecondaryColor = new Color(181, 173, 161);
    private readonly Vector2 _center;
    private string _activeSessionCode;
    private string _errorMessage;
    private int _selectedOptionIndex;

    public SessionStartScreen(
        SpriteFont titleFont,
        SpriteFont font,
        Texture2D selector,
        Vector2 center)
    {
        _titleFont = titleFont;
        _font = font;
        _selector = selector;
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

        bool clicked =
            UiPointer.UpdateOption(GetOptionBounds(ContinueOptionY), 0, ref _selectedOptionIndex) ||
            UiPointer.UpdateOption(GetOptionBounds(NewSessionOptionY), 1, ref _selectedOptionIndex);

        if (MenuInput.Up(keyboardState, previousKeyboardState) ||
            MenuInput.Down(keyboardState, previousKeyboardState))
        {
            _selectedOptionIndex = 1 - _selectedOptionIndex;
        }

        if (!clicked && !MenuInput.Confirm(keyboardState, previousKeyboardState))
        {
            return SessionStartAction.NONE;
        }

        return _selectedOptionIndex == 0
            ? SessionStartAction.CONTINUE
            : SessionStartAction.NEW_SESSION;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.GraphicsDevice.Clear(BackgroundColor);
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        DrawCenteredText(TitleText, _center.Y - 270.0f, PrimaryColor, spriteBatch, _titleFont);
        DrawCenteredText(InstructionText, _center.Y - 175.0f, SecondaryColor, spriteBatch);

        if (!string.IsNullOrEmpty(_activeSessionCode))
        {
            DrawCenteredText(
                $"Sessão: {_activeSessionCode}",
                _center.Y - 115.0f,
                SecondaryColor,
                spriteBatch);
            DrawOption("Continuar", 0, _center.Y + ContinueOptionY, spriteBatch);
            DrawOption("Nova sessão", 1, _center.Y + NewSessionOptionY, spriteBatch);
            DrawCenteredText(
                ReplacementWarningText,
                _center.Y + 235.0f,
                SecondaryColor,
                spriteBatch);
        }

        if (!string.IsNullOrEmpty(_errorMessage))
        {
            DrawCenteredText(_errorMessage, _center.Y + 290.0f, Color.OrangeRed, spriteBatch);
        }

        DrawCenteredText(NavigationText, _center.Y + 360.0f, SecondaryColor, spriteBatch, scale: 0.75f);
        spriteBatch.End();
    }

    private Rectangle GetOptionBounds(float offsetY) =>
        new Rectangle((int)_center.X - OptionHitWidth / 2,
            (int)(_center.Y + offsetY) - OptionHitHeight / 2, OptionHitWidth, OptionHitHeight);

    private void DrawOption(
        string text,
        int optionIndex,
        float y,
        SpriteBatch spriteBatch)
    {
        bool isSelected = optionIndex == _selectedOptionIndex;
        if (isSelected)
        {
            const int selectorWidth = 480;
            int selectorHeight = (int)(selectorWidth * _selector.Height / (float)_selector.Width);
            spriteBatch.Draw(
                _selector,
                new Rectangle(
                    (int)(_center.X - selectorWidth / 2.0f),
                    (int)(y - selectorHeight / 2.0f),
                    selectorWidth,
                    selectorHeight),
                Color.White);
        }

        DrawCenteredText(
            text,
            y,
            isSelected ? PrimaryColor : SecondaryColor,
            spriteBatch);
    }

    private void DrawCenteredText(
        string text,
        float y,
        Color color,
        SpriteBatch spriteBatch,
        SpriteFont font = null,
        float scale = 1.0f)
    {
        font ??= _font;
        Vector2 textSize = font.MeasureString(text) * scale;
        spriteBatch.DrawString(
            font,
            text,
            new Vector2(_center.X - textSize.X / 2.0f, y - textSize.Y / 2.0f),
            color,
            0.0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0.0f);
    }

    private static bool KeyWasPressed(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState,
        Keys key)
    {
        return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }
}
