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
    private const string InterventionEntranceLayerName = "entradaIntervencao";
    private const string InterventionButtonLayerName = "botaoIntervencao";
    private const string ExitDoorLayerName = "portaSaida";
    private const string SceneDestinationPropertyName = "destino";
    private const string SceneDestinationSpawnPropertyName = "spawnDestino";
    private const string InterventionEntrancePlacementPropertyName = "posicao";

    private readonly string _mapPath;
    private readonly List<Rectangle> _platforms = new List<Rectangle>();
    private readonly List<Rectangle> _gelatins = new List<Rectangle>();
    private readonly List<Rectangle> _cutsceneTriggers = new List<Rectangle>();
    private readonly List<SceneExit> _sceneExits = new List<SceneExit>();
    private readonly List<InterventionEntrance> _interventionEntrances = new List<InterventionEntrance>();
    private readonly List<Rectangle> _interventionButtons = new List<Rectangle>();
    private readonly List<Rectangle> _exitDoors = new List<Rectangle>();
    private readonly Dictionary<string, Vector2> _playerSpawns = new Dictionary<string, Vector2>();
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

    public IEnumerable<InterventionEntrance> InterventionEntrances
    {
        get
        {
            return _interventionEntrances;
        }
    }

    public IEnumerable<Rectangle> InterventionButtons
    {
        get
        {
            return _interventionButtons;
        }
    }

    public IEnumerable<Rectangle> ExitDoors
    {
        get
        {
            return _exitDoors;
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

    public Vector2? GetPlayerSpawn(string spawnId)
    {
        if (!string.IsNullOrWhiteSpace(spawnId) &&
            _playerSpawns.TryGetValue(spawnId, out Vector2 spawn))
        {
            return spawn;
        }

        return null;
    }

    public SceneExit GetSceneExit(Rectangle hitBox)
    {
        foreach (SceneExit sceneExit in _sceneExits)
        {
            if (hitBox.Intersects(sceneExit.Bounds))
            {
                return sceneExit;
            }
        }

        return null;
    }

    public InterventionEntrance GetInterventionEntrance(
        Rectangle hitBox,
        InterventionEntrancePlacement? activePlacement)
    {
        if (!activePlacement.HasValue)
        {
            return null;
        }

        foreach (InterventionEntrance entrance in _interventionEntrances)
        {
            if (entrance.Placement == activePlacement.Value &&
                hitBox.Intersects(entrance.Bounds))
            {
                return entrance;
            }
        }

        return null;
    }

    public bool HasInterventionButtonUnderPlayer(Rectangle playerHitBox, bool playerIsGrounded)
    {
        if (!playerIsGrounded)
        {
            return false;
        }

        var playerFeet = new Rectangle(
            playerHitBox.X,
            playerHitBox.Bottom - 1,
            playerHitBox.Width,
            1);

        foreach (Rectangle button in _interventionButtons)
        {
            if (playerFeet.Intersects(button))
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerable<Rectangle> GetSolidColliders(bool exitDoorIsOpen)
    {
        foreach (Rectangle platform in _platforms)
        {
            yield return platform;
        }

        if (exitDoorIsOpen)
        {
            yield break;
        }

        foreach (Rectangle door in _exitDoors)
        {
            yield return door;
        }
    }

    public void Draw(
        SpriteBatch spriteBatch,
        InterventionEntrancePlacement? activeInterventionEntrancePlacement,
        float interventionButtonPressOffset,
        bool exitDoorIsOpen)
    {
        foreach (var platform in _platforms)
        {
            spriteBatch.Draw(_platformTexture, platform, Color.Purple);
        }

        foreach (var gelatin in _gelatins)
        {
            spriteBatch.Draw(_platformTexture, gelatin, Color.Purple);
        }

        if (!activeInterventionEntrancePlacement.HasValue)
        {
            return;
        }

        foreach (InterventionEntrance entrance in _interventionEntrances)
        {
            if (entrance.Placement == activeInterventionEntrancePlacement.Value)
            {
                spriteBatch.Draw(_platformTexture, entrance.Bounds, Color.Orange);
            }
        }

        foreach (Rectangle button in _interventionButtons)
        {
            var pressedButton = new Rectangle(
                button.X,
                button.Y + (int)interventionButtonPressOffset,
                button.Width,
                button.Height);
            spriteBatch.Draw(_platformTexture, pressedButton, Color.Orange);
        }

        if (exitDoorIsOpen)
        {
            return;
        }

        foreach (Rectangle door in _exitDoors)
        {
            spriteBatch.Draw(_platformTexture, door, Color.Red);
        }
    }

    private void LoadObjects()
    {
        _platforms.Clear();
        _gelatins.Clear();
        _cutsceneTriggers.Clear();
        _sceneExits.Clear();
        _interventionEntrances.Clear();
        _interventionButtons.Clear();
        _exitDoors.Clear();
        _playerSpawns.Clear();
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
        if (playerSpawnLayer != null)
        {
            foreach (var row in playerSpawnLayer.Objects)
            {
                var spawn = new Vector2((float)row.X, (float)row.Y);

                if (!PlayerSpawn.HasValue)
                {
                    PlayerSpawn = spawn;
                }

                if (!string.IsNullOrWhiteSpace(row.Name))
                {
                    _playerSpawns[row.Name] = spawn;
                }
            }
        }

        var sceneExitLayer = map.ObjectGroups.FirstOrDefault(
            layer => layer.Name == SceneExitLayerName);

        if (sceneExitLayer != null)
        {
            foreach (var row in sceneExitLayer.Objects)
            {
                if (!row.Properties.TryGetValue(SceneDestinationPropertyName, out string destination) ||
                    string.IsNullOrWhiteSpace(destination))
                {
                    continue;
                }

                row.Properties.TryGetValue(SceneDestinationSpawnPropertyName, out string destinationSpawn);

                var bounds = new Rectangle((int)row.X, (int)row.Y, (int)row.Width, (int)row.Height);
                _sceneExits.Add(new SceneExit(bounds, destination, destinationSpawn));
            }
        }

        var entranceLayer = map.ObjectGroups.FirstOrDefault(
            layer => layer.Name == InterventionEntranceLayerName);

        if (entranceLayer != null)
        {
            foreach (var row in entranceLayer.Objects)
            {
                if (!row.Properties.TryGetValue(SceneDestinationPropertyName, out string destination) ||
                    string.IsNullOrWhiteSpace(destination) ||
                    !row.Properties.TryGetValue(
                        InterventionEntrancePlacementPropertyName,
                        out string placementValue) ||
                    !TryParseInterventionEntrancePlacement(
                        placementValue,
                        out InterventionEntrancePlacement placement))
                {
                    continue;
                }

                row.Properties.TryGetValue(SceneDestinationSpawnPropertyName, out string destinationSpawn);

                var bounds = new Rectangle((int)row.X, (int)row.Y, (int)row.Width, (int)row.Height);
                _interventionEntrances.Add(new InterventionEntrance(
                    bounds,
                    destination,
                    destinationSpawn,
                    placement));
            }
        }

        LoadRectanglesFromOptionalLayer(map, InterventionButtonLayerName, _interventionButtons);
        LoadRectanglesFromOptionalLayer(map, ExitDoorLayerName, _exitDoors);
    }

    private void LoadRectanglesFromOptionalLayer(
        TmxMap map,
        string layerName,
        List<Rectangle> rectangles)
    {
        var layer = map.ObjectGroups.FirstOrDefault(group => group.Name == layerName);

        if (layer == null)
        {
            return;
        }

        foreach (var row in layer.Objects)
        {
            rectangles.Add(new Rectangle(
                (int)row.X,
                (int)row.Y,
                (int)row.Width,
                (int)row.Height));
        }
    }

    private bool TryParseInterventionEntrancePlacement(
        string value,
        out InterventionEntrancePlacement placement)
    {
        if (value == "tardia")
        {
            placement = InterventionEntrancePlacement.LATE;
            return true;
        }

        if (value == "antecipada")
        {
            placement = InterventionEntrancePlacement.EARLY;
            return true;
        }

        placement = default;
        return false;
    }

    private void LoadPlatformTexture(GraphicsDevice graphicsDevice)
    {
        var platformColor = Enumerable.Repeat(Color.Purple, TileSize * TileSize).ToArray();
        _platformTexture = new Texture2D(graphicsDevice, TileSize, TileSize);
        _platformTexture.SetData(platformColor);
    }
}
