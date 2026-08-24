using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class DamageHud
{
    private const int Margin = 32;
    private const int CellSize = 6;
    private const int HeartSpacing = 12;

    private static readonly string[] HeartShape =
    {
        "0110110",
        "1111111",
        "1111111",
        "0111110",
        "0011100",
        "0001000"
    };

    private Texture2D _pixel;

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(SpriteBatch spriteBatch, int currentHealth, int maximumHealth)
    {
        if (_pixel == null || maximumHealth <= 0)
        {
            return;
        }

        int heartWidth = HeartShape[0].Length * CellSize;

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        for (int heartIndex = 0; heartIndex < maximumHealth; heartIndex++)
        {
            Color color = heartIndex < currentHealth
                ? Color.Crimson
                : new Color(70, 55, 65);
            int left = Margin + heartIndex * (heartWidth + HeartSpacing);
            DrawHeart(spriteBatch, left, Margin, color);
        }

        spriteBatch.End();
    }

    private void DrawHeart(
        SpriteBatch spriteBatch,
        int left,
        int top,
        Color color)
    {
        for (int row = 0; row < HeartShape.Length; row++)
        {
            for (int column = 0; column < HeartShape[row].Length; column++)
            {
                if (HeartShape[row][column] != '1')
                {
                    continue;
                }

                spriteBatch.Draw(
                    _pixel,
                    new Rectangle(
                        left + column * CellSize,
                        top + row * CellSize,
                        CellSize,
                        CellSize),
                    color);
            }
        }
    }
}
