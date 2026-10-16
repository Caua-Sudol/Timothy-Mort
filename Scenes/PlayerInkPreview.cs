using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

// Owned only by ScaleTestRoom; ContentManager owns the loaded effect.
internal sealed class PlayerInkPreview
{
    private static readonly float[] OutlineWidths = { 1.5f, 2.5f, 3.5f };
    private static readonly float[] HatchStrengths = { 0.30f, 0.50f, 0.70f };
    private static readonly Vector3 InkColor = new Color(24, 18, 32).ToVector3();
    private const float HatchSpacing = 7f;
    private readonly Effect _effect;
    private bool _enabled = true;
    private bool _hatching = true;
    private int _preset = 1;

    public PlayerInkPreview(ContentManager content)
    {
        _effect = content.Load<Effect>("Effects/PlayerInkOutline");
        var sprite = content.Load<Texture2D>("player/timothy_mort_2_right");
        float height = GameScaleConfig.PlayerSpriteReferenceHeight;
        _effect.Parameters["HatchReferenceSize"].SetValue(
            new Vector2(height * sprite.Width / sprite.Height, height));
        _effect.Parameters["HatchSpacing"].SetValue(HatchSpacing);
        _effect.Parameters["InkColor"].SetValue(InkColor);
    }

    public void Update(KeyboardState keys, KeyboardState previous)
    {
        bool Pressed(Keys key) => keys.IsKeyDown(key) && previous.IsKeyUp(key);
        if (Pressed(Keys.F6)) _enabled = !_enabled;
        if (Pressed(Keys.F7)) _preset = (_preset + 1) % OutlineWidths.Length;
        if (Pressed(Keys.F8)) _hatching = !_hatching;
    }

    public Effect Prepare()
    {
        if (!_enabled) return null;
        _effect.Parameters["OutlineWidth"].SetValue(OutlineWidths[_preset]);
        _effect.Parameters["HatchStrength"].SetValue(_hatching ? HatchStrengths[_preset] : 0f);
        return _effect;
    }

    public string Status => $"INK: {(_enabled ? "ON" : "OFF")} | F6: comparar | " +
        $"F7: intensidade {_preset + 1}/3 | F8: hachuras {(_hatching ? "ON" : "OFF")}";
}
