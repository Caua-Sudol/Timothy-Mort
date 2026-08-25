using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class DamageHud
{
    private const int Margin = 32;
    private const int DefaultCellSize = 6;
    private const int DefaultHeartSpacing = 12;
    private int _cellSize = DefaultCellSize;
    private int _heartSpacing = DefaultHeartSpacing;

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

    public void Draw(
        SpriteBatch spriteBatch,
        int currentHealth,
        int maximumHealth,
        SolutionVisualModule visualModules)
    {
        bool emphasizeHud = visualModules.HasFlag(SolutionVisualModule.DAMAGE_HUD_EMPHASIS);
        _cellSize = emphasizeHud ? 8 : DefaultCellSize;
        _heartSpacing = emphasizeHud ? 16 : DefaultHeartSpacing;
        if (_pixel == null || maximumHealth <= 0)
        {
            return;
        }

        int heartWidth = HeartShape[0].Length * _cellSize;

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        for (int heartIndex = 0; heartIndex < maximumHealth; heartIndex++)
        {
            Color color = heartIndex < currentHealth
                ? (emphasizeHud ? Color.Red : Color.Crimson)
                : new Color(70, 55, 65);
            int left = Margin + heartIndex * (heartWidth + _heartSpacing);
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
                        left + column * _cellSize,
                        top + row * _cellSize,
                        _cellSize,
                        _cellSize),
                    color);
            }
        }
    }
}
