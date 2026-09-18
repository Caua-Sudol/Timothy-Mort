using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public enum DeathOption
{
    NONE = 0,
    RETRY = 1,
    QUIT = 2
}

public class DeathMenu
{
    private const float OptionSpacing = 130.0f;

    private readonly SpriteFont _font;
    private readonly SpriteFont _titleFont;
    private readonly Texture2D _selector;
    private readonly Vector2 _center;
    private readonly Vector2 _retryPosition;
    private readonly Vector2 _quitPosition;

    private DeathOption _currentOption = DeathOption.RETRY;

    public DeathMenu(SpriteFont titleFont, SpriteFont font, Texture2D selector, Vector2 menuCenter)
    {
        _titleFont = titleFont;
        _selector = selector;
        _center = menuCenter;
        _font = font;
        _retryPosition = menuCenter - new Vector2(0, OptionSpacing / 2);
        _quitPosition = menuCenter + new Vector2(0, OptionSpacing / 2);
    }

    public void Open()
    {
        _currentOption = DeathOption.RETRY;
    }

    public DeathOption Update(KeyboardState keyboardState, KeyboardState previousKeyboardState)
    {
        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.W) ||
            KeyWasPressed(keyboardState, previousKeyboardState, Keys.S))
        {
            ToggleOption();
        }

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.Enter))
        {
            return _currentOption;
        }

        return DeathOption.NONE;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        Vector2 titlePosition = _center - new Vector2(0, 300);
        spriteBatch.DrawString(_titleFont, "Você morreu",
            titlePosition - _titleFont.MeasureString("Você morreu") / 2,
            new Color(216, 208, 187));
        DrawOption(spriteBatch, "Retry", _retryPosition, _currentOption == DeathOption.RETRY);
        DrawOption(spriteBatch, "Quit", _quitPosition, _currentOption == DeathOption.QUIT);
        spriteBatch.End();
    }

    private bool KeyWasPressed(KeyboardState keyboardState, KeyboardState previousKeyboardState, Keys key)
    {
        return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }

    private void ToggleOption()
    {
        if (_currentOption == DeathOption.RETRY)
        {
            _currentOption = DeathOption.QUIT;
        }
        else
        {
            _currentOption = DeathOption.RETRY;
        }
    }

    private void DrawOption(SpriteBatch spriteBatch, string text, Vector2 position, bool selected)
    {
        if (selected)
        {
            const int width = 480;
            int height = (int)(width * _selector.Height / (float)_selector.Width);
            spriteBatch.Draw(_selector,
                new Rectangle((int)(position.X - width / 2.0f), (int)(position.Y - height / 2.0f), width, height),
                Color.White);
        }
        Color color = selected ? new Color(216, 208, 187) : new Color(181, 173, 161);
        Vector2 textSize = _font.MeasureString(text);
        Vector2 textPosition = position - textSize / 2;

        spriteBatch.DrawString(_font, text, textPosition, color);
    }
}
