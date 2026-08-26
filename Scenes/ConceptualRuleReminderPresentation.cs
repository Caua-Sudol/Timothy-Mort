using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

internal sealed class ConceptualRuleReminderPresentation
{
    private const float TextScale = 0.7f;
    private const int PanelPadding = 6;
    private const int IconSize = 16;
    private const int IconTextSpacing = 6;
    private const int SurfaceSpacing = 6;

    private SpriteFont _font;
    private Texture2D _pixel;

    public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
    {
        _font = content.Load<SpriteFont>("font");
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Draw(
        SpriteBatch spriteBatch,
        LevelMap map,
        bool verticalRouteRevealed)
    {
        foreach (Rectangle gelatin in map.Gelatins)
        {
            DrawReminder(spriteBatch, gelatin, "DASH", Color.Cyan, false);
        }

        foreach (DamageVolume damageVolume in map.DamageVolumes)
        {
            DrawReminder(spriteBatch, damageVolume.Bounds, "DANO", Color.Gold, true);
        }

        foreach (DamageVolume damageVolume in map.HarmlessSpikeVolumes)
        {
            DrawReminder(spriteBatch, damageVolume.Bounds, "DANO", Color.Gold, true);
        }

        if (!verticalRouteRevealed)
        {
            return;
        }

        foreach (Rectangle gelatin in map.VerticalRouteGelatins)
        {
            DrawReminder(spriteBatch, gelatin, "DASH", Color.Cyan, false);
        }

        foreach (DamageVolume damageVolume in map.VerticalRouteDamageVolumes)
        {
            DrawReminder(spriteBatch, damageVolume.Bounds, "DANO", Color.Gold, true);
        }
    }

    private void DrawReminder(
        SpriteBatch spriteBatch,
        Rectangle surface,
        string label,
        Color accentColor,
        bool usesDamageIcon)
    {
        Vector2 textSize = _font.MeasureString(label) * TextScale;
        int panelWidth = (int)Math.Ceiling(
            PanelPadding * 2 + IconSize + IconTextSpacing + textSize.X);
        int panelHeight = Math.Max(
            IconSize + PanelPadding * 2,
            (int)Math.Ceiling(textSize.Y) + PanelPadding * 2);
        var panel = new Rectangle(
            surface.Center.X - panelWidth / 2,
            surface.Top - panelHeight - SurfaceSpacing,
            panelWidth,
            panelHeight);

        spriteBatch.Draw(_pixel, panel, Color.Black * 0.78f);
        DrawBorder(spriteBatch, panel, accentColor);

        var iconBounds = new Rectangle(
            panel.Left + PanelPadding,
            panel.Center.Y - IconSize / 2,
            IconSize,
            IconSize);

        if (usesDamageIcon)
        {
            DrawDamageIcon(spriteBatch, iconBounds, accentColor);
        }
        else
        {
            DrawDashIcon(spriteBatch, iconBounds, accentColor);
        }

        spriteBatch.DrawString(
            _font,
            label,
            new Vector2(
                iconBounds.Right + IconTextSpacing,
                panel.Center.Y - textSize.Y / 2.0f),
            Color.White,
            0.0f,
            Vector2.Zero,
            TextScale,
            SpriteEffects.None,
            0.0f);
    }

    private void DrawDamageIcon(
        SpriteBatch spriteBatch,
        Rectangle bounds,
        Color color)
    {
        spriteBatch.Draw(
            _pixel,
            new Rectangle(bounds.Center.X - 2, bounds.Top + 2, 4, 8),
            color);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(bounds.Center.X - 2, bounds.Bottom - 4, 4, 4),
            color);
    }

    private void DrawDashIcon(
        SpriteBatch spriteBatch,
        Rectangle bounds,
        Color color)
    {
        spriteBatch.Draw(
            _pixel,
            new Rectangle(bounds.Left, bounds.Center.Y - 2, 11, 4),
            color);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(bounds.Left + 8, bounds.Top + 2, 4, 4),
            color);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(bounds.Left + 12, bounds.Top + 6, 4, 4),
            color);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(bounds.Left + 8, bounds.Bottom - 6, 4, 4),
            color);
    }

    private void DrawBorder(
        SpriteBatch spriteBatch,
        Rectangle bounds,
        Color color)
    {
        const int thickness = 2;

        spriteBatch.Draw(
            _pixel,
            new Rectangle(bounds.Left, bounds.Top, bounds.Width, thickness),
            color);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(bounds.Left, bounds.Bottom - thickness, bounds.Width, thickness),
            color);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(bounds.Left, bounds.Top, thickness, bounds.Height),
            color);
        spriteBatch.Draw(
            _pixel,
            new Rectangle(bounds.Right - thickness, bounds.Top, thickness, bounds.Height),
            color);
    }
}
