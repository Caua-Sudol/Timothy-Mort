using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public sealed class TitleScreen
{
    private const string GameTitle = "TIMOTHY & MORT";
    private const string ContinueText = "Pressione qualquer botão";
    private const float TextSpacing = 80.0f;

    private readonly SpriteFont _font;
    private readonly Vector2 _center;

    public TitleScreen(SpriteFont font, Vector2 center)
    {
        _font = font;
        _center = center;
    }

    public bool Update(KeyboardState keyboardState, KeyboardState previousKeyboardState)
    {
        return keyboardState.GetPressedKeyCount() > 0 &&
               previousKeyboardState.GetPressedKeyCount() == 0;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin();
        DrawCenteredText(
            spriteBatch,
            GameTitle,
            _center - new Vector2(0.0f, TextSpacing / 2.0f),
            Color.White);
        DrawCenteredText(
            spriteBatch,
            ContinueText,
            _center + new Vector2(0.0f, TextSpacing / 2.0f),
            Color.LightGray);
        spriteBatch.End();
    }

    private void DrawCenteredText(
        SpriteBatch spriteBatch,
        string text,
        Vector2 position,
        Color color)
    {
        Vector2 textSize = _font.MeasureString(text);
        spriteBatch.DrawString(_font, text, position - textSize / 2.0f, color);
    }
}
