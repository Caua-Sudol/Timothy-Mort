using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class PhaseIntroductionScreen
{
    private const int LeafCount = 32;
    private const float TitleScale = 1.0f;
    private const float BodyScale = 1.0f;
    private const string DefaultNavigationText = "Enter para iniciar a fase    Esc para voltar";

    private readonly SpriteFont _font;
    private readonly SpriteFont _titleFont;
    private readonly Vector2 _dimensions;
    private readonly Random _random = new Random();
    private readonly Leaf[] _leaves = new Leaf[LeafCount];
    private string _title = string.Empty;
    private string _body = string.Empty;
    private string _navigationText = DefaultNavigationText;
    private float _bodyScale = BodyScale;

    private struct Leaf
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public float Rotation;
        public float RotationSpeed;
    }

    public PhaseIntroductionScreen(SpriteFont titleFont, SpriteFont font, Vector2 dimensions)
    {
        _titleFont = titleFont;
        _font = font;
        _dimensions = dimensions;
    }

    public void Open(string title, string text, string navigationText = DefaultNavigationText)
    {
        _title = title;
        _navigationText = navigationText;
        _body = WrapText(text.Replace("\r\n", "\n"), _dimensions.X * 0.64f / BodyScale);
        Vector2 bodySize = _font.MeasureString(_body);
        _bodyScale = Math.Min(BodyScale, Math.Min(
            _dimensions.Y * 0.43f / Math.Max(1.0f, bodySize.Y),
            _dimensions.X * 0.64f / Math.Max(1.0f, bodySize.X)));

        for (int index = 0; index < _leaves.Length; index++)
        {
            _leaves[index] = new Leaf
            {
                Position = new Vector2(NextFloat(0.0f, _dimensions.X), NextFloat(0.0f, _dimensions.Y)),
                Velocity = new Vector2(NextFloat(12.0f, 35.0f), NextFloat(20.0f, 55.0f)),
                Size = NextFloat(6.0f, 14.0f),
                Rotation = NextFloat(0.0f, MathHelper.TwoPi),
                RotationSpeed = NextFloat(-0.7f, 0.7f)
            };
        }
    }

    public void Update(double deltaTime)
    {
        float elapsed = (float)deltaTime;
        for (int index = 0; index < _leaves.Length; index++)
        {
            ref Leaf leaf = ref _leaves[index];
            leaf.Position += leaf.Velocity * elapsed;
            leaf.Rotation += leaf.RotationSpeed * elapsed;

            if (leaf.Position.Y > _dimensions.Y + leaf.Size)
            {
                leaf.Position = new Vector2(NextFloat(0.0f, _dimensions.X), -leaf.Size);
            }
            else if (leaf.Position.X > _dimensions.X + leaf.Size)
            {
                leaf.Position.X = -leaf.Size;
            }
        }
    }

    // The shared pixel belongs to Game1; this screen does not allocate a texture.
    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        DrawBackground(spriteBatch, pixel);

        DrawCenteredText(spriteBatch, _title, _dimensions.Y * 0.25f, new Color(216, 208, 187), TitleScale, _titleFont);

        Vector2 bodyPosition = new Vector2(_dimensions.X * 0.18f, _dimensions.Y * 0.37f);
        spriteBatch.DrawString(
            _font, _body, bodyPosition, new Color(216, 208, 187),
            0.0f, Vector2.Zero, _bodyScale, SpriteEffects.None, 0.0f);

        DrawCenteredText(spriteBatch, _navigationText, _dimensions.Y * 0.86f, new Color(181, 173, 161), 0.75f, _font);
        spriteBatch.End();
    }

    // The caller owns the SpriteBatch Begin/End pair.
    public void DrawBackground(SpriteBatch spriteBatch, Texture2D pixel)
    {
        spriteBatch.Draw(pixel, new Rectangle(0, 0, (int)_dimensions.X, (int)_dimensions.Y), new Color(21, 25, 31));

        foreach (Leaf leaf in _leaves)
        {
            spriteBatch.Draw(
                pixel, leaf.Position, null, Color.ForestGreen * 0.5f,
                leaf.Rotation, new Vector2(0.5f), leaf.Size, SpriteEffects.None, 0.0f);
        }

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

    private float NextFloat(float minimum, float maximum)
    {
        return minimum + (float)_random.NextDouble() * (maximum - minimum);
    }
}
