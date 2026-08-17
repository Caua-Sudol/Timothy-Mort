using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public class DebugView
{
    private const int MarkerSize = 8;
    private const float TileSize = 32.0f;
    private const int HudMargin = 16;
    private const int HudPadding = 12;

    private readonly SpriteFont _font;
    private Texture2D _pixel;

    public bool IsEnabled { get; private set; }

    public DebugView(SpriteFont font)
    {
        _font = font;
    }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Toggle()
    {
        IsEnabled = !IsEnabled;
    }

public void DrawHud(SpriteBatch spriteBatch, string scenarioId, Player player)
{
        if (!IsEnabled)
        {
            return;
        }

        Vector2 positionInTiles = player.Position / TileSize;
        string text =
            $"SCENARIO: {scenarioId}\n" +
            $"POS PX: X={player.Position.X:F1} Y={player.Position.Y:F1}\n" +
            $"POS TILE: X={positionInTiles.X:F2} Y={positionInTiles.Y:F2}\n" +
            $"VELOCITY PX/S: X={player.Velocity.X:F1} Y={player.Velocity.Y:F1}\n" +
            $"STATE: {player.State.ToString().ToUpperInvariant()}\n" +
            $"GROUNDED: {(player.IsGrounded ? "YES" : "NO")}\n" +
            $"DASH: {(player.CanDash ? "AVAILABLE" : "SPENT")}";

        Vector2 textSize = _font.MeasureString(text);
        var background = new Rectangle(
            HudMargin,
            HudMargin,
            (int)textSize.X + HudPadding * 2 + 1,
            (int)textSize.Y + HudPadding * 2 + 1);
        var textPosition = new Vector2(HudMargin + HudPadding, HudMargin + HudPadding);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(_pixel, background, Color.Black * 0.75f);
    spriteBatch.DrawString(_font, text, textPosition, Color.White);
    spriteBatch.End();
}

public void DrawReloadError(SpriteBatch spriteBatch, string errorMessage)
{
    if (string.IsNullOrWhiteSpace(errorMessage))
    {
        return;
    }

    string text =
        "TMX RELOAD FAILED\n" +
        $"{errorMessage}\n" +
        "Last valid map kept. Fix the file and press Ctrl+R.";
    Vector2 textSize = _font.MeasureString(text);
    int backgroundHeight = (int)textSize.Y + HudPadding * 2 + 1;
    var background = new Rectangle(
        HudMargin,
        spriteBatch.GraphicsDevice.Viewport.Height - backgroundHeight - HudMargin,
        (int)textSize.X + HudPadding * 2 + 1,
        backgroundHeight);
    var textPosition = new Vector2(
        background.X + HudPadding,
        background.Y + HudPadding);

    spriteBatch.Begin(samplerState: SamplerState.PointClamp);
    spriteBatch.Draw(_pixel, background, Color.DarkRed * 0.9f);
    spriteBatch.DrawString(_font, text, textPosition, Color.White);
    spriteBatch.End();
}

    public void DrawWorld(
        SpriteBatch spriteBatch,
        Camera camera,
        Player player,
        IEnumerable<Rectangle> platforms,
        IEnumerable<Rectangle> gelatins,
        IEnumerable<Rectangle> verticalRoutePlatforms,
        IEnumerable<Rectangle> verticalRouteGelatins,
        bool verticalRouteRevealed,
        IEnumerable<Rectangle> triggers,
        IEnumerable<SceneExit> sceneExits,
        IEnumerable<InterventionEntrance> interventionEntrances,
        IEnumerable<Rectangle> interventionButtons,
        Rectangle? exitDoor,
        DebugMeasurementPair measurementPair,
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

        foreach (Rectangle platform in verticalRoutePlatforms)
        {
            DrawOutline(
                spriteBatch,
                platform,
                verticalRouteRevealed ? Color.LimeGreen : Color.Gray);
        }

        foreach (Rectangle gelatin in verticalRouteGelatins)
        {
            DrawOutline(
                spriteBatch,
                gelatin,
                verticalRouteRevealed ? Color.DeepSkyBlue : Color.Gray);
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

        if (exitDoor.HasValue)
        {
            DrawOutline(spriteBatch, exitDoor.Value, Color.Red);
        }

        DrawMeasurementPair(spriteBatch, measurementPair);

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

    private void DrawMeasurementPair(
        SpriteBatch spriteBatch,
        DebugMeasurementPair measurementPair)
    {
        if (measurementPair == null)
        {
            return;
        }

        Vector2 deltaInPixels = measurementPair.Destination - measurementPair.Origin;
        Vector2 deltaInTiles = deltaInPixels / TileSize;
        Vector2 midpoint = (measurementPair.Origin + measurementPair.Destination) / 2.0f;
        string details =
            $"PAIR: {measurementPair.PairId} | ACTION: {measurementPair.Action.ToUpperInvariant()}\n" +
            $"DELTA PX: X={deltaInPixels.X:+0.0;-0.0;0.0} Y={deltaInPixels.Y:+0.0;-0.0;0.0}\n" +
            $"DELTA TILE: X={deltaInTiles.X:+0.00;-0.00;0.00} Y={deltaInTiles.Y:+0.00;-0.00;0.00}";
        Vector2 detailsSize = _font.MeasureString(details);
        Vector2 detailsPosition = new Vector2(
            midpoint.X - detailsSize.X / 2.0f,
            midpoint.Y - detailsSize.Y - 12.0f);

        DrawLine(
            spriteBatch,
            measurementPair.Origin,
            measurementPair.Destination,
            Color.White,
            2.0f);
        DrawCross(spriteBatch, measurementPair.Origin, Color.LimeGreen);
        DrawCross(spriteBatch, measurementPair.Destination, Color.OrangeRed);
        DrawWorldText(
            spriteBatch,
            "ORIGEM",
            measurementPair.Origin + new Vector2(10.0f, 8.0f),
            Color.LimeGreen);
        DrawWorldText(
            spriteBatch,
            "DESTINO",
            measurementPair.Destination + new Vector2(10.0f, 8.0f),
            Color.OrangeRed);
        DrawWorldText(spriteBatch, details, detailsPosition, Color.White);
    }

    private void DrawLine(
        SpriteBatch spriteBatch,
        Vector2 origin,
        Vector2 destination,
        Color color,
        float thickness)
    {
        Vector2 delta = destination - origin;
        float length = delta.Length();
        float rotation = (float)System.Math.Atan2(delta.Y, delta.X);

        spriteBatch.Draw(
            _pixel,
            origin,
            null,
            color,
            rotation,
            Vector2.Zero,
            new Vector2(length, thickness),
            SpriteEffects.None,
            0.0f);
    }

    private void DrawWorldText(
        SpriteBatch spriteBatch,
        string text,
        Vector2 position,
        Color color)
    {
        spriteBatch.DrawString(_font, text, position + Vector2.One, Color.Black);
        spriteBatch.DrawString(_font, text, position, color);
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
