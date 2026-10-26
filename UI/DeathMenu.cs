using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public enum DeathOption
{
    NONE = 0,
    RETRY = 1,
    QUIT = 2,
    MAIN_MENU = 3
}

public class DeathMenu
{
    private const float OptionSpacing = 115.0f;
    private const float OptionHitWidth = 480.0f;

    private static readonly (DeathOption Option, string Text)[] Options =
    {
        (DeathOption.RETRY, "Tentar de novo"),
        (DeathOption.MAIN_MENU, "Menu principal"),
        (DeathOption.QUIT, "Sair do jogo")
    };

    private readonly SpriteFont _font;
    private readonly SpriteFont _titleFont;
    private readonly Texture2D _selector;
    private readonly Vector2 _center;

    private int _selectedIndex;

    public DeathMenu(SpriteFont titleFont, SpriteFont font, Texture2D selector, Vector2 menuCenter)
    {
        _titleFont = titleFont;
        _selector = selector;
        _center = menuCenter;
        _font = font;
    }

    public void Open()
    {
        _selectedIndex = 0;
    }

    public DeathOption Update(KeyboardState keyboardState, KeyboardState previousKeyboardState)
    {
        for (int index = 0; index < Options.Length; index++)
        {
            if (UiPointer.UpdateOption(GetOptionBounds(index), index, ref _selectedIndex))
                return Options[index].Option;
        }

        if (MenuInput.Up(keyboardState, previousKeyboardState))
            _selectedIndex = (_selectedIndex - 1 + Options.Length) % Options.Length;
        else if (MenuInput.Down(keyboardState, previousKeyboardState))
            _selectedIndex = (_selectedIndex + 1) % Options.Length;

        if (MenuInput.Confirm(keyboardState, previousKeyboardState))
            return Options[_selectedIndex].Option;

        return DeathOption.NONE;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        Vector2 titlePosition = _center - new Vector2(0, 300);
        spriteBatch.DrawString(_titleFont, "Você morreu",
            titlePosition - _titleFont.MeasureString("Você morreu") / 2,
            new Color(216, 208, 187));
        for (int index = 0; index < Options.Length; index++)
            DrawOption(spriteBatch, Options[index].Text, GetOptionPosition(index), index == _selectedIndex);
        spriteBatch.End();
    }

    private Vector2 GetOptionPosition(int index) =>
        _center + new Vector2(0, (index - (Options.Length - 1) / 2.0f) * OptionSpacing);

    private Rectangle GetOptionBounds(int index)
    {
        Vector2 position = GetOptionPosition(index);
        return new Rectangle((int)(position.X - OptionHitWidth / 2.0f),
            (int)(position.Y - OptionSpacing / 2.0f), (int)OptionHitWidth, (int)OptionSpacing);
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
