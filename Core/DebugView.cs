using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public class DebugView
{
    private const int MarkerSize = 8;

    private Texture2D _pixel;

    public bool IsEnabled { get; private set; }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Toggle()
    {
        IsEnabled = !IsEnabled;
    }

    public void DrawWorld(
        SpriteBatch spriteBatch,
        Camera camera,
        Player player,
        IEnumerable<Rectangle> platforms,
        IEnumerable<Rectangle> gelatins,
        IEnumerable<Rectangle> triggers,
        IEnumerable<SceneExit> sceneExits,
        IEnumerable<InterventionEntrance> interventionEntrances,
        IEnumerable<Rectangle> interventionButtons,
        IEnumerable<Rectangle> exitDoors,
        InterventionEntrancePlacement? activeInterventionEntrancePlacement,
        Vector2 checkpoint)
    {
        if (!IsEnabled)
        {
            return;
        }

        spriteBatch.Begin(transformMatrix: camera.GetTransform());

        DrawOutline(spriteBatch, player.HitBox, GetPlayerColor(player));

        foreach (Rectangle platform in platforms)
        {
            DrawOutline(spriteBatch, platform, Color.LimeGreen);
        }

        foreach (Rectangle gelatin in gelatins)
        {
            DrawOutline(spriteBatch, gelatin, Color.DeepSkyBlue);
        }

        foreach (Rectangle trigger in triggers)
        {
            spriteBatch.Draw(_pixel, trigger, Color.Red * 0.25f);
            DrawOutline(spriteBatch, trigger, Color.Red);
        }

        foreach (SceneExit sceneExit in sceneExits)
        {
            spriteBatch.Draw(_pixel, sceneExit.Bounds, Color.Gold * 0.25f);
            DrawOutline(spriteBatch, sceneExit.Bounds, Color.Gold);
        }

        foreach (InterventionEntrance entrance in interventionEntrances)
        {
            Color color = activeInterventionEntrancePlacement.HasValue &&
                entrance.Placement == activeInterventionEntrancePlacement.Value
                    ? Color.Orange
                    : Color.Gray;

            DrawOutline(spriteBatch, entrance.Bounds, color);
        }

        foreach (Rectangle button in interventionButtons)
        {
            DrawOutline(spriteBatch, button, Color.Orange);
        }

        foreach (Rectangle door in exitDoors)
        {
            DrawOutline(spriteBatch, door, Color.Red);
        }

        DrawCross(spriteBatch, checkpoint, Color.Magenta);
        DrawCross(spriteBatch, camera.Position, Color.Cyan);

        spriteBatch.End();
    }

    private Color GetPlayerColor(Player player)
    {
        if (player.State == PlayerState.Dashing)
        {
            return Color.Orange;
        }

        return player.IsGrounded ? Color.LimeGreen : Color.Yellow;
    }

    private void DrawOutline(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, 1), color);
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Bottom - 1, rectangle.Width, 1), color);
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Left, rectangle.Top, 1, rectangle.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rectangle.Right - 1, rectangle.Top, 1, rectangle.Height), color);
    }

    private void DrawCross(SpriteBatch spriteBatch, Vector2 position, Color color)
    {
        int x = (int)position.X;
        int y = (int)position.Y;

        spriteBatch.Draw(_pixel, new Rectangle(x - MarkerSize, y, MarkerSize * 2 + 1, 1), color);
        spriteBatch.Draw(_pixel, new Rectangle(x, y - MarkerSize, 1, MarkerSize * 2 + 1), color);
    }
}
