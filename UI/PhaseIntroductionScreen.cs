using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class PhaseIntroductionScreen
{
    private const float TitleScale = 1.0f;
    private const float BodyScale = 1.0f;
    private const double CharactersPerSecond = 60.0;
    private const string DefaultNavigationText = "Enter para iniciar a fase    Esc para voltar";

    private readonly SpriteFont _font;
    private readonly SpriteFont _titleFont;
    private readonly Vector2 _dimensions;
    private readonly SymbolRain _symbols;
    private string _title = string.Empty;
    private string _body = string.Empty;
    private string _navigationText = DefaultNavigationText;
    private float _bodyScale = BodyScale;
    private double _visibleCharacters;
    // Body area as fractions of the screen; the final screen asks for a larger one.
    private float _bodyX = 0.18f;
    private float _bodyWidth = 0.64f;
    private float _bodyHeight = 0.43f;
    // Largest body scale; the final screen lets short blocks grow a little.
    private float _maximumBodyScale = BodyScale;
    // Optional second column (final screen), drawn beside the body; off by default.
    private string _secondBody = string.Empty;
    private float _secondBodyScale = BodyScale;
    private float _secondX;
    private float _secondWidth;

    public PhaseIntroductionScreen(SpriteFont titleFont, SpriteFont font, Vector2 dimensions, Texture2D symbols)
    {
        _titleFont = titleFont;
        _font = font;
        _dimensions = dimensions;
        _symbols = new SymbolRain(symbols, dimensions);
    }

    public void SetBodyArea(float x, float width, float height)
    {
        _bodyX = x;
        _bodyWidth = width;
        _bodyHeight = height;
    }

    public void SetMaximumBodyScale(float scale) => _maximumBodyScale = scale;

    // A second text block to the right of the body (x and width as screen fractions).
    public void SetSecondColumn(float x, float width)
    {
        _secondX = x;
        _secondWidth = width;
    }

    public void Open(string title, string text, string navigationText = DefaultNavigationText,
        string secondText = null)
    {
        _title = title;
        _navigationText = navigationText;
        _body = FitText(text, _bodyWidth, out _bodyScale);
        _secondBody = string.IsNullOrEmpty(secondText) || _secondWidth <= 0
            ? string.Empty
            : FitText(secondText, _secondWidth, out _secondBodyScale);

        _visibleCharacters = 0;
        _symbols.Reset();
    }

    // Wraps the text to the column and returns the scale that makes it fit the body area.
    private string FitText(string text, float widthFraction, out float scale)
    {
        float width = _dimensions.X * widthFraction;
        string wrapped = WrapText(text.Replace("\r\n", "\n"), width / _maximumBodyScale);
        Vector2 size = _font.MeasureString(wrapped);
        scale = Math.Min(_maximumBodyScale, Math.Min(
            _dimensions.Y * _bodyHeight / Math.Max(1.0f, size.Y),
            width / Math.Max(1.0f, size.X)));
        return wrapped;
    }

    public bool IsTyping => _visibleCharacters < _body.Length;

    public void CompleteTyping() => _visibleCharacters = _body.Length;

    public void Update(double deltaTime)
    {
        _symbols.Update(deltaTime);
        if (IsTyping)
            _visibleCharacters += deltaTime * CharactersPerSecond;
    }

    // The shared pixel belongs to Game1; this screen does not allocate a texture.
    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        DrawBackground(spriteBatch, pixel);

        DrawCenteredText(spriteBatch, _title, _dimensions.Y * 0.25f, new Color(216, 208, 187), TitleScale, _titleFont);

        Vector2 bodyPosition = new Vector2(_dimensions.X * _bodyX, _dimensions.Y * 0.37f);
        spriteBatch.DrawString(
            _font, _body.Substring(0, Math.Min(_body.Length, (int)_visibleCharacters)), bodyPosition, new Color(216, 208, 187),
            0.0f, Vector2.Zero, _bodyScale, SpriteEffects.None, 0.0f);

        // The second column appears once the body has finished typing.
        if (_secondBody.Length > 0 && !IsTyping)
            spriteBatch.DrawString(
                _font, _secondBody, new Vector2(_dimensions.X * _secondX, _dimensions.Y * 0.37f),
                new Color(216, 208, 187), 0.0f, Vector2.Zero, _secondBodyScale, SpriteEffects.None, 0.0f);

        DrawCenteredText(spriteBatch, _navigationText, _dimensions.Y * 0.86f, new Color(181, 173, 161), 0.75f, _font);
        spriteBatch.End();
    }

    // The caller owns the SpriteBatch Begin/End pair.
    public void DrawBackground(SpriteBatch spriteBatch, Texture2D pixel)
    {
        spriteBatch.Draw(pixel, new Rectangle(0, 0, (int)_dimensions.X, (int)_dimensions.Y), new Color(21, 25, 31));

        _symbols.Draw(spriteBatch);
    }

    private void DrawCenteredText(SpriteBatch spriteBatch, string text, float y, Color color, float scale, SpriteFont font)
    {
        Vector2 textSize = font.MeasureString(text);
        float fittedScale = Math.Min(scale, _dimensions.X * 0.8f / Math.Max(1.0f, textSize.X));
        Vector2 position = new Vector2((_dimensions.X - textSize.X * fittedScale) / 2.0f, y);
        spriteBatch.DrawString(
            font, text, position, color, 0.0f, Vector2.Zero, fittedScale, SpriteEffects.None, 0.0f);
    }

    private string WrapText(string text, float maximumWidth)
    {
        var result = new StringBuilder();
        foreach (string paragraph in text.Split('\n'))
        {
            string line = string.Empty;
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = line.Length == 0 ? word : line + " " + word;
                if (line.Length > 0 && _font.MeasureString(candidate).X > maximumWidth)
                {
                    result.AppendLine(line);
                    line = word;
                }
                else
                {
                    line = candidate;
                }
            }

            result.AppendLine(line);
        }

        return result.ToString().TrimEnd();
    }

}
