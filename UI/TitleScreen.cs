using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public sealed class TitleScreen
{
    private const string ContinueText = "Pressione qualquer botão";
    private static readonly Color ContinueColor = new Color(181, 173, 161);

    private readonly SpriteFont _font;
    private readonly Texture2D _background;
    private readonly Vector2 _center;

    public TitleScreen(SpriteFont font, Texture2D background, Vector2 center)
    {
        _font = font;
        _background = background;
        _center = center;
    }

    public bool Update(KeyboardState keyboardState, KeyboardState previousKeyboardState)
    {
        return keyboardState.GetPressedKeyCount() > 0 &&
               previousKeyboardState.GetPressedKeyCount() == 0;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        spriteBatch.Draw(
            _background,
            new Rectangle(0, 0, (int)(_center.X * 2), (int)(_center.Y * 2)),
            Color.White);
        DrawCenteredText(
            spriteBatch,
            ContinueText,
            new Vector2(_center.X, _center.Y * 1.7f),
            ContinueColor);
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
