using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class AffordanceBackground
{
    private const int ViewWidth = 1920;
    private const int ViewHeight = 1080;
    private readonly Texture2D _background;
    private readonly Texture2D _midground;
    private readonly Texture2D _foreground;
    private readonly Texture2D _lights;

    public AffordanceBackground(ContentManager content)
    {
        _background = LoadDiagnostics.LoadTexture(content, "visual/affordance_visibility_bg");
        _midground = LoadDiagnostics.LoadTexture(content, "visual/affordance_visibility_mg");
        _foreground = LoadDiagnostics.LoadTexture(content, "visual/affordance_visibility_fg");
        _lights = LoadDiagnostics.LoadTexture(content, "visual/affordance_visibility_lgt");
    }

    public static float GetRoomOffset(string scenarioId)
    {
        // Each challenge room shows its own third of the panorama, in room order.
        int roomIndex = PhaseCatalog.GetForScenario(scenarioId)?.GetRoomIndex(scenarioId) ?? -1;
        return roomIndex > 0 ? ViewWidth * roomIndex : 0;
    }

    public void DrawBackground(SpriteBatch spriteBatch, float roomOffset)
    {
        spriteBatch.Begin(blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.LinearClamp);
        DrawLayer(spriteBatch, _background, roomOffset);
        DrawLayer(spriteBatch, _midground, roomOffset);
        // All three scenery layers belong behind gameplay.
        DrawLayer(spriteBatch, _foreground, roomOffset);
        spriteBatch.End();
    }

    public void DrawForeground(SpriteBatch spriteBatch, float roomOffset)
    {
        spriteBatch.Begin(blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.LinearClamp);
        DrawLayer(spriteBatch, _lights, roomOffset);
        spriteBatch.End();
    }

    private static void DrawLayer(
        SpriteBatch spriteBatch, Texture2D texture, float roomOffset)
    {
        // Preserve the panorama's aspect ratio; never squeeze all three rooms into one.
        float scale = ViewHeight / (float)texture.Height;
        float width = texture.Width * scale;
        float offset = MathHelper.Clamp(roomOffset, 0, System.Math.Max(0, width - ViewWidth));
        spriteBatch.Draw(texture, new Vector2(-offset, 0), null, Color.White,
            0, Vector2.Zero, scale, SpriteEffects.None, 0);
    }
}
