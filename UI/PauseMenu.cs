using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public enum PauseOption
{
    NONE = 0,
    RESUME = 1,
    RESTART = 2,
    QUIT = 3
}

public class PauseMenu
{
    private const float OptionSpacing = 130.0f;

    private readonly SpriteFont _font;
    private readonly SpriteFont _titleFont;
    private readonly Texture2D _selector;
    private readonly Vector2 _center;
    private readonly Vector2 _resumePosition;
    private readonly Vector2 _restartPosition;
    private readonly Vector2 _quitPosition;

    private PauseOption _currentOption = PauseOption.RESUME;

    public PauseMenu(SpriteFont titleFont, SpriteFont font, Texture2D selector, Vector2 menuCenter)
    {
        _titleFont = titleFont;
        _selector = selector;
        _center = menuCenter;
        _font = font;
        _resumePosition = menuCenter - new Vector2(0, OptionSpacing);
        _restartPosition = menuCenter;
        _quitPosition = menuCenter + new Vector2(0, OptionSpacing);
    }

    public void Open()
    {
        _currentOption = PauseOption.RESUME;
    }

    public PauseOption Update(KeyboardState keyboardState, KeyboardState previousKeyboardState)
    {
        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.W))
        {
            MoveOptionUp();
        }
        else if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.S))
        {
            MoveOptionDown();
        }

        if (KeyWasPressed(keyboardState, previousKeyboardState, Keys.Enter))
        {
            return _currentOption;
        }

        return PauseOption.NONE;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        Vector2 titlePosition = _center - new Vector2(0, 300);
        spriteBatch.DrawString(_titleFont, "Pausa",
            titlePosition - _titleFont.MeasureString("Pausa") / 2,
            new Color(216, 208, 187));
        DrawOption(spriteBatch, "Resume", _resumePosition, _currentOption == PauseOption.RESUME);
        DrawOption(spriteBatch, "Restart", _restartPosition, _currentOption == PauseOption.RESTART);
        DrawOption(spriteBatch, "Quit", _quitPosition, _currentOption == PauseOption.QUIT);
        spriteBatch.End();
    }

    private bool KeyWasPressed(KeyboardState keyboardState, KeyboardState previousKeyboardState, Keys key)
    {
        return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
    }

    private void MoveOptionUp()
    {
        if (_currentOption == PauseOption.RESUME)
        {
            _currentOption = PauseOption.QUIT;
        }
        else if (_currentOption == PauseOption.RESTART)
        {
            _currentOption = PauseOption.RESUME;
        }
        else if (_currentOption == PauseOption.QUIT)
        {
            _currentOption = PauseOption.RESTART;
        }
    }

    private void MoveOptionDown()
    {
        if (_currentOption == PauseOption.RESUME)
        {
            _currentOption = PauseOption.RESTART;
        }
        else if (_currentOption == PauseOption.RESTART)
        {
            _currentOption = PauseOption.QUIT;
        }
        else if (_currentOption == PauseOption.QUIT)
        {
            _currentOption = PauseOption.RESUME;
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
