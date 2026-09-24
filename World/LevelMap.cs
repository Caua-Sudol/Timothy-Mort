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
    private const string TrampolineGelatinLayerName = "gelatinaTrampolim";
    private const string CutsceneTriggerLayerName = "triggerColision";
    private const string PlayerSpawnLayerName = "spawnJogador";
    private const string SceneExitLayerName = "saidaCena";
    private const string InterventionEntranceLayerName = "entradaIntervencao";
    private const string InterventionButtonLayerName = "botaoIntervencao";
    private const string ExitDoorLayerName = "portaSaida";
    private const string ExitDoorDestinationLayerName = "destinoPortaSaida";
    private const string VerticalRoutePlatformLayerName = "rotaVerticalSolida";
    private const string VerticalRouteGelatinLayerName = "rotaVerticalGelatina";
    private const string DamageLayerName = "dano";
    private const string HarmlessSpikeLayerName = "espinhoInofensivo";
    private const string VerticalRouteDamageLayerName = "rotaVerticalDano";
    private const string SafeRouteSolidLayerName = "rotaSeguraSolida";
    private const string DebugMeasurementLayerName = "medicaoDebug";
    private const string PropVisualForegroundLayerName = "visualForeground";
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
    private List<DynamicPlatform> _dynamicPlatforms = new List<DynamicPlatform>();
    private List<Rectangle> _platforms = new List<Rectangle>();
    private List<Rectangle> _gelatins = new List<Rectangle>();
    private List<Rectangle> _trampolineGelatins = new List<Rectangle>();
    private List<Rectangle> _cutsceneTriggers = new List<Rectangle>();
    private List<SceneExit> _sceneExits = new List<SceneExit>();
    private List<InterventionEntrance> _interventionEntrances = new List<InterventionEntrance>();
    private List<Rectangle> _interventionButtons = new List<Rectangle>();
    private List<Rectangle> _verticalRoutePlatforms = new List<Rectangle>();
    private List<Rectangle> _safeRoutePlatforms = new List<Rectangle>();
    private List<Rectangle> _verticalRouteGelatins = new List<Rectangle>();
    private List<DamageVolume> _damageVolumes = new List<DamageVolume>();
    private List<DamageVolume> _harmlessSpikeVolumes = new List<DamageVolume>();
    private List<DamageVolume> _verticalRouteDamageVolumes = new List<DamageVolume>();
    private List<Rectangle> _propVisualForegrounds = new List<Rectangle>();
    private Dictionary<string, Vector2> _playerSpawns = new Dictionary<string, Vector2>();
    private Texture2D _platformTexture;
    private readonly Dictionary<string, Texture2D> _objectTextures = new Dictionary<string, Texture2D>();
    private Dictionary<(string Layer, Rectangle Bounds), string> _objectNames =
        new Dictionary<(string Layer, Rectangle Bounds), string>();

    public void SetObjectTexture(string objectName, Texture2D texture)
    {
        _objectTextures[objectName] = texture;
    }

    private bool TryGetObjectTexture(string layer, Rectangle bounds, out Texture2D texture)
    {
        texture = null;
        return _objectNames.TryGetValue((layer, bounds), out string name) &&
            _objectTextures.TryGetValue(name, out texture);
    }
    private Texture2D _woodPlatformTexture;
    private Texture2D _initialCheckpointTexture;
    private Rectangle? _initialCheckpointBounds;

    public void SetInitialCheckpointTexture(Texture2D texture)
    {
        _initialCheckpointTexture = texture;
    }
    private HashSet<Rectangle> _woodPlatforms = new HashSet<Rectangle>();

    public void SetWoodPlatformTexture(Texture2D texture)
    {
        _woodPlatformTexture = texture;
    }
    private double _visualEffectTime;

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
            return _gelatins.Concat(_trampolineGelatins);
        }
    }

    public IEnumerable<Rectangle> TrampolineGelatins
    {
        get { return _trampolineGelatins; }
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

    public IEnumerable<Rectangle> VerticalRoutePlatforms => _verticalRoutePlatforms; // Isso é equivalente a public...{get{return _verticalRoutePlatforms}}

    public IEnumerable<Rectangle> VerticalRouteGelatins
    {
        get { return _verticalRouteGelatins; }
    }

    public IEnumerable<DamageVolume> DamageVolumes
    {
        get { return _damageVolumes; }
    }

    public IEnumerable<DamageVolume> HarmlessSpikeVolumes
    {
        get { return _harmlessSpikeVolumes; }
    }

    public IEnumerable<DamageVolume> VerticalRouteDamageVolumes
    {
        get { return _verticalRouteDamageVolumes; }
    }

    public IEnumerable<Rectangle> PropVisualForegrounds
    {
        get { return _propVisualForegrounds; }
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

public void UpdateDynamicPlatforms(Player player, double deltaTime)
{
    foreach (DynamicPlatform platform in _dynamicPlatforms) platform.Update(player, deltaTime);
}

public void ResetDynamicPlatforms()
{
    foreach (DynamicPlatform platform in _dynamicPlatforms) platform.Reset();
}

public void UpdateVisualEffects(double deltaTime)
{
    _visualEffectTime += deltaTime;
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
        bool safeRouteRevealed,
        Vector2 exitDoorPosition)
    {
        foreach (Rectangle platform in _platforms)
        {
            yield return platform;
        }
        foreach (DynamicPlatform platform in _dynamicPlatforms)
            if (platform.Active) yield return platform.Bounds;

        if (verticalRouteRevealed)
        {
            foreach (Rectangle platform in _verticalRoutePlatforms)
            {
                yield return platform;
            }
        }

        if (safeRouteRevealed)
        {
            foreach (Rectangle platform in _safeRoutePlatforms)
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

        foreach (Rectangle trampoline in _trampolineGelatins)
        {
            yield return trampoline;
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

    public bool IsTrampolineGelatin(Rectangle gelatin)
    {
        return _trampolineGelatins.Contains(gelatin);
    }

    public DamageVolume GetDamageVolume(
        Rectangle hitBox,
        bool verticalRouteRevealed)
    {
        foreach (DamageVolume damageVolume in _damageVolumes)
        {
            if (hitBox.Intersects(damageVolume.Bounds))
            {
                return damageVolume;
            }
        }

        if (!verticalRouteRevealed)
        {
            return null;
        }

        foreach (DamageVolume damageVolume in _verticalRouteDamageVolumes)
        {
            if (hitBox.Intersects(damageVolume.Bounds))
            {
                return damageVolume;
            }
        }

        return null;
    }

    public DamageVolume GetHarmlessSpikeVolume(Rectangle hitBox)
    {
        foreach (DamageVolume damageVolume in _harmlessSpikeVolumes)
        {
            if (hitBox.Intersects(damageVolume.Bounds))
            {
                return damageVolume;
            }
        }

        return null;
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
        foreach (DynamicPlatform platform in _dynamicPlatforms)
        {
            if (!platform.Active) continue;
            Color color = platform.IsBreakable
                ? Color.Lerp(Color.SandyBrown, Color.IndianRed, platform.BreakProgress)
                : Color.SteelBlue;
            spriteBatch.Draw(_platformTexture, platform.Bounds, color);
            spriteBatch.Draw(_platformTexture,
                new Rectangle(platform.Bounds.X + 8, platform.Bounds.Y + 4,
                    platform.Bounds.Width - 16, 3), Color.White * 0.5f);
        }
        foreach (Rectangle platform in _platforms)
        {
            if (TryGetObjectTexture(PlatformLayerName, platform, out Texture2D objectTexture))
                spriteBatch.Draw(objectTexture, platform, Color.White);
            else if (_initialCheckpointTexture != null && _initialCheckpointBounds == platform)
                spriteBatch.Draw(_initialCheckpointTexture, platform, Color.White);
            else if (_woodPlatformTexture != null && _woodPlatforms.Contains(platform))
                spriteBatch.Draw(_woodPlatformTexture, platform, Color.White);
            else
                spriteBatch.Draw(_platformTexture, platform, Color.Purple);
            DrawSharedPlatformDecoration(spriteBatch, platform, visualModules);
            DrawSharedPlatformOutline(spriteBatch, platform, visualModules);
        }

        foreach (Rectangle gelatin in _gelatins)
        {
            DrawGelatin(spriteBatch, gelatin, visualModules);
            DrawSharedPlatformDecoration(spriteBatch, gelatin, visualModules);
            DrawSharedPlatformOutline(spriteBatch, gelatin, visualModules);
        }

        foreach (Rectangle trampoline in _trampolineGelatins)
        {
            Color? trampolineColor = visualModules.HasFlag(
                SolutionVisualModule.DISTINCT_TRAMPOLINE_GELATIN)
                ? Color.LimeGreen
                : null;
            DrawGelatin(
                spriteBatch,
                trampoline,
                visualModules,
                trampolineColor);
            DrawSharedPlatformDecoration(spriteBatch, trampoline, visualModules);
            DrawSharedPlatformOutline(spriteBatch, trampoline, visualModules);
        }

        if (verticalRouteRevealed)
        {
            foreach (Rectangle platform in _verticalRoutePlatforms)
            {
                spriteBatch.Draw(_platformTexture, platform, Color.Purple);
                DrawSharedPlatformDecoration(spriteBatch, platform, visualModules);
                DrawSharedPlatformOutline(spriteBatch, platform, visualModules);
            }

            foreach (Rectangle gelatin in _verticalRouteGelatins)
            {
                DrawGelatin(spriteBatch, gelatin, visualModules);
                DrawSharedPlatformDecoration(spriteBatch, gelatin, visualModules);
                DrawSharedPlatformOutline(spriteBatch, gelatin, visualModules);
            }

            if ((visualModules & SolutionVisualModule.PATH_INDICATOR) != 0)
            {
                DrawPathIndicators(spriteBatch);
            }
        }

        if (visualModules.HasFlag(SolutionVisualModule.SAFE_ROUTE))
        {
            foreach (Rectangle platform in _safeRoutePlatforms)
            {
                spriteBatch.Draw(_platformTexture, platform, new Color(38, 122, 82));
                spriteBatch.Draw(
                    _platformTexture,
                    new Rectangle(platform.X, platform.Y, platform.Width, 5),
                    Color.LightGreen);

                int markerY = platform.Y + Math.Max(8, platform.Height / 2);
                for (int markerX = platform.X + 20; markerX < platform.Right - 8; markerX += 40)
                {
                    spriteBatch.Draw(
                        _platformTexture,
                        new Rectangle(markerX, markerY, 12, 4),
                        Color.LightGreen);
                    spriteBatch.Draw(
                        _platformTexture,
                        new Rectangle(markerX + 8, markerY - 4, 4, 12),
                        Color.LightGreen);
                }
            }
        }

        DrawDamageVolumes(spriteBatch, _damageVolumes, visualModules, false);
        DrawDamageVolumes(
            spriteBatch,
            _harmlessSpikeVolumes,
            visualModules,
            visualModules.HasFlag(SolutionVisualModule.DISTINCT_HARMLESS_SPIKES));

        if (verticalRouteRevealed)
        {
            DrawDamageVolumes(
                spriteBatch,
                _verticalRouteDamageVolumes,
                visualModules,
                false);
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

        if (exitDoorCollider.HasValue)
        {
            spriteBatch.Draw(_platformTexture, exitDoorCollider.Value, Color.Red);
        }

        if ((visualModules & SolutionVisualModule.DESTINATION_HIGHLIGHT) != 0)
        {
            DrawDestinationHighlights(spriteBatch);
        }
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
    foreach (var layer in map.ObjectGroups)
    {
        if (layer.Name != PlatformLayerName && layer.Name != GelatinLayerName &&
            layer.Name != DamageLayerName)
            continue;
        foreach (var obj in layer.Objects)
        {
            if (string.IsNullOrWhiteSpace(obj.Name)) continue;
            var bounds = new Rectangle((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height);
            _objectNames[(layer.Name, bounds)] = obj.Name;
        }
    }

    foreach (var layer in map.ObjectGroups)
        if (layer.Name == "plataformaMovel" || layer.Name == "plataformaQuebravel")
            foreach (var obj in layer.Objects)
                _dynamicPlatforms.Add(new DynamicPlatform(new Rectangle((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height), obj.Properties, layer.Name == "plataformaQuebravel"));


        foreach (var row in map.ObjectGroups[PlatformLayerName].Objects)
        {
            var bounds = new Rectangle((int)row.X, (int)row.Y, (int)row.Width, (int)row.Height);
            _platforms.Add(bounds);
            if (row.Name == "inicio_checkpoint")
                _initialCheckpointBounds = bounds;
            if (row.Name != null && row.Name.StartsWith("wood", StringComparison.OrdinalIgnoreCase) &&
                row.Name.Length > 4 && row.Name.Skip(4).All(char.IsDigit))
                _woodPlatforms.Add(bounds);
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

        LoadRectanglesFromOptionalLayer(
            map,
            TrampolineGelatinLayerName,
            _trampolineGelatins);

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
        LoadRectanglesFromOptionalLayer(map, SafeRouteSolidLayerName, _safeRoutePlatforms);
    LoadRectanglesFromOptionalLayer(map, VerticalRouteGelatinLayerName, _verticalRouteGelatins);
    LoadDamageVolumesFromOptionalLayer(map, DamageLayerName, _damageVolumes);
    LoadDamageVolumesFromOptionalLayer(
        map,
        HarmlessSpikeLayerName,
        _harmlessSpikeVolumes);
    LoadDamageVolumesFromOptionalLayer(
        map,
        VerticalRouteDamageLayerName,
        _verticalRouteDamageVolumes);
    LoadRectanglesFromOptionalLayer(
        map,
        PropVisualForegroundLayerName,
        _propVisualForegrounds);
    LoadDebugMeasurementPair(map);
}

private void ApplyObjectState(LevelMap candidate)
{
    _platforms = candidate._platforms;
    _objectNames = candidate._objectNames;
    _initialCheckpointBounds = candidate._initialCheckpointBounds;
    _woodPlatforms = candidate._woodPlatforms;
    _dynamicPlatforms = candidate._dynamicPlatforms;
    _gelatins = candidate._gelatins;
    _trampolineGelatins = candidate._trampolineGelatins;
    _cutsceneTriggers = candidate._cutsceneTriggers;
    _sceneExits = candidate._sceneExits;
    _interventionEntrances = candidate._interventionEntrances;
    _interventionButtons = candidate._interventionButtons;
    _verticalRoutePlatforms = candidate._verticalRoutePlatforms;
    _safeRoutePlatforms = candidate._safeRoutePlatforms;
    _verticalRouteGelatins = candidate._verticalRouteGelatins;
    _damageVolumes = candidate._damageVolumes;
    _harmlessSpikeVolumes = candidate._harmlessSpikeVolumes;
    _verticalRouteDamageVolumes = candidate._verticalRouteDamageVolumes;
    _propVisualForegrounds = candidate._propVisualForegrounds;
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

    private void LoadDamageVolumesFromOptionalLayer(
        TmxMap map,
        string layerName,
        List<DamageVolume> damageVolumes)
    {
        var layer = map.ObjectGroups.FirstOrDefault(group => group.Name == layerName);

        if (layer == null)
        {
            return;
        }

        foreach (var row in layer.Objects)
        {
            damageVolumes.Add(new DamageVolume(
                new Rectangle(
                    (int)row.X,
                    (int)row.Y,
                    (int)row.Width,
                    (int)row.Height)));
        }
    }

    private void DrawDamageVolumes(
        SpriteBatch spriteBatch,
        IEnumerable<DamageVolume> damageVolumes,
        SolutionVisualModule visualModules,
        bool usesDistinctVisual)
    {
        const int spikeWidth = 16;
        const int spikeStepHeight = 4;

        foreach (DamageVolume damageVolume in damageVolumes)
        {
            Rectangle bounds = damageVolume.Bounds;
            Color spikeColor = usesDistinctVisual
                ? Color.MediumVioletRed
                : Color.OrangeRed;
            if (visualModules.HasFlag(SolutionVisualModule.DANGER_PLATFORM_HIGHLIGHT))
            {
                Rectangle dangerBand = new(
                    bounds.X,
                    bounds.Y - 8,
                    bounds.Width,
                    bounds.Height + 8);
                spriteBatch.Draw(_platformTexture, dangerBand, Color.DarkRed * 0.65f);
                spriteBatch.Draw(
                    _platformTexture,
                    new Rectangle(bounds.X, bounds.Y - 8, bounds.Width, 4),
                    Color.Yellow);

                for (int alertX = bounds.X + 24; alertX < bounds.Right; alertX += 48)
                {
                    spriteBatch.Draw(
                        _platformTexture,
                        new Rectangle(alertX, bounds.Y - 30, 5, 14),
                        Color.Yellow);
                    spriteBatch.Draw(
                        _platformTexture,
                        new Rectangle(alertX, bounds.Y - 12, 5, 5),
                        Color.Yellow);
                }
            }

            // Vertical hazard strips repeat sideways spikes along their entire height.
            if (TryGetObjectTexture(DamageLayerName, bounds, out Texture2D spikeTexture))
            {
                spriteBatch.Draw(spikeTexture, bounds, Color.White);
                continue;
            }

            if (bounds.Height > bounds.Width)
            {
                for (int spikeTop = bounds.Top; spikeTop < bounds.Bottom; spikeTop += spikeWidth)
                {
                    int spikeHeight = Math.Min(spikeWidth, bounds.Bottom - spikeTop);
                    for (int offset = 0; offset < bounds.Width; offset += spikeStepHeight)
                    {
                        int inset = (int)(offset / (float)bounds.Width * (spikeHeight - 1) / 2);
                        spriteBatch.Draw(_platformTexture,
                            new Rectangle(bounds.Left + offset, spikeTop + inset,
                                Math.Min(spikeStepHeight, bounds.Width - offset),
                                Math.Max(1, spikeHeight - inset * 2)), spikeColor);
                    }
                }
                continue;
            }

            for (int spikeLeft = bounds.Left;
                 spikeLeft < bounds.Right;
                 spikeLeft += spikeWidth)
            {
                int currentSpikeWidth = Math.Min(
                    spikeWidth,
                    bounds.Right - spikeLeft);
                int stepCount = Math.Min(
                    bounds.Height / spikeStepHeight,
                    currentSpikeWidth / (spikeStepHeight * 2));

                for (int step = 0; step < stepCount; step++)
                {
                    int inset = step * spikeStepHeight;
                    spriteBatch.Draw(
                        _platformTexture,
                        new Rectangle(
                            spikeLeft + inset,
                            bounds.Bottom - (step + 1) * spikeStepHeight,
                            currentSpikeWidth - inset * 2,
                            spikeStepHeight),
                        spikeColor);
                }
            }
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
        SolutionVisualModule visualModules,
        Color? colorOverride = null)
    {
        TryGetObjectTexture(GelatinLayerName, gelatin, out Texture2D gelatinTexture);
        gelatin = GetGelatinVisualBounds(gelatin, visualModules);

        Color gelatinColor = colorOverride ?? (
            (visualModules & SolutionVisualModule.GELATIN_COLOR) != 0
                ? Color.DeepSkyBlue
                : (visualModules & SolutionVisualModule.GELATIN_TEXTURE) != 0
                    ? Color.CornflowerBlue
                    : (visualModules & SolutionVisualModule.GELATIN_GLOW) != 0
                        ? Color.Cyan
                        : Color.Purple);

        if ((visualModules & SolutionVisualModule.GELATIN_GLOW) != 0)
        {
            float glowOpacity = 0.35f;

            if ((visualModules & SolutionVisualModule.GELATIN_GLOW_PULSE) != 0)
            {
                double pulse =
                    (Math.Sin(_visualEffectTime * 3.0) + 1.0) / 2.0;
                glowOpacity = (float)(0.30 + 0.50 * pulse);
            }

            var glow = new Rectangle(
                gelatin.X - 8,
                gelatin.Y - 8,
                gelatin.Width + 16,
                gelatin.Height + 16);
            spriteBatch.Draw(_platformTexture, glow, Color.Cyan * glowOpacity);
        }

        if (gelatinTexture != null)
        {
            Color tint = colorOverride ??
                (visualModules.HasFlag(SolutionVisualModule.GELATIN_COLOR) ? gelatinColor : Color.White);
            spriteBatch.Draw(gelatinTexture, gelatin, tint);
        }
        else
            spriteBatch.Draw(_platformTexture, gelatin, gelatinColor);

        if ((visualModules & SolutionVisualModule.GELATIN_DASH_SYMBOL) != 0)
        {
            DrawGelatinDashSymbol(spriteBatch, gelatin);
        }

        if ((visualModules & SolutionVisualModule.GELATIN_DIRECTIONAL_ANIMATION) != 0)
        {
            DrawGelatinEnergyChannels(spriteBatch, gelatin);
        }

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

    private void DrawGelatinDashSymbol(
        SpriteBatch spriteBatch,
        Rectangle gelatin)
    {
        const int stroke = 4;
        int centerX = gelatin.Center.X;
        int centerY = gelatin.Center.Y;
        Color symbolColor = Color.White * 0.85f;

        spriteBatch.Draw(
            _platformTexture,
            new Rectangle(centerX - 10, centerY - 2, 12, stroke),
            symbolColor);
        spriteBatch.Draw(
            _platformTexture,
            new Rectangle(centerX, centerY - 6, stroke, 12),
            symbolColor);
        spriteBatch.Draw(
            _platformTexture,
            new Rectangle(centerX + 4, centerY - 2, stroke, stroke),
            symbolColor);
    }

    private void DrawGelatinEnergyChannels(
        SpriteBatch spriteBatch,
        Rectangle gelatin)
    {
        const int channelRowSpacing = 10;
        const int segmentSpacing = 22;
        const int segmentWidth = 12;
        const int segmentHeight = 3;

        for (int y = gelatin.Top + 5;
             y < gelatin.Bottom - segmentHeight;
             y += channelRowSpacing)
        {
            for (int x = gelatin.Left + 6;
                 x < gelatin.Right - 4;
                 x += segmentSpacing)
            {
                double phase =
                    _visualEffectTime * 5.0 +
                    (x - gelatin.Left) * 0.18 +
                    (y - gelatin.Top) * 0.45;
                float pulse = (float)((Math.Sin(phase) + 1.0) * 0.5);
                Color channelColor = Color.Aquamarine * (0.25f + pulse * 0.60f);

                spriteBatch.Draw(
                    _platformTexture,
                    new Rectangle(x, y, segmentWidth, segmentHeight),
                    channelColor);
            }
        }
    }

    private Rectangle GetGelatinVisualBounds(
        Rectangle gelatin,
        SolutionVisualModule visualModules)
    {
        if ((visualModules & SolutionVisualModule.GELATIN_JELLY_MOTION) == 0)
        {
            return gelatin;
        }

        const double motionSpeed = 4.7;
        const double horizontalPhaseScale = 0.0125;
        const double verticalPhaseScale = 0.0175;

        double phase =
            _visualEffectTime * motionSpeed +
            gelatin.X * horizontalPhaseScale +
            gelatin.Y * verticalPhaseScale;
        double wave = Math.Sin(phase);
        int horizontalAmplitude = Math.Min(4, gelatin.Width / 8);
        int verticalAmplitude = Math.Min(3, gelatin.Height / 8);
        int horizontalOffset =
            (int)Math.Round(wave * horizontalAmplitude);
        int verticalOffset =
            (int)Math.Round(wave * verticalAmplitude);

        return new Rectangle(
            gelatin.X - horizontalOffset,
            gelatin.Y + verticalOffset,
            gelatin.Width + horizontalOffset * 2,
            gelatin.Height - verticalOffset);
    }

    private void DrawSharedPlatformDecoration(
        SpriteBatch spriteBatch,
        Rectangle platform,
        SolutionVisualModule visualModules)
    {
        if ((visualModules & SolutionVisualModule.PLATFORM_DECORATION) == 0)
        {
            return;
        }

        const int markerSize = 6;
        const int markerSpacing = 32;
        const int markerInset = 12;

        for (int y = platform.Top + markerInset;
             y + markerSize <= platform.Bottom;
             y += markerSpacing)
        {
            for (int x = platform.Left + markerInset;
                 x + markerSize <= platform.Right;
                 x += markerSpacing)
            {
                spriteBatch.Draw(
                    _platformTexture,
                    new Rectangle(x, y, markerSize, markerSize),
                    Color.White * 0.45f);
            }
        }
    }

    private void DrawSharedPlatformOutline(
        SpriteBatch spriteBatch,
        Rectangle platform,
        SolutionVisualModule visualModules)
    {
        if ((visualModules & SolutionVisualModule.PLATFORM_OUTLINE) == 0)
        {
            return;
        }

        int thickness = Math.Min(
            2,
            Math.Min(platform.Width, platform.Height));

        if (thickness <= 0)
        {
            return;
        }

        Color outlineColor = Color.White * 0.65f;

        spriteBatch.Draw(
            _platformTexture,
            new Rectangle(
                platform.Left,
                platform.Top,
                platform.Width,
                thickness),
            outlineColor);
        spriteBatch.Draw(
            _platformTexture,
            new Rectangle(
                platform.Left,
                platform.Bottom - thickness,
                platform.Width,
                thickness),
            outlineColor);
        spriteBatch.Draw(
            _platformTexture,
            new Rectangle(
                platform.Left,
                platform.Top,
                thickness,
                platform.Height),
            outlineColor);
        spriteBatch.Draw(
            _platformTexture,
            new Rectangle(
                platform.Right - thickness,
                platform.Top,
                thickness,
                platform.Height),
            outlineColor);
    }

    private void DrawPathIndicators(SpriteBatch spriteBatch)
    {
        bool hasVerticalRoute =
            _verticalRoutePlatforms.Count > 0 ||
            _verticalRouteGelatins.Count > 0;

        if (hasVerticalRoute)
        {
            foreach (Rectangle platform in _verticalRoutePlatforms)
            {
                DrawPathIndicator(spriteBatch, platform);
            }

            foreach (Rectangle gelatin in _verticalRouteGelatins)
            {
                DrawPathIndicator(spriteBatch, gelatin);
            }

            return;
        }

        foreach (Rectangle platform in _platforms)
        {
            DrawPathIndicator(spriteBatch, platform);
        }

        foreach (Rectangle gelatin in _gelatins)
        {
            DrawPathIndicator(spriteBatch, gelatin);
        }

        foreach (Rectangle trampoline in _trampolineGelatins)
        {
            DrawPathIndicator(spriteBatch, trampoline);
        }
    }

    private void DrawPathIndicator(
        SpriteBatch spriteBatch,
        Rectangle surface)
    {
        var marker = new Rectangle(
            surface.Center.X - 24,
            surface.Top - 20,
            48,
            8);
        spriteBatch.Draw(_platformTexture, marker, Color.Gold);
    }

    private void DrawDestinationHighlights(SpriteBatch spriteBatch)
    {
        const int thickness = 4;

        foreach (SceneExit sceneExit in _sceneExits)
        {
            Rectangle bounds = sceneExit.Bounds;
            spriteBatch.Draw(
                _platformTexture,
                new Rectangle(bounds.Left, bounds.Top, bounds.Width, thickness),
                Color.Gold);
            spriteBatch.Draw(
                _platformTexture,
                new Rectangle(bounds.Left, bounds.Bottom - thickness, bounds.Width, thickness),
                Color.Gold);
            spriteBatch.Draw(
                _platformTexture,
                new Rectangle(bounds.Left, bounds.Top, thickness, bounds.Height),
                Color.Gold);
            spriteBatch.Draw(
                _platformTexture,
                new Rectangle(bounds.Right - thickness, bounds.Top, thickness, bounds.Height),
                Color.Gold);
        }
    }
}
