using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace DontLikePoetry;

public class LevelMap
{
    private const int TileSize = 32;
    private const string PlatformLayerName = "mapaGeral";
    private const string GelatinLayerName = "gelatina";
    private const string CutsceneTriggerLayerName = "triggerColision";
    private const string PlayerSpawnLayerName = "spawnJogador";
    private const string SceneExitLayerName = "saidaCena";
    private const string SceneDestinationPropertyName = "destino";

    private readonly string _mapPath;
    private readonly List<Rectangle> _platforms = new List<Rectangle>();
    private readonly List<Rectangle> _gelatins = new List<Rectangle>();
    private readonly List<Rectangle> _cutsceneTriggers = new List<Rectangle>();
    private readonly List<SceneExit> _sceneExits = new List<SceneExit>();
    private Texture2D _platformTexture;

    public Vector2? PlayerSpawn { get; private set; }

    public IEnumerable<Rectangle> Platforms
    {
        get
        {
            return _platforms;
        }
    }

    public IEnumerable<Rectangle> Gelatins
    {
        get
        {
            return _gelatins;
        }
    }

    public IEnumerable<Rectangle> CutsceneTriggers
    {
        get
        {
            return _cutsceneTriggers;
        }
    }

    public IEnumerable<SceneExit> SceneExits
    {
        get
        {
            return _sceneExits;
        }
    }

    public LevelMap(string mapPath)
    {
        _mapPath = mapPath;
    }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        LoadObjects();
        LoadPlatformTexture(graphicsDevice);
    }

    public bool HasCutsceneTrigger(Rectangle hitBox)
    {
        foreach (var trigger in _cutsceneTriggers)
        {
            if (hitBox.Intersects(trigger))
            {
                return true;
            }
        }

        return false;
    }

    public string GetSceneDestination(Rectangle hitBox)
    {
        foreach (SceneExit sceneExit in _sceneExits)
        {
            if (hitBox.Intersects(sceneExit.Bounds))
            {
                return sceneExit.DestinationScenarioId;
            }
        }

        return null;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var platform in _platforms)
        {
            spriteBatch.Draw(_platformTexture, platform, Color.Purple);
        }

        foreach (var gelatin in _gelatins)
        {
            spriteBatch.Draw(_platformTexture, gelatin, Color.Purple);
        }
    }

    private void LoadObjects()
    {
        _platforms.Clear();
        _gelatins.Clear();
        _cutsceneTriggers.Clear();
        _sceneExits.Clear();
        PlayerSpawn = null;

        var map = new TmxMap(_mapPath);

        foreach (var row in map.ObjectGroups[PlatformLayerName].Objects)
        {
            _platforms.Add(new Rectangle((int)row.X, (int)row.Y, (int)row.Width, (int)row.Height));
        }

        var gelatinLayer = map.ObjectGroups.FirstOrDefault(
            layer => layer.Name == GelatinLayerName);

        if (gelatinLayer != null)
        {
            foreach (var row in gelatinLayer.Objects)
            {
                _gelatins.Add(new Rectangle((int)row.X, (int)row.Y, (int)row.Width, (int)row.Height));
            }
        }

        var cutsceneTriggerLayer = map.ObjectGroups.FirstOrDefault(
            layer => layer.Name == CutsceneTriggerLayerName);

        if (cutsceneTriggerLayer != null)
        {
            foreach (var row in cutsceneTriggerLayer.Objects)
            {
                _cutsceneTriggers.Add(new Rectangle((int)row.X, (int)row.Y, (int)row.Width, (int)row.Height));
            }
        }

        var playerSpawnLayer = map.ObjectGroups.FirstOrDefault(
            layer => layer.Name == PlayerSpawnLayerName);
        var playerSpawnObject = playerSpawnLayer?.Objects.FirstOrDefault();

        if (playerSpawnObject != null)
        {
            PlayerSpawn = new Vector2((float)playerSpawnObject.X, (float)playerSpawnObject.Y);
        }

        var sceneExitLayer = map.ObjectGroups.FirstOrDefault(
            layer => layer.Name == SceneExitLayerName);

        if (sceneExitLayer == null)
        {
            return;
        }

        foreach (var row in sceneExitLayer.Objects)
        {
            if (!row.Properties.TryGetValue(SceneDestinationPropertyName, out string destination) ||
                string.IsNullOrWhiteSpace(destination))
            {
                continue;
            }

            var bounds = new Rectangle((int)row.X, (int)row.Y, (int)row.Width, (int)row.Height);
            _sceneExits.Add(new SceneExit(bounds, destination));
        }
    }

    private void LoadPlatformTexture(GraphicsDevice graphicsDevice)
    {
        var platformColor = Enumerable.Repeat(Color.Purple, TileSize * TileSize).ToArray();
        _platformTexture = new Texture2D(graphicsDevice, TileSize, TileSize);
        _platformTexture.SetData(platformColor);
    }
}
