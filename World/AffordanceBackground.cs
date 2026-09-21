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
        _background = content.Load<Texture2D>("visual/affordance_visibility_bg");
        _midground = content.Load<Texture2D>("visual/affordance_visibility_mg");
        _foreground = content.Load<Texture2D>("visual/affordance_visibility_fg");
        _lights = content.Load<Texture2D>("visual/affordance_visibility_lgt");
    }

    public static float GetRoomOffset(string scenarioId)
    {
        return scenarioId == ScenarioCatalog.AffordanceRoom3ScenarioId ? ViewWidth * 2 :
            scenarioId == ScenarioCatalog.AffordanceRoom2ScenarioId ? ViewWidth : 0;
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
