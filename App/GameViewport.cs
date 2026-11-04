using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public sealed class GameViewport
{
    public const int LogicalWidth = 1920;
    public const int LogicalHeight = 1080;

    private const int WindowMargin = 64;

    private readonly GraphicsDeviceManager _graphics;
    private readonly GameWindow _window;

    private RenderTarget2D _renderTarget;
    public Vector2 LogicalSize => new(LogicalWidth, LogicalHeight);

    public GameViewport(Game game)
    {
        _graphics = new GraphicsDeviceManager(game);
        _window = game.Window;

        Point initialWindowSize = GetInitialWindowSize();
        // The Affordance panoramas are 5760 px wide.
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.PreferredBackBufferWidth = initialWindowSize.X;
        _graphics.PreferredBackBufferHeight = initialWindowSize.Y;
        _graphics.ApplyChanges();

        _window.AllowUserResizing = true;
        _window.ClientSizeChanged += OnClientSizeChanged;
    }

    public void CreateRenderTarget(GraphicsDevice graphicsDevice)
    {
        _renderTarget = new RenderTarget2D(
            graphicsDevice,
            LogicalWidth,
            LogicalHeight);
    }

    public void BeginDraw(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.SetRenderTarget(_renderTarget);
        // Charcoal blue (#15191F, as in the menus): areas without art, such as outside the
        // room during Mort's zoom, stay dark instead of showing a bright default blue.
        graphicsDevice.Clear(new Color(21, 25, 31));
    }

    public void Present(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        graphicsDevice.SetRenderTarget(null);
        graphicsDevice.Clear(Color.Black);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(_renderTarget, GetPresentationRectangle(graphicsDevice.Viewport), Color.White);
        spriteBatch.End();
    }

    private static Point GetInitialWindowSize()
    {
        DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        int availableWidth = displayMode.Width - WindowMargin;
        int availableHeight = displayMode.Height - WindowMargin;
        float widthScale = availableWidth / (float)LogicalWidth;
        float heightScale = availableHeight / (float)LogicalHeight;
        float scale = Math.Min(1.0f, Math.Min(widthScale, heightScale));

        return new Point((int)(LogicalWidth * scale), (int)(LogicalHeight * scale));
    }

    public static Rectangle GetPresentationRectangle(Viewport viewport)
    {
        float widthScale = viewport.Width / (float)LogicalWidth;
        float heightScale = viewport.Height / (float)LogicalHeight;
        float scale = Math.Min(widthScale, heightScale);
        int width = (int)(LogicalWidth * scale);
        int height = (int)(LogicalHeight * scale);
        int x = (viewport.Width - width) / 2;
        int y = (viewport.Height - height) / 2;

        return new Rectangle(x, y, width, height);
    }

    private void OnClientSizeChanged(object sender, EventArgs eventArgs)
    {
        int width = _window.ClientBounds.Width;
        int height = _window.ClientBounds.Height;

        if (width <= 0 || height <= 0 ||
            _graphics.PreferredBackBufferWidth == width &&
            _graphics.PreferredBackBufferHeight == height)
        {
            return;
        }

        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges();
    }
}
