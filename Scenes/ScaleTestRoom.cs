using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

// Standalone composition room, entered from phase selection without study progress.
internal sealed class ScaleTestRoom
{
    private readonly Scene _scene;
    private readonly Camera _camera;
    private readonly DebugView _debug;
    private readonly SpriteFont _font;
    private readonly Texture2D _background;
    private readonly Texture2D _lights;
    private readonly PulsingLightPreview _lightPreview;
    private readonly Texture2D _pixel;
    private readonly LdtkMolduraPreview _ldtkMoldura;
    private bool _showOriginal;
    private int _afterPreset = GameScaleConfig.DefaultPlayerScalePreset;
    private string _reloadError;

    public ScaleTestRoom(GraphicsDevice graphics, ContentManager content, Vector2 dimensions)
    {
        _font = content.Load<SpriteFont>("font");
        _background = content.Load<Texture2D>("visual/bg_prot");
        _lights = content.Load<Texture2D>("visual/affordance_visibility_lgt");
        _lightPreview = new PulsingLightPreview(content);
        _ldtkMoldura = new LdtkMolduraPreview(content,
            "assets/teste_ldtk/teste_moldura.ldtk");
        _pixel = new Texture2D(graphics, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _debug = new DebugView(_font);
        _debug.LoadContent(graphics);
        _debug.Toggle();
        _camera = new Camera(Vector2.Zero, dimensions, GameScaleConfig.OriginalCameraZoom);
        _scene = new Scene(new ScenarioDefinition(
            GameScaleConfig.ScaleTestScenarioId, GameScaleConfig.ScaleTestMapPath, Vector2.Zero));
        _scene.LoadContent(graphics, content);
        _scene.SetAdditionalSolidColliders(_ldtkMoldura.FrameColliders);
        _scene.SetAdditionalGelatinVolumes(_ldtkMoldura.GelatinVolumes);
        _scene.UseAdditionalCollidersOnly();
        _scene.AlignCamera(_camera);
    }

    public void ToggleDebug() => _debug.Toggle();

    public void Update(KeyboardState keys, KeyboardState previous, double deltaTime)
    {
        bool Pressed(Keys key) => keys.IsKeyDown(key) && previous.IsKeyUp(key);
        _lightPreview.Update(keys, previous, deltaTime);

        if (keys.IsKeyDown(Keys.LeftControl) && Pressed(Keys.R))
        {
            _scene.TryReloadMap(_camera, out _reloadError);
            return;
        }

        if (Pressed(Keys.B))
            ResetComparison(!_showOriginal, _afterPreset);
        else if (Pressed(Keys.R))
            _scene.Restart(_camera);

        int requestedPreset =
            Pressed(Keys.D1) || Pressed(Keys.NumPad1) ? 1 :
            Pressed(Keys.D2) || Pressed(Keys.NumPad2) ? 2 :
            Pressed(Keys.D3) || Pressed(Keys.NumPad3) ? 3 :
            Pressed(Keys.D4) || Pressed(Keys.NumPad4) ? 4 : 0;
        if (requestedPreset != 0)
        {
            if (_showOriginal)
                ResetComparison(false, requestedPreset);
            else if (_scene.TrySetPlayerScale(requestedPreset))
                _afterPreset = requestedPreset;
        }

        _scene.Update(_camera, deltaTime);
        if (_scene.PlayerIsDead)
            _scene.Restart(_camera);
    }

    private void ResetComparison(bool original, int afterPreset)
    {
        // Reset first so growing back to the original collider is safe at the spawn.
        _scene.Restart(_camera);
        if (!_scene.TrySetPlayerScale(original ? 1 : afterPreset))
            return;

        _afterPreset = afterPreset;
        _showOriginal = original;
        _scene.UseOriginalScaleTestCamera = original;
        _scene.Restart(_camera);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var viewport = spriteBatch.GraphicsDevice.Viewport;
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(_background, new Rectangle(0, 0, viewport.Width, viewport.Height), Color.White);
        spriteBatch.End();

        _scene.Draw(spriteBatch, _camera);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.GetTransform());
        _ldtkMoldura.Draw(spriteBatch);
        spriteBatch.End();
        // Preview the first room's light layer at its original panorama proportions.
        spriteBatch.Begin(blendState: BlendState.AlphaBlend,
            samplerState: SamplerState.LinearClamp, effect: _lightPreview.Prepare());
        spriteBatch.Draw(_lights, Vector2.Zero, null, Color.White,
            0f, Vector2.Zero, viewport.Height / (float)_lights.Height, SpriteEffects.None, 0f);
        spriteBatch.End();
        _scene.DrawDebug(spriteBatch, _camera, _debug);
        _debug.DrawHud(spriteBatch, GameScaleConfig.ScaleTestScenarioId, _scene.Player);

        string label =
            $"SALA DE TESTE | {(_showOriginal ? "ANTES" : "DEPOIS")} | " +
            $"zoom {_camera.Zoom:F3} | player {_scene.Player.RelativeScale:F2}\n" +
            "B: antes/depois | 1/2/3/4: escala | R: spawn | Ctrl+R: TMX\n" +
            "Ctrl+P: grade | Esc: seleção de fases\n" +
            _lightPreview.Status +
            (_scene.Player.ScaleChangeBlocked ? " | ESCALA BLOQUEADA" : "");
        int margin = GameScaleConfig.ScaleTestHudMargin;
        Vector2 size = _font.MeasureString(label);
        var panel = new Rectangle(margin, viewport.Height - (int)size.Y - margin * 2,
            (int)size.X + margin * 2, (int)size.Y + margin);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(_pixel, panel, Color.Black * GameScaleConfig.DebugPanelOpacity);
        spriteBatch.DrawString(_font, label, new Vector2(panel.X + margin, panel.Y), Color.White);
        spriteBatch.End();
        _debug.DrawReloadError(spriteBatch, _reloadError);
    }
}
