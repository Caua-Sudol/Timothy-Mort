using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class PhaseIntroductionScreen
{
    private const int LeafCount = 32;
    private const float TitleScale = 1.5f;
    private const float BodyScale = 1.25f;
    private const string NavigationText = "Enter para iniciar a fase    Esc para voltar";

    private readonly SpriteFont _font;
    private readonly Vector2 _dimensions;
    private readonly Random _random = new Random();
    private readonly Leaf[] _leaves = new Leaf[LeafCount];
    private string _title = string.Empty;
    private string _body = string.Empty;

    private struct Leaf
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Size;
        public float Rotation;
        public float RotationSpeed;
    }

    public PhaseIntroductionScreen(SpriteFont font, Vector2 dimensions)
    {
        _font = font;
        _dimensions = dimensions;
    }

    public void Open(string title, string text)
    {
        _title = title;
        _body = WrapText(text, _dimensions.X * 0.64f / BodyScale);

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
        spriteBatch.Begin();
        spriteBatch.Draw(pixel, new Rectangle(0, 0, (int)_dimensions.X, (int)_dimensions.Y), Color.Black);

        foreach (Leaf leaf in _leaves)
        {
            spriteBatch.Draw(
                pixel, leaf.Position, null, Color.ForestGreen * 0.5f,
                leaf.Rotation, new Vector2(0.5f), leaf.Size, SpriteEffects.None, 0.0f);
        }

        DrawCenteredText(spriteBatch, _title, _dimensions.Y * 0.25f, Color.White, TitleScale);

        Vector2 bodyPosition = new Vector2(_dimensions.X * 0.18f, _dimensions.Y * 0.37f);
        spriteBatch.DrawString(
            _font, _body, bodyPosition, Color.WhiteSmoke,
            0.0f, Vector2.Zero, BodyScale, SpriteEffects.None, 0.0f);

        DrawCenteredText(spriteBatch, NavigationText, _dimensions.Y * 0.86f, Color.LightGray, 1.0f);
        spriteBatch.End();
    }

    private void DrawCenteredText(SpriteBatch spriteBatch, string text, float y, Color color, float scale)
    {
        Vector2 textSize = _font.MeasureString(text);
        float fittedScale = Math.Min(scale, _dimensions.X * 0.8f / Math.Max(1.0f, textSize.X));
        Vector2 position = new Vector2((_dimensions.X - textSize.X * fittedScale) / 2.0f, y);
        spriteBatch.DrawString(
            _font, text, position, color, 0.0f, Vector2.Zero, fittedScale, SpriteEffects.None, 0.0f);
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
