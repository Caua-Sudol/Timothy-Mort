using System;
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
    private const string ExitDoorDestinationLayerName = "destinoPortaSaida";
    private const string VerticalRoutePlatformLayerName = "rotaVerticalSolida";
    private const string VerticalRouteGelatinLayerName = "rotaVerticalGelatina";
    private const string DebugMeasurementLayerName = "medicaoDebug";
    private const string SceneDestinationPropertyName = "destino";
    private const string SceneDestinationSpawnPropertyName = "spawnDestino";
    private const string SceneRequiresAppliedSolutionPropertyName = "requerSolucaoAplicada";
    private const string InterventionEntrancePlacementPropertyName = "posicao";
    private const string DebugMeasurementPairIdPropertyName = "parId";
    private const string DebugMeasurementRolePropertyName = "papel";
    private const string DebugMeasurementActionPropertyName = "acao";
    private const string DebugMeasurementOriginRole = "origem";
    private const string DebugMeasurementDestinationRole = "destino";

    private readonly string _mapPath;
    private List<Rectangle> _platforms = new List<Rectangle>();
    private List<Rectangle> _gelatins = new List<Rectangle>();
    private List<Rectangle> _cutsceneTriggers = new List<Rectangle>();
    private List<SceneExit> _sceneExits = new List<SceneExit>();
    private List<InterventionEntrance> _interventionEntrances = new List<InterventionEntrance>();
    private List<Rectangle> _interventionButtons = new List<Rectangle>();
    private List<Rectangle> _verticalRoutePlatforms = new List<Rectangle>();
    private List<Rectangle> _verticalRouteGelatins = new List<Rectangle>();
    private Dictionary<string, Vector2> _playerSpawns = new Dictionary<string, Vector2>();
    private Texture2D _platformTexture;

    public Vector2? PlayerSpawn { get; private set; }
    public DebugMeasurementPair DebugMeasurementPair { get; private set; }
    public Rectangle? ExitDoorBounds { get; private set; }
    public Vector2 ExitDoorDestination { get; private set; }

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

    public Vector2 ExitDoorStartPosition
    {
        get
        {
            if (!ExitDoorBounds.HasValue)
            {
                return Vector2.Zero;
            }

            Rectangle bounds = ExitDoorBounds.Value;
            return new Vector2(bounds.X, bounds.Y);
        }
    }

    public IEnumerable<Rectangle> VerticalRoutePlatforms
    {
        get { return _verticalRoutePlatforms; }
    }

    public IEnumerable<Rectangle> VerticalRouteGelatins
    {
        get { return _verticalRouteGelatins; }
    }

    public LevelMap(string mapPath)
    {
        _mapPath = mapPath;
    }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        ReloadObjects();
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

    public SceneExit GetSceneExit(
        Rectangle hitBox,
        bool exitDoorIsOpen,
        bool appliedSolutionAllowsExit)
    {
        if (!exitDoorIsOpen && ExitDoorBounds.HasValue)
        {
            return null;
        }

        foreach (SceneExit sceneExit in _sceneExits)
        {
            if (sceneExit.RequiresAppliedSolution && !appliedSolutionAllowsExit)
            {
                continue;
            }

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

    public IEnumerable<Rectangle> GetSolidColliders(
        bool exitDoorIsOpen,
        bool verticalRouteRevealed,
        Vector2 exitDoorPosition)
    {
        foreach (Rectangle platform in _platforms)
        {
            yield return platform;
        }

        if (verticalRouteRevealed)
        {
            foreach (Rectangle platform in _verticalRoutePlatforms)
            {
                yield return platform;
            }
        }

        Rectangle? exitDoorCollider = GetExitDoorCollider(
            exitDoorIsOpen,
            exitDoorPosition);

        if (exitDoorCollider.HasValue)
        {
            yield return exitDoorCollider.Value;
        }
    }

    public Rectangle? GetExitDoorCollider(
        bool exitDoorIsOpen,
        Vector2 exitDoorPosition)
    {
        if (exitDoorIsOpen || !ExitDoorBounds.HasValue)
        {
            return null;
        }

        Rectangle bounds = ExitDoorBounds.Value;
        return new Rectangle(
            (int)exitDoorPosition.X,
            (int)exitDoorPosition.Y,
            bounds.Width,
            bounds.Height);
    }

    public IEnumerable<Rectangle> GetGelatinVolumes(bool verticalRouteRevealed)
    {
        foreach (Rectangle gelatin in _gelatins)
        {
            yield return gelatin;
        }

        if (!verticalRouteRevealed)
        {
            yield break;
        }

        foreach (Rectangle gelatin in _verticalRouteGelatins)
        {
            yield return gelatin;
        }
    }

    public void Draw(
        SpriteBatch spriteBatch,
        InterventionEntrancePlacement? activeInterventionEntrancePlacement,
        float interventionButtonPressOffset,
        Vector2 exitDoorPosition,
        bool exitDoorIsOpen,
        bool verticalRouteRevealed,
        SolutionVisualModule visualModules)
    {
        foreach (var platform in _platforms)
        {
            spriteBatch.Draw(_platformTexture, platform, Color.Purple);
        }

        foreach (Rectangle gelatin in _gelatins)
        {
            DrawGelatin(spriteBatch, gelatin, visualModules);
        }

        if (verticalRouteRevealed)
        {
            foreach (Rectangle platform in _verticalRoutePlatforms)
            {
                spriteBatch.Draw(_platformTexture, platform, Color.Purple);
            }

            foreach (Rectangle gelatin in _verticalRouteGelatins)
            {
                DrawGelatin(spriteBatch, gelatin, visualModules);
            }

            if ((visualModules & SolutionVisualModule.PATH_INDICATOR) != 0)
            {
                DrawPathIndicators(spriteBatch);
            }
        }

        if (activeInterventionEntrancePlacement.HasValue)
        {
            foreach (InterventionEntrance entrance in _interventionEntrances)
            {
                if (entrance.Placement == activeInterventionEntrancePlacement.Value)
                {
                    spriteBatch.Draw(_platformTexture, entrance.Bounds, Color.Orange);
                }
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

        Rectangle? exitDoorCollider = GetExitDoorCollider(
            exitDoorIsOpen,
            exitDoorPosition);

        if (!exitDoorCollider.HasValue)
        {
            return;
        }

        spriteBatch.Draw(_platformTexture, exitDoorCollider.Value, Color.Red);
    }

public void ReloadObjects()
{
    if (!TryReloadObjects(out string errorMessage))
    {
        throw new InvalidOperationException(errorMessage);
    }
}

public bool TryReloadObjects(out string errorMessage)
{
    if (!TmxMapValidator.TryLoad(_mapPath, out TmxMap map, out errorMessage))
    {
        return false;
    }

    var candidate = new LevelMap(_mapPath);

    try
    {
        candidate.LoadObjects(map);
    }
    catch (Exception)
    {
        errorMessage = "TMX reload failed while reading map objects.";
        return false;
    }

    ApplyObjectState(candidate);
    errorMessage = null;
    return true;
}

private void LoadObjects(TmxMap map)
{

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
            if (!TmxMapValidator.IsValidPosition(row.X, row.Y))
            {
                continue;
            }

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

                bool requiresAppliedSolution = false;
                if (row.Properties.TryGetValue(
                        SceneRequiresAppliedSolutionPropertyName,
                        out string requiresAppliedSolutionValue))
                {
                    bool.TryParse(requiresAppliedSolutionValue, out requiresAppliedSolution);
                }

                var bounds = new Rectangle((int)row.X, (int)row.Y, (int)row.Width, (int)row.Height);
                _sceneExits.Add(new SceneExit(
                    bounds,
                    destination,
                    destinationSpawn,
                    requiresAppliedSolution));
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
        LoadExitDoor(map);
        LoadRectanglesFromOptionalLayer(map, VerticalRoutePlatformLayerName, _verticalRoutePlatforms);
    LoadRectanglesFromOptionalLayer(map, VerticalRouteGelatinLayerName, _verticalRouteGelatins);
    LoadDebugMeasurementPair(map);
}

private void ApplyObjectState(LevelMap candidate)
{
    _platforms = candidate._platforms;
    _gelatins = candidate._gelatins;
    _cutsceneTriggers = candidate._cutsceneTriggers;
    _sceneExits = candidate._sceneExits;
    _interventionEntrances = candidate._interventionEntrances;
    _interventionButtons = candidate._interventionButtons;
    _verticalRoutePlatforms = candidate._verticalRoutePlatforms;
    _verticalRouteGelatins = candidate._verticalRouteGelatins;
    _playerSpawns = candidate._playerSpawns;
    PlayerSpawn = candidate.PlayerSpawn;
    DebugMeasurementPair = candidate.DebugMeasurementPair;
    ExitDoorBounds = candidate.ExitDoorBounds;
    ExitDoorDestination = candidate.ExitDoorDestination;
}

    private void LoadExitDoor(TmxMap map)
    {
        var doorLayer = map.ObjectGroups.FirstOrDefault(
            group => group.Name == ExitDoorLayerName);
        var doorObject = doorLayer?.Objects.FirstOrDefault();

        if (doorObject == null)
        {
            return;
        }

        ExitDoorBounds = new Rectangle(
            (int)doorObject.X,
            (int)doorObject.Y,
            (int)doorObject.Width,
            (int)doorObject.Height);
        ExitDoorDestination = ExitDoorStartPosition;

        var destinationLayer = map.ObjectGroups.FirstOrDefault(
            group => group.Name == ExitDoorDestinationLayerName);
        var destinationObject = destinationLayer?.Objects.FirstOrDefault();

        if (destinationObject != null)
        {
            ExitDoorDestination = new Vector2(
                (float)destinationObject.X,
                (float)destinationObject.Y);
        }
    }

    private void LoadDebugMeasurementPair(TmxMap map)
    {
        var layer = map.ObjectGroups.FirstOrDefault(
            group => group.Name == DebugMeasurementLayerName);

        if (layer == null)
        {
            return;
        }

        Vector2? origin = null;
        Vector2? destination = null;
        string pairId = null;
        string action = null;
        bool pairIsInvalid = false;

        foreach (var row in layer.Objects)
        {
            if (!row.Properties.TryGetValue(
                    DebugMeasurementPairIdPropertyName,
                    out string markerPairId) ||
                string.IsNullOrWhiteSpace(markerPairId) ||
                !row.Properties.TryGetValue(
                    DebugMeasurementRolePropertyName,
                    out string markerRole) ||
                !row.Properties.TryGetValue(
                    DebugMeasurementActionPropertyName,
                    out string markerAction) ||
                string.IsNullOrWhiteSpace(markerAction) ||
                markerRole != DebugMeasurementOriginRole &&
                markerRole != DebugMeasurementDestinationRole)
            {
                continue;
            }

            if (pairId == null)
            {
                pairId = markerPairId;
                action = markerAction;
            }
            else if (pairId != markerPairId || action != markerAction)
            {
                pairIsInvalid = true;
                break;
            }

            var position = new Vector2((float)row.X, (float)row.Y);

            if (markerRole == DebugMeasurementOriginRole)
            {
                if (origin.HasValue)
                {
                    pairIsInvalid = true;
                    break;
                }

                origin = position;
            }
            else
            {
                if (destination.HasValue)
                {
                    pairIsInvalid = true;
                    break;
                }

                destination = position;
            }
        }

        if (pairIsInvalid || !origin.HasValue || !destination.HasValue)
        {
            return;
        }

        DebugMeasurementPair = new DebugMeasurementPair(
            pairId,
            action,
            origin.Value,
            destination.Value);
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
        var platformColor = Enumerable.Repeat(Color.White, TileSize * TileSize).ToArray();
        _platformTexture = new Texture2D(graphicsDevice, TileSize, TileSize);
        _platformTexture.SetData(platformColor);
    }

    private void DrawGelatin(
        SpriteBatch spriteBatch,
        Rectangle gelatin,
        SolutionVisualModule visualModules)
    {
        Color gelatinColor = (visualModules & SolutionVisualModule.GELATIN_COLOR) != 0
            ? Color.DeepSkyBlue
            : (visualModules & SolutionVisualModule.GELATIN_TEXTURE) != 0
                ? Color.CornflowerBlue
                : (visualModules & SolutionVisualModule.GELATIN_GLOW) != 0
                    ? Color.Cyan
                    : Color.Purple;

        if ((visualModules & SolutionVisualModule.GELATIN_GLOW) != 0)
        {
            var glow = new Rectangle(
                gelatin.X - 8,
                gelatin.Y - 8,
                gelatin.Width + 16,
                gelatin.Height + 16);
            spriteBatch.Draw(_platformTexture, glow, Color.Cyan * 0.35f);
        }

        spriteBatch.Draw(_platformTexture, gelatin, gelatinColor);

        if ((visualModules & SolutionVisualModule.GELATIN_TEXTURE) == 0)
        {
            return;
        }

        for (int x = gelatin.Left + 8; x < gelatin.Right; x += 24)
        {
            spriteBatch.Draw(
                _platformTexture,
                new Rectangle(x, gelatin.Top, 6, gelatin.Height),
                Color.White * 0.65f);
        }
    }

    private void DrawPathIndicators(SpriteBatch spriteBatch)
    {
        foreach (Rectangle platform in _verticalRoutePlatforms)
        {
            var marker = new Rectangle(
                platform.Center.X - 24,
                platform.Top - 20,
                48,
                8);
            spriteBatch.Draw(_platformTexture, marker, Color.Gold);
        }

        foreach (Rectangle gelatin in _verticalRouteGelatins)
        {
            var marker = new Rectangle(
                gelatin.Center.X - 24,
                gelatin.Top - 20,
                48,
                8);
            spriteBatch.Draw(_platformTexture, marker, Color.Gold);
        }
    }
}
