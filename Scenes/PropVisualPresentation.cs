using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

internal sealed class PropVisualPresentation
{
    private static readonly Rectangle BackgroundBounds = new Rectangle(0, 0, 1920, 1080);
    private static readonly Rectangle PlayerSourceBounds = new Rectangle(400, 62, 744, 1413);
    private const int PlayerVisualWidth = 67;
    private const int PlayerVisualHeight = 128;

    private Texture2D _backgroundTexture;
    private Texture2D _solidPlatformTexture;
    private Texture2D _gelatinPlatformTexture;
    private Texture2D _foregroundTexture;
    private Texture2D _playerTexture;

    public void LoadContent(ContentManager content)
    {
        _backgroundTexture = content.Load<Texture2D>("visual/bg_prot");
        _solidPlatformTexture = content.Load<Texture2D>("visual/mg_solid");
        _gelatinPlatformTexture = content.Load<Texture2D>("visual/mg_gelatine");
        _foregroundTexture = content.Load<Texture2D>("visual/foreground_prot");
        _playerTexture = content.Load<Texture2D>("visual/timothy_mort");
    }

    public void DrawBackground(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_backgroundTexture, BackgroundBounds, Color.White);
    }

    public void DrawMidground(
        SpriteBatch spriteBatch,
        IEnumerable<Rectangle> platforms,
        IEnumerable<Rectangle> gelatins)
    {
        foreach (Rectangle platform in platforms)
        {
            spriteBatch.Draw(_solidPlatformTexture, platform, Color.White);
        }

        foreach (Rectangle gelatin in gelatins)
        {
            spriteBatch.Draw(_gelatinPlatformTexture, gelatin, Color.White);
        }
    }

    public void DrawPlayer(SpriteBatch spriteBatch, Player player)
    {
        Rectangle hitBox = player.HitBox;
        var playerVisualBounds = new Rectangle(
            hitBox.Center.X - PlayerVisualWidth / 2,
            hitBox.Bottom - PlayerVisualHeight,
            PlayerVisualWidth,
            PlayerVisualHeight);

        spriteBatch.Draw(
            _playerTexture,
            playerVisualBounds,
            PlayerSourceBounds,
            Color.White);
    }

    public void DrawForeground(
        SpriteBatch spriteBatch,
        IEnumerable<Rectangle> foregrounds)
    {
        foreach (Rectangle foreground in foregrounds)
        {
            spriteBatch.Draw(_foregroundTexture, foreground, Color.White);
        }
    }
}
