using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DontLikePoetry;

public class Scene
{
    private const int PlayerWidth = 16;
    private const int PlayerHeight = 16;

    private const double PlayerGravity = GameScaleConfig.PlayerGravity;
    private const double SecondsPerFramePlaying = 1.0 / 60.0;
    private const double SecondsPerFrameCutscene = 1.0 / 5.0;

    private const int ScreenWidth = 1920;
    private const int ScreenHeight = 1080;
    private const int FallLimitY = ScreenHeight + 128;

    private const float FadeStep = 0.2f;
    private const float CutsceneCameraZoom = 2.0f;
    private const float CutsceneWalkSpeed = 10.0f;
    private const float CutsceneEndX = 940.0f;
    private const float GelatinMaximumFallSpeed = GameScaleConfig.PlayerGelatinMaximumFallSpeed;

    private Texture2D _fadeTexture;
    private SpriteFont _interactionFont;
    private Texture2D _passageSymbols;
    private double _passageTime;
    private bool IsInterventionRoom => UsesRoomFormat &&
        _phaseState.CurrentScenarioId == _phaseState.Definition.InterventionScenarioId;

    private Rectangle PassageInteractionBounds
    {
        get
        {
            Rectangle reach = _player.HitBox;
            reach.Inflate(64, 24);
            return reach;
        }
    }

    public bool HasVisibleInterventionPassage => UsesRoomFormat &&
        _map.InterventionEntrances.Any(e => e.Placement == GetActiveInterventionEntrancePlacement());
    private bool _interactPressed;
    private float _respawnEffectTime;
    // Room format (shared by every phase built like Affordance): rooms, intervention room,
    // barrier, lights, fixed camera and respawn. Content rules stay under IsAffordance.
    private bool UsesRoomFormat => _phaseState?.Definition.UsesRoomFormat == true;
    private bool IsAffordance => _phaseState?.PhaseId == ScenarioCatalog.AffordanceVisibilityScenarioId;
    private int CurrentRoomIndex =>
        _phaseState?.Definition.GetRoomIndex(_phaseState.CurrentScenarioId) ?? -1;

    public void SetInteractionPressed(bool pressed) => _interactPressed = pressed;
    public void AlignCamera(Camera camera) => UpdatePlayingCamera(camera);
    private double _damageParticlesTimer;
    private double _damageImpactMarkTimer;
    private Vector2 _damageContactPoint;
    // Phase 2 solution effects. The heart is a placeholder until the 3-frame art arrives.
    private double _heartCrackTimer;
    private double _screenShakeTimer;
    private static Texture2D _heartTexture;
    // Mort balloon (Feedback): the player freezes and the camera zooms in until it ends.
    private const double CalloutSeconds = 2.8;
    private const double CalloutEaseSeconds = 0.35;
    private const float CalloutZoom = 1.8f;
    private double _calloutTimer;
    private string _calloutText;
    private bool _room2CalloutChecked;
    private static readonly Color HeartCrackColor = new Color(214, 52, 62);
    private static readonly Color DamageBorderColor = new Color(190, 20, 30);
    private Rectangle _fadeRectangle;
    private float _fadeAlpha;

    private Player _player;
    private readonly LevelMap _map;
    private IEnumerable<Rectangle> _additionalSolidColliders = Enumerable.Empty<Rectangle>();
    private bool _useAdditionalCollidersOnly;
    private IEnumerable<Rectangle> _additionalGelatinVolumes = Enumerable.Empty<Rectangle>();
    private readonly ScenarioCameraBehavior _cameraBehavior;
    private readonly float? _playerScreenHeightRatio;
    private readonly bool _supportsPlayerScalePreview;
    public bool UseOriginalScaleTestCamera { get; set; }
    private readonly PropVisualPresentation _propVisualPresentation;
    private readonly ConceptualRuleReminderPresentation
        _conceptualRuleReminderPresentation;
private readonly PlayerCollision _playerCollision = new PlayerCollision();
private readonly HashSet<Rectangle> _usedGelatins = new HashSet<Rectangle>();
private bool _playerWasTouchingSolidSurface;
private bool _playerWasTouchingGelatinSurface;
private Vector2 _checkpoint;
    private bool _playerIsDead;
    private Vector2 _playerStart;
    private readonly Vector2 _fallbackPlayerStart;
    private readonly string _entrySpawnId;
    private readonly PhaseState _phaseState;
    private string _requestedScenarioId;
    private string _requestedSpawnId;
    private readonly InterventionTrigger _interventionTrigger = new();
    private readonly ExitDoor _exitDoor = new();

    public SceneMode SceneMode { get; private set; }
    public double SecondsPerFrame { get; private set; }
    public bool PlayerIsDead
    {
        get
        {
            return _playerIsDead;
        }
    }

    public bool InterventionButtonPressed => _interventionTrigger.IsPressed;

    public Scene()
        : this(ScenarioCatalog.Initial, null, null)
    {
    }

    public Scene(ScenarioDefinition scenario)
        : this(scenario, null, null)
    {
    }

    public Scene(ScenarioDefinition scenario, string entrySpawnId)
        : this(scenario, entrySpawnId, null)
    {
    }

    public Scene(
        ScenarioDefinition scenario,
        string entrySpawnId,
        PhaseState phaseState)
        : this(
            scenario.MapPath,
            scenario.FallbackPlayerStart,
            entrySpawnId,
            phaseState,
            scenario.Id == ScenarioCatalog.PropVisualScenarioId,
            scenario.LdtkLevelId,
            scenario.CameraBehavior,
            scenario.PlayerScreenHeightRatio,
            scenario.SupportsPlayerScalePreview,
            LdtkRoomLinks.For(scenario.Id))
    {
        _isTutorial = scenario.Id == ScenarioCatalog.TutorialScenarioId;
    }

    // Tutorial room: flat background, control signs, falls only send the player back.
    private readonly bool _isTutorial;
    private bool _tutorialExitReached;
    private Rectangle _tutorialExitBounds;
    private bool _controlsSignActivated;
    // FalaMort areas already spoken in this session (LDtk entity iids).
    private static readonly HashSet<string> ShownMortCallouts = new();
    private static readonly Color TutorialBackgroundColor = new Color(0x15, 0x19, 0x1F);
    private static readonly Color TutorialSignColor = new Color(0xD8, 0xD0, 0xBB);

    public Scene(string mapPath, Vector2 playerStart)
        : this(mapPath, playerStart, null, null, false)
    {
    }

    private Scene(
        string mapPath,
        Vector2 playerStart,
        string entrySpawnId,
        PhaseState phaseState,
        bool usesPropVisualPresentation,
        string ldtkLevelId = null,
        ScenarioCameraBehavior cameraBehavior = ScenarioCameraBehavior.FOLLOW_PLAYER,
        float? playerScreenHeightRatio = null,
        bool supportsPlayerScalePreview = false,
        LdtkRoomLinks roomLinks = null)
    {
        _map = new LevelMap(mapPath, ldtkLevelId, roomLinks);
        _cameraBehavior = cameraBehavior;
        _playerScreenHeightRatio = playerScreenHeightRatio;
        _supportsPlayerScalePreview = supportsPlayerScalePreview;
        _propVisualPresentation = usesPropVisualPresentation
            ? new PropVisualPresentation()
            : null;
        _conceptualRuleReminderPresentation =
            phaseState?.PhaseId == ScenarioCatalog.ConceptualModelsConstraintsScenarioId
                ? new ConceptualRuleReminderPresentation()
                : null;
        _fallbackPlayerStart = playerStart;
        _entrySpawnId = entrySpawnId;
        _phaseState = phaseState;
    }

    public Player Player
    {
        get
        {
            return _player;
        }
    }

    public void SetAdditionalSolidColliders(IEnumerable<Rectangle> colliders)
    {
        _additionalSolidColliders = colliders ?? Enumerable.Empty<Rectangle>();
    }

    public void SetAdditionalGelatinVolumes(IEnumerable<Rectangle> volumes) =>
        _additionalGelatinVolumes = volumes ?? Enumerable.Empty<Rectangle>();

    private IEnumerable<Rectangle> GetGelatinVolumes(bool verticalRouteRevealed)
    {
        if (_useAdditionalCollidersOnly) return _additionalGelatinVolumes;
        return _map.GetGelatinVolumes(verticalRouteRevealed)
            .Concat(_additionalGelatinVolumes);
    }

    // Phase 3: Restaurar makes the impostors act like the known gelatin and hot rock.
    private bool ImpostorsRestored =>
        GetAppliedVisualModules().HasFlag(SolutionVisualModule.IMPOSTOR_RESTORE);

    public void UseAdditionalCollidersOnly() => _useAdditionalCollidersOnly = true;

    public void ReleaseLdtkTextures() => _map.ReleaseLdtkTextures();

    internal void UseLdtkTextureCache(LdtkTextureCache cache) => _map.UseLdtkTextureCache(cache);

    public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
    {
        using var loading = new LoadDiagnostics("Scene.LoadContent");
        _fadeAlpha = 0.0f;
        _playerIsDead = false;
        _requestedScenarioId = null;
        _requestedSpawnId = null;
        _interventionTrigger.Reset();
        SyncInterventionState();
        _usedGelatins.Clear();
        _playerWasTouchingSolidSurface = false;
        _playerWasTouchingGelatinSurface = false;
        _fadeRectangle = new Rectangle(0, 0, ScreenWidth, ScreenHeight);
        loading.Mark("preparacao");
        _map.LoadContent(graphicsDevice);
        loading.Mark("mapa completo", includeInSummary: true);
        // LDtk draws terrain from its tilesets, not these legacy object textures.
        if (SupportsCameraScalePreview && !_map.IsLdtk)
        {
            _map.SetWoodPlatformTexture(LoadDiagnostics.LoadTexture(content, "visual/platformer_wood"));
            _map.SetInitialCheckpointTexture(LoadDiagnostics.LoadTexture(content, "visual/ini_ckp"));
            _map.SetObjectTexture("parede_esquerda", LoadDiagnostics.LoadTexture(content, "visual/parede_esquerda"));
            _map.SetObjectTexture("plat_solida1", LoadDiagnostics.LoadTexture(content, "visual/plat_solid_1"));
            _map.SetObjectTexture("recarga_2", LoadDiagnostics.LoadTexture(content, "visual/recarga2"));
            _map.SetObjectTexture("espinhos1", LoadDiagnostics.LoadTexture(content, "visual/espinhos1"));
            _map.SetObjectTexture("teto", LoadDiagnostics.LoadTexture(content, "visual/teto"));
            _map.SetObjectTexture("plat_solida3", LoadDiagnostics.LoadTexture(content, "visual/plat_solid3"));
            _map.SetObjectTexture("recarga_1", LoadDiagnostics.LoadTexture(content, "visual/recarga1"));
            _map.SetObjectTexture("wood1", LoadDiagnostics.LoadTexture(content, "visual/wood1"));
            _map.SetObjectTexture("plat_solida2", LoadDiagnostics.LoadTexture(content, "visual/plat_solid_2"));
            _map.SetObjectTexture("wood4", LoadDiagnostics.LoadTexture(content, "visual/wood4"));
            _map.SetObjectTexture("recarga_3", LoadDiagnostics.LoadTexture(content, "visual/recarga3"));
            _map.SetObjectTexture("recarga_4", LoadDiagnostics.LoadTexture(content, "visual/recarga4"));
            Texture2D rightPlatform = LoadDiagnostics.LoadTexture(content, "visual/plat_direita");
            _map.SetObjectTexture("recarga_5", rightPlatform);
            _map.SetObjectTexture("recarga_6", rightPlatform);
            _map.SetObjectTexture("wood3", rightPlatform);
            _map.SetObjectTexture("wood2", rightPlatform);
            _map.SetObjectTexture("plat_solida4", LoadDiagnostics.LoadTexture(content, "visual/plat_solid4"));
            _map.SetObjectTexture("parede_direita", LoadDiagnostics.LoadTexture(content, "visual/parede_direita"));
            _map.SetObjectTexture("espinhos_lateral", LoadDiagnostics.LoadTexture(content, "visual/espinho_lateral"));
        }
        loading.Mark("texturas de plataformas legadas", includeInSummary: true);
        if (UsesRoomFormat || _isTutorial)
        {
            _interactionFont = content.Load<SpriteFont>("patrick");
            _passageSymbols = LoadDiagnostics.LoadTexture(content, "ui/icons/icons");
            _heartTexture ??= CreateRedHeart(graphicsDevice,
                LoadDiagnostics.LoadTexture(content, "ui/icons/coracao"));
            _interventionTrigger.SetSwordTexture(LoadDiagnostics.LoadTexture(content, "visual/intervention_sword"));
        }
        _propVisualPresentation?.LoadContent(content);
        _conceptualRuleReminderPresentation?.LoadContent(graphicsDevice, content);
        ConfigureExitDoor();
        _playerStart = _map.GetPlayerSpawn(_entrySpawnId)
            ?? _map.PlayerSpawn
            ?? _fallbackPlayerStart;
        _checkpoint = _playerStart;

        loading.Mark("interacao, apresentacoes e spawn");
        // Every use stretches a solid color into a destination rectangle.
        _fadeTexture = new Texture2D(graphicsDevice, 1, 1);
        _fadeTexture.SetData(new[] { Color.White });
        loading.Mark("textura branca", includeInSummary: true);
        loading.Texture(_fadeTexture);

        int colliderWidth = SupportsCameraScalePreview
            ? GameScaleConfig.PlayerReferenceColliderWidth : PlayerWidth;
        int colliderHeight = SupportsCameraScalePreview
            ? GameScaleConfig.PlayerReferenceColliderHeight : PlayerHeight;
        _player = new Player(
            (int)_playerStart.X + (PlayerWidth - colliderWidth) / 2,
            (int)_playerStart.Y + PlayerHeight - colliderHeight,
            PlayerGravity, colliderWidth, colliderHeight);
        _player.LoadContent(graphicsDevice, PlayerWidth, PlayerHeight);
        loading.Mark("criacao do player");
        // 8 * 0.75 player scale * 0.675 camera zoom = 4.05 screen pixels.
        _player.ColliderOffsetFromSpriteX = SupportsCameraScalePreview ? 8f : 0f;
        if (SupportsCameraScalePreview)
        {
            Texture2D rightSprite = LoadDiagnostics.LoadTexture(content, "player/timothy_mort_2_right");
            _player.SetDirectionalPreviewSprites(
                rightSprite,
                LoadDiagnostics.LoadTexture(content, "player/timothy_mort_2_left"),
                GameScaleConfig.PlayerSpriteReferenceHeight / rightSprite.Height);
            var walkFrames = new Texture2D[7];
            for (int index = 0; index < walkFrames.Length; index++)
                walkFrames[index] = LoadDiagnostics.LoadTexture(content, $"player/walk/walk{index + 1:D4}");
            _player.SetWalkAnimation(walkFrames);
            var jumpFrames = new Texture2D[4];
            for (int index = 0; index < jumpFrames.Length; index++)
                jumpFrames[index] = LoadDiagnostics.LoadTexture(content, $"player/jump/jump{index:D4}");
            var glideFrames = new Texture2D[5];
            for (int index = 0; index < glideFrames.Length; index++)
                glideFrames[index] = LoadDiagnostics.LoadTexture(content, $"player/glide/glide{index:D4}");
            var glideDashUsedFrames = new Texture2D[4];
            for (int index = 0; index < glideDashUsedFrames.Length; index++)
                glideDashUsedFrames[index] = LoadDiagnostics.LoadTexture(content,
                    $"player/dash/glide_dash_used{index:D4}");
            _player.SetAirAnimations(jumpFrames, glideFrames, glideDashUsedFrames);
            var upDashFrames = new Texture2D[2];
            var sideDashFrames = new Texture2D[2];
            for (int index = 0; index < upDashFrames.Length; index++)
            {
                upDashFrames[index] = LoadDiagnostics.LoadTexture(content, $"player/dash/dash{index:D4}");
                sideDashFrames[index] = LoadDiagnostics.LoadTexture(content, $"player/dash/dash_vert{index:D4}");
            }
            _player.SetDashAnimations(upDashFrames, sideDashFrames);
            _player.SetLookAnimations(
                new[]
                {
                    LoadDiagnostics.LoadTexture(content, "player/look/look_top0000"),
                    LoadDiagnostics.LoadTexture(content, "player/look/look_top0001")
                },
                LoadDiagnostics.LoadTexture(content, "player/look/loop_bottom0000"));
        }

        loading.Mark("sprites e animacoes do player", includeInSummary: true);
        if (SupportsCameraScalePreview)
            TrySetPlayerScale(_map.IsLdtk ? 1 : GameScaleConfig.DefaultPlayerScalePreset);

        SceneMode = SceneMode.PLAYING;
        SecondsPerFrame = SecondsPerFramePlaying;
        loading.Mark("escala e finalizacao");
    }

    public void Update(Camera camera, double deltaTime)
    {
        _passageTime += deltaTime;
        if (SceneMode == SceneMode.PLAYING)
        {
            UpdatePlaying(camera, deltaTime);
        }
        else if (SceneMode == SceneMode.FADE_OUT)
        {
            UpdateFadeOut(camera);
        }
        else if (SceneMode == SceneMode.FADE_IN)
        {
            UpdateFadeIn();
        }
        else if (SceneMode == SceneMode.CUTSCENE)
        {
            UpdateCutscene(camera, deltaTime);
        }
    }

    public void Restart(Camera camera)
    {
        _requestedScenarioId = null;
        RespawnPlayer(camera);
    }

public bool TryReloadMap(Camera camera, out string errorMessage)
{
    if (!_map.TryReloadObjects(out errorMessage))
    {
        return false;
    }

    _playerStart = _map.GetPlayerSpawn(_entrySpawnId)
        ?? _map.PlayerSpawn
        ?? _fallbackPlayerStart;
        _checkpoint = _playerStart;

        _requestedScenarioId = null;
        _requestedSpawnId = null;
        _interventionTrigger.Reset();
        SyncInterventionState();
        ConfigureExitDoor();

    RespawnPlayer(camera);
    return true;
}

    public bool TryTakeScenarioTransition(out string scenarioId, out string spawnId)
    {
        scenarioId = _requestedScenarioId;
        spawnId = _requestedSpawnId;

        if (string.IsNullOrEmpty(scenarioId))
        {
            return false;
        }

        _requestedScenarioId = null;
        _requestedSpawnId = null;
        return true;
    }

    public bool TryTakeInterventionButtonActivation() => _interventionTrigger.TryTakeActivation();

    public void ResyncPlayerInput() => _player.ResyncInput();

    // Room 2's way to room 3 stays blocked until a first solution is chosen in the intervention:
    // entering it sends the player back to the room's start, and it shows a dark light.
    private IEnumerable<SceneExit> GetRoom3Barriers()
    {
        if (!UsesRoomFormat || _phaseState.HasFirstChoice || CurrentRoomIndex != 1)
            return Enumerable.Empty<SceneExit>();
        string lastRoom = _phaseState.Definition.LastRoomScenarioId;
        return _map.SceneExits.Where(exit => exit.DestinationScenarioId == lastRoom);
    }

    // Open ways forward in the challenge rooms, marked with a light.
    // RoomBack exits lead to "retorno_tela" and stay unmarked.
    private IEnumerable<SceneExit> GetLitExits()
    {
        if (!(UsesRoomFormat || _isTutorial) || IsInterventionRoom) return Enumerable.Empty<SceneExit>();
        var barriers = GetRoom3Barriers().ToList();
        return _map.SceneExits.Where(exit =>
            exit.DestinationSpawnId != "retorno_tela" && !barriers.Contains(exit) &&
            (!exit.RequiresAppliedSolution || _phaseState?.ExitDoorIsOpen == true));
    }

private void UpdatePlaying(Camera camera, double deltaTime)
{
    _map.UpdateVisualEffects(deltaTime);
    TryStartRoom2Callout();
    TryStartAreaCallout();
    if (UpdateMortCallout(camera, deltaTime)) return;
    _respawnEffectTime = Math.Max(0, _respawnEffectTime - (float)deltaTime);
    _map.UpdateDynamicPlatforms(_player, deltaTime);
    _phaseState?.UpdateDamageInvulnerability(deltaTime);
        _player.Update(deltaTime);
        ApplyGelatinFallSpeed();
        bool dashWasAvailableBeforePhysics = _player.CanDash;
        UpdatePlayerPhysics(deltaTime);
        _player.UpdateAnimation(deltaTime);
        ApplyGelatinFallSpeed();
        _damageParticlesTimer = Math.Max(0, _damageParticlesTimer - deltaTime);
        _damageImpactMarkTimer = Math.Max(0, _damageImpactMarkTimer - deltaTime);
        _heartCrackTimer = Math.Max(0, _heartCrackTimer - deltaTime);
        _screenShakeTimer = Math.Max(0, _screenShakeTimer - deltaTime);
        UpdateDamage(camera);

        if (_playerIsDead)
        {
            return;
        }

        if (!dashWasAvailableBeforePhysics && _player.CanDash &&
            _phaseState?.AppliedSolutionUsesModule(
                SolutionVisualModule.PLAYER_DASH_RECHARGE_SYMBOL) == true)
        {
            _player.StartDashRechargeSymbol();
        }

        if (!dashWasAvailableBeforePhysics && _player.CanDash &&
            _phaseState?.AppliedSolutionUsesModule(
                SolutionVisualModule.PLAYER_DASH_ENERGY_ABSORPTION) == true)
        {
            _player.StartDashEnergyAbsorption(new Vector2(
                _player.HitBox.Center.X,
                _player.HitBox.Bottom + 8));
        }

        _interventionTrigger.Update(
            _map.InterventionButtons, _player.HitBox, _player.IsGrounded,
            UsesRoomFormat, _interactPressed, deltaTime);
        _exitDoor.Update(
            InterventionButtonPressed || _phaseState?.VerticalRouteRevealed == true,
            deltaTime);

        bool playerIsTouchingGelatin = PlayerTouchesAnySurface(
            GetGelatinVolumes(
                _phaseState?.VerticalRouteRevealed == true));

        if (_player.IsGrounded && !playerIsTouchingGelatin)
        {
            _usedGelatins.Clear();
        }

        // Affordance room 1: Mort speaks on the first touch of any gelatin, before the choice.
        if (playerIsTouchingGelatin && IsAffordance && CurrentRoomIndex == 0 &&
            !_phaseState.HasFirstChoice && _phaseState.TryTakeGelatinCallout())
            StartMortCallout(MortDialogueCatalog.AffordanceFirstGelatinCallout);

    UpdateGelatinContact();
    UpdateSurfaceContactFeedback();
    // Tutorial: the last ground touched is the checkpoint, so a fall only repeats that stretch.
    if (_isTutorial && _player.IsGrounded)
        _checkpoint = new Vector2(
            _player.HitBox.X - (PlayerWidth - _player.HitBox.Width) / 2f,
            _player.HitBox.Bottom - PlayerHeight);

    if (PlayerFell())
        {
            if (UsesRoomFormat) RespawnInRoom(camera, true);
            else if (_isTutorial)
            {
                RespawnPlayer(camera);
                _respawnEffectTime = 0.22f;
            }
            else KillPlayer();
            return;
        }

        UpdatePlayingCamera(camera);

        SceneExit sceneExit = _map.GetSceneExit(
            IsInterventionRoom ? PassageInteractionBounds : _player.HitBox,
            _exitDoor.IsOpen,
            _phaseState?.ExitDoorIsOpen == true);
        if (_isTutorial && _interactPressed &&
            _map.ControlSigns.Any(sign => sign.Intersects(PassageInteractionBounds)))
            _controlsSignActivated = true;
        // Tutorial exit: PhaseRun asks for confirmation before leaving.
        if (sceneExit != null && _isTutorial)
        {
            _tutorialExitReached = true;
            _tutorialExitBounds = sceneExit.Bounds;
            return;
        }
        if (sceneExit != null && GetRoom3Barriers().Contains(sceneExit))
        {
            // Blocked passage: back to the room's start (no heart lost) and Mort explains.
            RespawnPlayer(camera);
            _respawnEffectTime = 0.22f;
            StartMortCallout(MortDialogueCatalog.Room3Barrier);
            return;
        }
        if (sceneExit != null && (!IsInterventionRoom || _interactPressed))
        {
            // Leaving the intervention with a solution makes the choice final.
            if (IsInterventionRoom) _phaseState.LockChoice();
            _requestedScenarioId = sceneExit.DestinationScenarioId;
            _requestedSpawnId = sceneExit.DestinationSpawnId;
            ResolveInterventionReturn();
            return;
        }

        InterventionEntrance interventionEntrance = _map.GetInterventionEntrance(
            UsesRoomFormat ? PassageInteractionBounds : _player.HitBox,
            GetActiveInterventionEntrancePlacement());

        if (interventionEntrance != null && (!UsesRoomFormat || _interactPressed))
        {
            _requestedScenarioId = interventionEntrance.DestinationScenarioId;
            _requestedSpawnId = interventionEntrance.DestinationSpawnId;
            ResolveInterventionReturn();
            return;
        }

        if (PlayerTouchedCutsceneTrigger())
        {
            StartFadeOut();
        }
    }

    private void UpdatePlayerPhysics(double deltaTime)
    {
        _playerCollision.MoveAndResolve(
            _player,
            GetSolidColliders(),
            deltaTime);
    }

    private IEnumerable<Rectangle> GetSolidColliders()
    {
        if (_useAdditionalCollidersOnly)
            return _additionalSolidColliders;

        return _map.GetSolidColliders(
                _phaseState?.VerticalRouteRevealed == true,
                GetAppliedVisualModules().HasFlag(SolutionVisualModule.SAFE_ROUTE))
            .Concat(_exitDoor.Collider.HasValue
                ? new[] { _exitDoor.Collider.Value }
                : Enumerable.Empty<Rectangle>())
            .Concat(_additionalSolidColliders);
    }

    private bool UpdateDamage(Camera camera)
    {
        if (_phaseState?.HasDamageSystem != true)
        {
            return false;
        }

        if (TryApplyHotSurfaceDamage(camera))
        {
            return true;
        }

        SolutionVisualModule visualModules = GetAppliedVisualModules();
        DamageVolume damageVolume = _map.GetDamageVolume(
            _player.HitBox,
            _phaseState.VerticalRouteRevealed);
        bool hitHarmlessSpike = false;

        if (damageVolume == null &&
            (visualModules.HasFlag(
                SolutionVisualModule.RESTORE_HARMLESS_SPIKE_DAMAGE) ||
             visualModules.HasFlag(
                SolutionVisualModule.HARMLESS_SPIKE_DOUBLE_DAMAGE)))
        {
            damageVolume = _map.GetHarmlessSpikeVolume(_player.HitBox);
            hitHarmlessSpike = damageVolume != null;
        }

        // Affordance spikes work like falling: lose a heart and respawn, even while invulnerable.
        if (damageVolume != null && IsAffordance)
        {
            RespawnInRoom(camera, true);
            return true;
        }

        int damageAmount = hitHarmlessSpike &&
            visualModules.HasFlag(
                SolutionVisualModule.HARMLESS_SPIKE_DOUBLE_DAMAGE)
            ? 2
            : 1;

        if (damageVolume == null ||
            !_phaseState.TryApplyDamage(damageAmount))
        {
            return false;
        }

        ApplyDamageFeedback(damageVolume);

        if (_phaseState.CurrentHealth <= 0)
        {
            HandleHeartsDepleted(camera);
        }
        return true;
    }

    // Hot rocks (phase 2): touching one (floor, wall or ceiling) costs a heart; the phase's
    // hidden invulnerability spaces the hits to one per second. No sign unless a solution adds it.
    private bool TryApplyHotSurfaceDamage(Camera camera)
    {
        Rectangle playerBounds = _player.HitBox;
        Rectangle? touchedSurface = null;
        foreach (Rectangle surface in _map.HotSurfaces)
        {
            if (RectanglesTouchOrIntersect(playerBounds, surface))
            {
                touchedSurface = surface;
                break;
            }
        }

        // Phase 3 impostor rock costs two hearts; Restaurar brings it back to one.
        bool impostorHit = touchedSurface != null && _map.IsImpostorRock(touchedSurface.Value) &&
                           !ImpostorsRestored;
        if (touchedSurface == null || !_phaseState.TryApplyDamage(impostorHit ? 2 : 1))
        {
            return false;
        }

        // The heavier hit always gets the stronger signal: heart crack and screen shake.
        if (impostorHit)
        {
            _heartCrackTimer = DamageFeedbackSettings.HeartCrackSeconds;
            _screenShakeTimer = DamageFeedbackSettings.ScreenShakeSeconds;
        }

        if (IsFeedback && !IsInterventionRoom && _phaseState.TryTakeDamageCallout())
            StartMortCallout(MortDialogueCatalog.FeedbackFirstDamageCallout);

        SolutionVisualModule visualModules = GetAppliedVisualModules();
        if (visualModules.HasFlag(SolutionVisualModule.DAMAGE_KNOCKBACK))
            _player.ApplySurfaceKnockback(GetAwayFromSurface(playerBounds, touchedSurface.Value, _player.FacingDirection));
        if (visualModules.HasFlag(SolutionVisualModule.DAMAGE_HEART_CRACK))
            _heartCrackTimer = DamageFeedbackSettings.HeartCrackSeconds;
        if (visualModules.HasFlag(SolutionVisualModule.DAMAGE_SCREEN_SHAKE))
            _screenShakeTimer = DamageFeedbackSettings.ScreenShakeSeconds;

        if (_phaseState.CurrentHealth <= 0)
        {
            HandleHeartsDepleted(camera);
        }
        return true;
    }

    public bool TryTakeTutorialExit()
    {
        if (!_tutorialExitReached) return false;
        _tutorialExitReached = false;
        return true;
    }

    public bool TryTakeControlsSignActivation()
    {
        if (!_controlsSignActivated) return false;
        _controlsSignActivated = false;
        return true;
    }

    // "Testar mais": move the player back out of the exit so it does not trigger again at once.
    public void StepBackFromTutorialExit()
    {
        bool exitOnRight = _tutorialExitBounds.Center.X > _map.WidthInPixels / 2;
        int x = exitOnRight
            ? _tutorialExitBounds.Left - _player.HitBox.Width - 96
            : _tutorialExitBounds.Right + 96;
        _player.Move(x, _player.HitBox.Y);
        _player.ResetVelocity();
        _tutorialExitReached = false;
    }

    // FalaMort: entering the area makes Mort speak once per session.
    private void TryStartAreaCallout()
    {
        if (_calloutTimer > 0) return;
        foreach (var (bounds, text, id) in _map.MortCallouts)
        {
            if (ShownMortCallouts.Contains(id) || !_player.HitBox.Intersects(bounds)) continue;
            ShownMortCallouts.Add(id);
            StartMortCallout(text);
            return;
        }
    }

    private bool IsFeedback => _phaseState?.PhaseId == ScenarioCatalog.DamageFeedbackScenarioId;

    // Reaching room 2 without a hit: Mort points to the intervention instead.
    private void TryStartRoom2Callout()
    {
        if (_room2CalloutChecked) return;
        _room2CalloutChecked = true;
        if (IsFeedback && CurrentRoomIndex == 1 && !_phaseState.HasFirstChoice &&
            _phaseState.TryTakeDamageCallout())
            StartMortCallout(MortDialogueCatalog.FeedbackRoom2Callout);
    }

    private void StartMortCallout(string text)
    {
        _calloutText = text;
        _calloutTimer = CalloutSeconds;
    }

    // While the balloon shows, the game stands still and the camera eases in on the player.
    private bool UpdateMortCallout(Camera camera, double deltaTime)
    {
        if (_calloutTimer <= 0) return false;
        _calloutTimer = Math.Max(0, _calloutTimer - deltaTime);
        UpdatePlayingCamera(camera);
        float ease = CalloutEase;
        camera.Zoom *= 1f + (CalloutZoom - 1f) * ease;
        camera.LookAt(Vector2.Lerp(camera.Position, _player.HitBox.Center.ToVector2(), ease));
        return true;
    }

    private float CalloutEase
    {
        get
        {
            double edge = Math.Min(CalloutSeconds - _calloutTimer, _calloutTimer);
            float t = (float)Math.Clamp(edge / CalloutEaseSeconds, 0, 1);
            return t * t * (3f - 2f * t);
        }
    }

    // Balloon in screen space above the player's head, in the style of the "E" prompt.
    private void DrawMortCallout(SpriteBatch spriteBatch, Camera camera)
    {
        if (_calloutTimer <= 0 || string.IsNullOrEmpty(_calloutText) || _interactionFont == null) return;
        float alpha = Math.Min(1f, CalloutEase * 1.5f);
        Vector2 head = Vector2.Transform(
            new Vector2(_player.HitBox.Center.X, _player.HitBox.Top), camera.GetTransform());
        Vector2 size = _interactionFont.MeasureString(_calloutText);
        const int padding = 18, tail = 14, outline = 3;
        var plate = new Rectangle(0, 0, (int)size.X + padding * 2, (int)size.Y + padding);
        plate.X = (int)Math.Clamp(head.X - plate.Width / 2f, 16, _fadeRectangle.Width - plate.Width - 16);
        plate.Y = (int)Math.Max(16, head.Y - plate.Height - tail - 24);
        Color cream = new Color(255, 237, 182) * alpha;
        Color dark = new Color(17, 20, 26) * alpha;
        spriteBatch.Begin();
        spriteBatch.Draw(_fadeTexture, plate, cream);
        Rectangle inner = plate;
        inner.Inflate(-outline, -outline);
        spriteBatch.Draw(_fadeTexture, inner, dark);
        // Tail pointing at the player's head.
        int tailX = (int)Math.Clamp(head.X, plate.Left + 20, plate.Right - 20);
        for (int row = 0; row < tail; row++)
        {
            int half = tail - row;
            spriteBatch.Draw(_fadeTexture, new Rectangle(tailX - half, plate.Bottom + row, half * 2, 1), cream);
            if (half > outline)
                spriteBatch.Draw(_fadeTexture,
                    new Rectangle(tailX - half + outline, plate.Bottom + row - (row == 0 ? outline : 0),
                        (half - outline) * 2, row == 0 ? outline + 1 : 1), dark);
        }
        spriteBatch.DrawString(_interactionFont, _calloutText,
            new Vector2(plate.X + padding, plate.Y + padding / 2f), cream);
        spriteBatch.End();
    }

    // The heart icon is only an outline: paint every visible pixel red for contrast.
    private static Texture2D CreateRedHeart(GraphicsDevice graphicsDevice, Texture2D icon)
    {
        var pixels = new Color[icon.Width * icon.Height];
        icon.GetData(pixels);
        for (int index = 0; index < pixels.Length; index++)
            pixels[index] = HeartCrackColor * (pixels[index].A / 255f);
        var red = new Texture2D(graphicsDevice, icon.Width, icon.Height);
        red.SetData(pixels);
        return red;
    }

    private static Vector2 GetAwayFromSurface(Rectangle player, Rectangle surface, Direction facing)
    {
        const int edgeTolerance = 2;
        // On a floor, push back against the way the player was facing, with a low hop.
        if (player.Bottom <= surface.Top + edgeTolerance)
            return new Vector2(facing == Direction.Left ? 1 : -1, -1);
        if (player.Top >= surface.Bottom - edgeTolerance) return new Vector2(0, 1);
        return new Vector2(player.Center.X < surface.Center.X ? -1 : 1, 0);
    }

    private void ApplyDamageFeedback(DamageVolume damageVolume)
    {
        SolutionVisualModule visualModules = GetAppliedVisualModules();
        Rectangle damageBounds = damageVolume.Bounds;
        int playerCenterX = _player.HitBox.Center.X;
        int horizontalDirection = playerCenterX.CompareTo(damageBounds.Center.X);

        _damageContactPoint = new Vector2(
            Math.Clamp(playerCenterX, damageBounds.Left, damageBounds.Right),
            Math.Clamp(_player.HitBox.Center.Y, damageBounds.Top, damageBounds.Bottom));

        if (visualModules.HasFlag(SolutionVisualModule.DAMAGE_KNOCKBACK))
        {
            _player.ApplyDamageKnockback(horizontalDirection);
        }

        if (visualModules.HasFlag(SolutionVisualModule.DAMAGE_PARTICLES))
        {
            _damageParticlesTimer = DamageFeedbackSettings.ImpactEffectDurationSeconds;
        }

        if (visualModules.HasFlag(SolutionVisualModule.DAMAGE_IMPACT_MARK))
        {
            _damageImpactMarkTimer = DamageFeedbackSettings.ImpactEffectDurationSeconds;
        }

        if (visualModules.HasFlag(SolutionVisualModule.DAMAGE_COMPRESSION))
        {
            _player.StartDamageCompression();
        }
    }

    private void ClearDamageFeedbackEffects()
    {
        _damageParticlesTimer = 0;
        _damageImpactMarkTimer = 0;
        _heartCrackTimer = 0;
        _screenShakeTimer = 0;
        _player?.ClearDamageFeedback();
    }

    public void SyncInterventionState()
    {
        _exitDoor.SetOpen(_phaseState?.ExitDoorIsOpen == true);
        ClearDamageFeedbackEffects();
    }

    private void ConfigureExitDoor()
    {
        _exitDoor.Configure(_map.ExitDoorBounds, _map.ExitDoorDestination);
        _exitDoor.Reset(
            _phaseState?.VerticalRouteRevealed == true,
            _phaseState?.ExitDoorIsOpen == true);
    }

    private void ApplyGelatinFallSpeed()
    {
        foreach (Rectangle gelatin in GetGelatinVolumes(
            _phaseState?.VerticalRouteRevealed == true))
        {
            if (!_player.HitBox.Intersects(gelatin))
            {
                continue;
            }

            _player.LimitFallSpeed(GelatinMaximumFallSpeed * _player.RelativeScale);
            return;
        }
    }

private void UpdateGelatinContact()
{
        foreach (Rectangle gelatin in GetGelatinVolumes(
            _phaseState?.VerticalRouteRevealed == true))
        {
        if (!_player.HitBox.Intersects(gelatin) || _usedGelatins.Contains(gelatin))
        {
            continue;
        }

        bool dashWasAvailable = _player.CanDash;

        SolutionVisualModule visualModules = GetAppliedVisualModules();
        // Restaurar (phase 3) makes the trampoline impostor act like the known gelatin.
        bool restoresTrampolineGelatin = visualModules.HasFlag(
            SolutionVisualModule.RESTORE_TRAMPOLINE_GELATIN) || ImpostorsRestored;

        if (_map.IsTrampolineGelatin(gelatin) && !restoresTrampolineGelatin)
        {
            if (_player.Velocity.Y >= 0)
            {
                float trampolineSpeed = visualModules.HasFlag(
                    SolutionVisualModule.TRAMPOLINE_DASH_EQUIVALENT)
                    ? ConceptualModelsConstraintsSettings
                        .TrampolineDashEquivalentVerticalSpeed
                    : ConceptualModelsConstraintsSettings.TrampolineVerticalSpeed;
                _player.ApplyUpwardImpulse(trampolineSpeed);
                _usedGelatins.Add(gelatin);
            }

            continue;
        }

        if (_phaseState?.AppliedSolutionUsesModule(
            SolutionVisualModule.PLAYER_DASH_RECOVERY_COLOR) == true)
            {
                _player.RestoreDash();
            }
            else
        {
            _player.RestoreDashWithoutVisualFeedback();
        }

        if (!dashWasAvailable &&
            _phaseState?.AppliedSolutionUsesModule(
                SolutionVisualModule.PLAYER_DASH_RECHARGE_SYMBOL) == true)
        {
            _player.StartDashRechargeSymbol();
        }

        if (!dashWasAvailable &&
            _phaseState?.AppliedSolutionUsesModule(
                SolutionVisualModule.PLAYER_DASH_ENERGY_ABSORPTION) == true)
        {
            _player.StartDashEnergyAbsorption(new Vector2(
                gelatin.Center.X,
                gelatin.Center.Y));
        }

        _usedGelatins.Add(gelatin);
    }
}

private void UpdateSurfaceContactFeedback()
{
    bool verticalRouteRevealed =
        _phaseState?.VerticalRouteRevealed == true;

    bool playerIsTouchingSolidSurface =
        PlayerTouchesAnySurface(_map.Platforms) ||
        (verticalRouteRevealed &&
         PlayerTouchesAnySurface(_map.VerticalRoutePlatforms));

    bool playerIsTouchingGelatinSurface =
        PlayerTouchesAnySurface(
            GetGelatinVolumes(verticalRouteRevealed));

    bool contactStarted =
        playerIsTouchingSolidSurface &&
        !_playerWasTouchingSolidSurface ||
        playerIsTouchingGelatinSurface &&
        !_playerWasTouchingGelatinSurface;

    if (contactStarted &&
        _phaseState?.AppliedSolutionUsesModule(
            SolutionVisualModule.PLAYER_SURFACE_CONTACT_FLASH) == true)
    {
        _player.StartSurfaceContactFlash();
    }

    _playerWasTouchingSolidSurface =
        playerIsTouchingSolidSurface;
    _playerWasTouchingGelatinSurface =
        playerIsTouchingGelatinSurface;
}

private bool PlayerTouchesAnySurface(
    IEnumerable<Rectangle> surfaces)
{
    Rectangle playerBounds = _player.HitBox;

    foreach (Rectangle surface in surfaces)
    {
        if (RectanglesTouchOrIntersect(playerBounds, surface))
        {
            return true;
        }
    }

    return false;
}

private static bool RectanglesTouchOrIntersect(
    Rectangle first,
    Rectangle second)
{
    if (first.Intersects(second))
    {
        return true;
    }

    bool horizontalOverlap =
        first.Left < second.Right &&
        first.Right > second.Left;

    bool verticalOverlap =
        first.Top < second.Bottom &&
        first.Bottom > second.Top;

    bool touchesHorizontally =
        verticalOverlap &&
        (first.Right == second.Left ||
         first.Left == second.Right);

    bool touchesVertically =
        horizontalOverlap &&
        (first.Bottom == second.Top ||
         first.Top == second.Bottom);

    return touchesHorizontally || touchesVertically;
}

    private bool PlayerTouchedCutsceneTrigger()
    {
        return _map.HasCutsceneTrigger(_player.HitBox);
    }

    // Room format: losing a heart (fall, or an Affordance spike) returns the player to
    // the room's entry point; the run only ends when the hearts run out.
    private void RespawnInRoom(Camera camera, bool fall)
    {
        _phaseState.RegisterRoomRespawn(fall);
        if (_phaseState.CurrentHealth <= 0)
        {
            HandleHeartsDepleted(camera);
            return;
        }
        RespawnPlayer(camera);
        _respawnEffectTime = 0.22f;
    }

    // In the intervention room the player tests solutions freely: running out of hearts
    // only refills them at the room's spawn. Elsewhere it opens the death menu.
    private void HandleHeartsDepleted(Camera camera)
    {
        if (!IsInterventionRoom)
        {
            KillPlayer();
            return;
        }
        _phaseState.BeginAttempt();
        RespawnPlayer(camera);
        _respawnEffectTime = 0.22f;
    }

    private bool PlayerFell()
    {
        if (SupportsCameraScalePreview)
        {
            return _player.Position.Y >
                _map.HeightInPixels + GameScaleConfig.AffordanceVisibilityFallMargin;
        }

        return _player.Position.Y > (UsesRoomFormat ? ScreenHeight + 16 : FallLimitY);
    }

    private void KillPlayer()
    {
        ClearDamageFeedbackEffects();
        _playerIsDead = true;
        _usedGelatins.Clear();
        _playerWasTouchingSolidSurface = false;
        _playerWasTouchingGelatinSurface = false;
        _player.CancelDash();
        _player.ClearSurfaceContactFlash();
        _player.ClearDashRechargeSymbol();
        _player.ClearDashEnergyAbsorption();
        _player.ResetVelocity();
    }

    private void RespawnPlayer(Camera camera)
    {
        _map.ResetDynamicPlatforms();
        _playerIsDead = false;
        _usedGelatins.Clear();
        _playerWasTouchingSolidSurface = false;
        _playerWasTouchingGelatinSurface = false;
        _player.Move(
            (int)_checkpoint.X + (PlayerWidth - _player.HitBox.Width) / 2,
            (int)_checkpoint.Y + PlayerHeight - _player.HitBox.Height);
        _player.CancelDash();
        _player.ClearSurfaceContactFlash();
        _player.ClearDashRechargeSymbol();
        _player.ClearDashEnergyAbsorption();
        _player.ResetVelocity();
        _player.Grounded();
        _player.RestoreDash();

        _fadeAlpha = 0.0f;
        SecondsPerFrame = SecondsPerFramePlaying;
        SceneMode = SceneMode.PLAYING;

        UpdatePlayingCamera(camera);
    }

    private void StartFadeOut()
    {
        SecondsPerFrame = SecondsPerFrameCutscene;
        SceneMode = SceneMode.FADE_OUT;
    }

    private void UpdateFadeOut(Camera camera)
    {
        if (_fadeAlpha < 1.0f)
        {
            _fadeAlpha += FadeStep;
        }

        if (_fadeAlpha >= 1.0f)
        {
            _fadeAlpha = 1.0f;
            _player.Move((int)_checkpoint.X, (int)_checkpoint.Y);
            UpdateCutsceneCamera(camera);
            SceneMode = SceneMode.FADE_IN;
        }
    }

    private void UpdateFadeIn()
    {
        if (_fadeAlpha > 0.0f)
        {
            _fadeAlpha -= FadeStep;
        }

        if (_fadeAlpha <= 0.0f)
        {
            _fadeAlpha = 0.0f;
            SceneMode = SceneMode.CUTSCENE;
        }
    }

    private void UpdateCutscene(Camera camera, double deltaTime)
    {
        UpdateCutsceneCamera(camera);

        if (_player.Position.X <= CutsceneEndX)
        {
            _player.Walk(CutsceneWalkSpeed, 0, deltaTime);
        }
        else
        {
            FinishCutscene(camera);
        }
    }

    private void UpdatePlayingCamera(Camera camera)
    {
        if (_cameraBehavior == ScenarioCameraBehavior.CENTER_MAP)
        {
            camera.Zoom = UseOriginalScaleTestCamera
                ? GameScaleConfig.OriginalCameraZoom
                : _playerScreenHeightRatio.HasValue
                    ? camera.Dimensions.Y * _playerScreenHeightRatio.Value / _player.UnscaledReferenceHeight
                    : GameScaleConfig.GetCameraZoom(camera.Dimensions.Y, _player.UnscaledReferenceHeight);
            camera.LookAt(new Vector2(_map.WidthInPixels / 2f, _map.HeightInPixels / 2f));
            return;
        }

        camera.Zoom = SupportsCameraScalePreview
            ? GameScaleConfig.GetCameraZoom(camera.Dimensions.Y, _player.UnscaledReferenceHeight)
            : GameScaleConfig.OriginalCameraZoom;
        if (UsesRoomFormat) camera.LookAt(new Vector2(ScreenWidth / 2f, ScreenHeight / 2f));
        else camera.Follow(_player);
    }

    public bool SupportsCameraScalePreview =>
        _supportsPlayerScalePreview;

    public bool TrySetPlayerScale(int preset)
    {
        if (!SupportsCameraScalePreview || preset < 1 || preset > 4)
            return false;

        return _player.TrySetRelativeScale(
            GameScaleConfig.GetPlayerScale(preset),
            GetSolidColliders());
    }

    private void UpdateCutsceneCamera(Camera camera)
    {
        camera.Zoom = CutsceneCameraZoom;
        camera.Follow(_player);
    }

    private void FinishCutscene(Camera camera)
    {
        SceneMode = SceneMode.PLAYING;
        SecondsPerFrame = SecondsPerFramePlaying;
        camera.Zoom = 1.0f;
        _fadeAlpha = 0.0f;
        _player.Move((int)_playerStart.X, (int)_playerStart.Y);
    }

    public void Draw(SpriteBatch spriteBatch, Camera camera, Effect playerEffect = null)
    {
        DrawWorld(spriteBatch, camera, playerEffect);
        DrawMortCallout(spriteBatch, camera);
        DrawScreenEffects(spriteBatch);
    }

    public void DrawDebug(SpriteBatch spriteBatch, Camera camera, DebugView debugView)
    {
        debugView.DrawWorld(
            spriteBatch,
            camera,
            _player,
            _map.Platforms,
            _map.Gelatins,
            _map.VerticalRoutePlatforms,
            _map.VerticalRouteGelatins,
            _phaseState?.VerticalRouteRevealed == true,
            _map.CutsceneTriggers,
            _map.SceneExits,
            _map.InterventionEntrances,
            _map.InterventionButtons,
            _exitDoor.Collider,
            _map.DebugMeasurementPair,
            GetActiveInterventionEntrancePlacement(),
            _checkpoint);

        debugView.DrawScaleOverlay(
            spriteBatch,
            camera,
            _player,
            _propVisualPresentation?.DebugReferenceHeight ?? _player.DebugReferenceHeight,
            _playerScreenHeightRatio.HasValue);
    }

    private void DrawTutorialBackground(SpriteBatch spriteBatch, Matrix cameraTransform)
    {
        spriteBatch.Begin();
        spriteBatch.Draw(_fadeTexture, _fadeRectangle, TutorialBackgroundColor);
        spriteBatch.End();

        spriteBatch.Begin(samplerState: SamplerState.LinearClamp, transformMatrix: cameraTransform);
        foreach (var (bounds, direction) in _map.SignArrows)
            DrawSignArrow(spriteBatch, bounds, direction);
        foreach (var (bounds, text) in _map.SignTexts)
            DrawSignText(spriteBatch, bounds, TutorialControls.Fill(text));
        foreach (Rectangle sign in _map.ControlSigns)
        {
            spriteBatch.Draw(_fadeTexture, sign, TutorialSignColor);
            Rectangle inner = sign;
            inner.Inflate(-4, -4);
            spriteBatch.Draw(_fadeTexture, inner, TutorialBackgroundColor);
            inner.Inflate(-14, -14);
            DrawSignText(spriteBatch, inner, "Controles");
        }
        spriteBatch.End();
    }

    // The text fills its LDtk box (keeping proportions), with a soft shadow for contrast.
    private void DrawSignText(SpriteBatch spriteBatch, Rectangle bounds, string text)
    {
        if (_interactionFont == null) return;
        Vector2 size = _interactionFont.MeasureString(text);
        if (size.X <= 0 || size.Y <= 0) return;
        float scale = Math.Min(bounds.Width / size.X, bounds.Height / size.Y);
        Vector2 position = bounds.Center.ToVector2() - size * scale / 2f;
        Vector2 shadow = new Vector2(1f, 1f) * Math.Max(2f, bounds.Height * 0.04f);
        spriteBatch.DrawString(_interactionFont, text, position + shadow, Color.Black * 0.5f,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        spriteBatch.DrawString(_interactionFont, text, position, TutorialSignColor,
            0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    private void DrawSignArrow(SpriteBatch spriteBatch, Rectangle bounds, string direction)
    {
        Vector2 dir = direction switch
        {
            "cima" => new Vector2(0, -1),
            "baixo" => new Vector2(0, 1),
            "esquerda" => new Vector2(-1, 0),
            "cima-direita" => Vector2.Normalize(new Vector2(1, -1)),
            "cima-esquerda" => Vector2.Normalize(new Vector2(-1, -1)),
            _ => new Vector2(1, 0)
        };
        bool diagonal = dir.X != 0 && dir.Y != 0;
        float length = diagonal
            ? MathF.Sqrt(bounds.Width * bounds.Width + bounds.Height * bounds.Height) * 0.8f
            : (dir.X != 0 ? bounds.Width : bounds.Height) * 0.9f;
        float thickness = Math.Max(6f, Math.Min(bounds.Width, bounds.Height) * 0.14f);
        float headLength = Math.Min(length * 0.35f, thickness * 4f);
        Vector2 center = bounds.Center.ToVector2();
        Vector2 tail = center - dir * length / 2f;
        Vector2 tip = center + dir * length / 2f;
        Vector2 shadow = new Vector2(1f, 1f) * Math.Max(2f, thickness * 0.3f);
        foreach ((Vector2 offset, Color color) in new[]
                 { (shadow, Color.Black * 0.5f), (Vector2.Zero, TutorialSignColor) })
        {
            DrawSignLine(spriteBatch, tail + offset, tip + offset, thickness, color);
            foreach (float side in new[] { -1f, 1f })
            {
                Vector2 back = Vector2.Transform(-dir, Matrix.CreateRotationZ(side * MathHelper.ToRadians(40)));
                DrawSignLine(spriteBatch, tip + offset, tip + offset + back * headLength, thickness, color);
            }
        }
    }

    private void DrawSignLine(SpriteBatch spriteBatch, Vector2 from, Vector2 to, float thickness, Color color)
    {
        Vector2 delta = to - from;
        spriteBatch.Draw(_fadeTexture, from, null, color, MathF.Atan2(delta.Y, delta.X),
            new Vector2(0f, 0.5f), new Vector2(delta.Length(), thickness), SpriteEffects.None, 0f);
    }

    // Screen-space offset that decays over the shake.
    private Matrix GetScreenShakeTransform()
    {
        if (_screenShakeTimer <= 0) return Matrix.Identity;
        float strength = (float)(_screenShakeTimer / DamageFeedbackSettings.ScreenShakeSeconds);
        float amplitude = DamageFeedbackSettings.ScreenShakePixels * strength;
        return Matrix.CreateTranslation(
            (float)Math.Sin(_screenShakeTimer * 95.0) * amplitude,
            (float)Math.Cos(_screenShakeTimer * 77.0) * amplitude * 0.6f, 0f);
    }

    // Red glow on the screen edges, fading inward and over time.
    private void DrawDamageBorder(SpriteBatch spriteBatch)
    {
        if (_screenShakeTimer <= 0) return;
        const int bands = 24;
        const int thickness = 110;
        float strength = (float)(_screenShakeTimer / DamageFeedbackSettings.ScreenShakeSeconds);
        int width = _fadeRectangle.Width, height = _fadeRectangle.Height;
        int step = thickness / bands;
        spriteBatch.Begin();
        for (int band = 0; band < bands; band++)
        {
            float edge = 1f - band / (float)bands;
            Color color = DamageBorderColor * (0.5f * edge * edge * strength);
            int inset = band * step;
            spriteBatch.Draw(_fadeTexture, new Rectangle(0, inset, width, step), color);
            spriteBatch.Draw(_fadeTexture, new Rectangle(0, height - inset - step, width, step), color);
            spriteBatch.Draw(_fadeTexture, new Rectangle(inset, 0, step, height), color);
            spriteBatch.Draw(_fadeTexture, new Rectangle(width - inset - step, 0, step, height), color);
        }
        spriteBatch.End();
    }

    // Placeholder for the 3-frame art: the heart pops in, trembles as it cracks,
    // then splits in two halves that fall and fade.
    private void DrawHeartCrack(SpriteBatch spriteBatch)
    {
        if (_heartCrackTimer <= 0 || _heartTexture == null) return;
        float progress = 1f - (float)(_heartCrackTimer / DamageFeedbackSettings.HeartCrackSeconds);
        Rectangle player = _player.HitBox;
        float size = player.Width * 1.4f;
        float scale = size / _heartTexture.Width;
        var center = new Vector2(player.Center.X, player.Top - size * 0.8f);
        int halfWidth = _heartTexture.Width / 2;
        if (progress < 0.45f)
        {
            float pop = Math.Min(1f, progress / 0.12f);
            float tremble = progress > 0.2f ? (float)Math.Sin(progress * 90.0) * size * 0.04f : 0f;
            spriteBatch.Draw(_heartTexture, center + new Vector2(tremble, 0), null, Color.White, 0f,
                new Vector2(_heartTexture.Width / 2f, _heartTexture.Height / 2f), scale * pop,
                SpriteEffects.None, 0f);
            return;
        }

        float split = (progress - 0.45f) / 0.55f;
        float spread = split * size * 0.35f;
        float drop = split * split * size * 0.6f;
        Color color = Color.White * (1f - split);
        var halfOrigin = new Vector2(halfWidth / 2f, _heartTexture.Height / 2f);
        spriteBatch.Draw(_heartTexture, center + new Vector2(-size / 4f - spread, drop),
            new Rectangle(0, 0, halfWidth, _heartTexture.Height), color, -split * 0.5f,
            halfOrigin, scale, SpriteEffects.None, 0f);
        spriteBatch.Draw(_heartTexture, center + new Vector2(size / 4f + spread, drop),
            new Rectangle(halfWidth, 0, _heartTexture.Width - halfWidth, _heartTexture.Height), color,
            split * 0.5f, halfOrigin, scale, SpriteEffects.None, 0f);
    }

    private void DrawDamageEventEffects(
        SpriteBatch spriteBatch,
        SolutionVisualModule visualModules)
    {
        DrawHeartCrack(spriteBatch);
        if (_damageParticlesTimer > 0 &&
            visualModules.HasFlag(SolutionVisualModule.DAMAGE_PARTICLES))
        {
            float progress = 1f - (float)(
                _damageParticlesTimer / DamageFeedbackSettings.ImpactEffectDurationSeconds);
            Vector2[] directions =
            [
                new(-1f, -1f),
                new(0f, -1.3f),
                new(1f, -1f),
                new(-1.2f, 0f),
                new(1.2f, 0f),
                new(0f, 1f)
            ];

            foreach (Vector2 direction in directions)
            {
                Vector2 position = _damageContactPoint + direction * (8f + 18f * progress);
                spriteBatch.Draw(
                    _fadeTexture,
                    new Rectangle((int)position.X - 2, (int)position.Y - 2, 4, 4),
                    Color.OrangeRed * (1f - progress));
            }
        }

        if (_damageImpactMarkTimer > 0 &&
            visualModules.HasFlag(SolutionVisualModule.DAMAGE_IMPACT_MARK))
        {
            float opacity = (float)(
                _damageImpactMarkTimer / DamageFeedbackSettings.ImpactEffectDurationSeconds);
            int centerX = (int)_damageContactPoint.X;
            int centerY = (int)_damageContactPoint.Y;
            Color markColor = Color.Gold * opacity;

            spriteBatch.Draw(
                _fadeTexture,
                new Rectangle(centerX - 10, centerY - 2, 20, 4),
                markColor);
            spriteBatch.Draw(
                _fadeTexture,
                new Rectangle(centerX - 2, centerY - 10, 4, 20),
                markColor);
        }
    }

    private void DrawWorld(SpriteBatch spriteBatch, Camera camera, Effect playerEffect)
    {
        if (_propVisualPresentation != null)
        {
            DrawPropVisualWorld(spriteBatch, camera);
            return;
        }

        var cameraTransform = camera.GetTransform() * GetScreenShakeTransform();
        bool drawsScenery = _map.HasLdtkScenery && !_useAdditionalCollidersOnly;
        if (_isTutorial) DrawTutorialBackground(spriteBatch, cameraTransform);
        if (drawsScenery)
        {
            // Painted 1080p layers are upscaled; linear filtering avoids blocky edges.
            spriteBatch.Begin(samplerState: SamplerState.LinearClamp,
                transformMatrix: cameraTransform);
            _map.DrawLdtkScenery(spriteBatch, inFrontOfPlayer: false);
            spriteBatch.End();
        }
        if (_map.IsLdtk && !_useAdditionalCollidersOnly)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp,
                transformMatrix: cameraTransform);
            _map.DrawLdtkBackground(spriteBatch, GetAppliedVisualModules());
            spriteBatch.End();
        }
    spriteBatch.Begin(
        samplerState: SamplerState.PointClamp,
        effect: playerEffect,
        transformMatrix: cameraTransform);

    SolutionVisualModule visualModules = GetAppliedVisualModules();

    _player.Draw(
        spriteBatch,
        visualModules,
        _phaseState?.DamageInvulnerabilityRemainingSeconds ?? 0);
    if (playerEffect != null)
    {
        spriteBatch.End();
        spriteBatch.Begin(samplerState: SamplerState.PointClamp,
            transformMatrix: cameraTransform);
    }
    if (!_useAdditionalCollidersOnly)
    {
        _map.Draw(
            spriteBatch,
                GetActiveInterventionEntrancePlacement(),
                _interventionTrigger,
            _phaseState?.VerticalRouteRevealed == true,
            visualModules);
        _exitDoor.Draw(spriteBatch, _fadeTexture);
    }

    if (visualModules.HasFlag(SolutionVisualModule.CONCEPTUAL_RULE_REMINDERS))
    {
        _conceptualRuleReminderPresentation?.Draw(
            spriteBatch,
            _map,
            _phaseState?.VerticalRouteRevealed == true);
    }

    if (UsesRoomFormat) DrawRoomFormatEffects(spriteBatch);
    if (_isTutorial)
        foreach (Rectangle sign in _map.ControlSigns)
            DrawInteractionPrompt(spriteBatch, sign);
    DrawDamageEventEffects(spriteBatch, visualModules);
    _player.DrawDashRechargeSymbol(spriteBatch, visualModules);
    _player.DrawDashEnergyAbsorption(spriteBatch, visualModules);

        spriteBatch.End();

        var litExits = GetLitExits().ToList();
        if (litExits.Count > 0)
        {
            spriteBatch.Begin(blendState: BlendState.Additive, samplerState: SamplerState.LinearClamp,
                transformMatrix: cameraTransform);
            foreach (SceneExit exit in litExits)
                ExitLight.Draw(spriteBatch, exit.Bounds, _map.WidthInPixels, _map.HeightInPixels, _passageTime);
            spriteBatch.End();
        }

        var blockedExits = GetRoom3Barriers().ToList();
        if (blockedExits.Count > 0)
        {
            spriteBatch.Begin(samplerState: SamplerState.LinearClamp, transformMatrix: cameraTransform);
            foreach (SceneExit exit in blockedExits)
                ExitLight.Draw(spriteBatch, exit.Bounds, _map.WidthInPixels, _map.HeightInPixels,
                    _passageTime, dark: true);
            spriteBatch.End();
        }

        if (drawsScenery)
        {
            spriteBatch.Begin(samplerState: SamplerState.LinearClamp,
                transformMatrix: cameraTransform);
            _map.DrawLdtkScenery(spriteBatch, inFrontOfPlayer: true);
            spriteBatch.End();
        }
    }

    private void DrawPropVisualWorld(SpriteBatch spriteBatch, Camera camera)
    {
        Matrix cameraTransform = camera.GetTransform();
        SolutionVisualModule visualModules = GetAppliedVisualModules();

        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        _propVisualPresentation.DrawBackground(spriteBatch);
        spriteBatch.End();

        spriteBatch.Begin(
            samplerState: SamplerState.LinearClamp,
            transformMatrix: cameraTransform);
        _propVisualPresentation.DrawMidground(
            spriteBatch,
            _map.Platforms,
            _map.Gelatins);
        spriteBatch.End();

        spriteBatch.Begin(
            samplerState: SamplerState.LinearClamp,
            transformMatrix: cameraTransform);
        _propVisualPresentation.DrawPlayer(spriteBatch, _player);
        DrawDamageEventEffects(spriteBatch, visualModules);
        _player.DrawDashRechargeSymbol(spriteBatch, visualModules);
        _player.DrawDashEnergyAbsorption(spriteBatch, visualModules);
        spriteBatch.End();

        spriteBatch.Begin(
            samplerState: SamplerState.LinearClamp,
            transformMatrix: cameraTransform);
        _propVisualPresentation.DrawForeground(
            spriteBatch,
            _map.PropVisualForegrounds);
        spriteBatch.End();
    }

    private bool _combinationDemo;

    public void EnableCombinationDemo()
    {
        _combinationDemo = _phaseState != null &&
            _phaseState.CurrentScenarioId == _phaseState.Definition.LastRoomScenarioId;
    }

    private SolutionVisualModule GetAppliedVisualModules()
    {
        SolutionVisualModule modules = _phaseState?.AppliedVisualModules ?? SolutionVisualModule.NONE;
        if (_combinationDemo)
        {
            // Demonstration only: preserve the player's choice and completion records.
            // It only runs on top of the phase's best solution, so nothing needs removing.
            modules |= _phaseState.Definition.DemonstrationModules;
        }
        return modules;
    }

    private void ResolveInterventionReturn()
    {
        if (IsInterventionRoom &&
            _requestedScenarioId == _phaseState.Definition.RoomScenarioIds[0])
        {
            _requestedScenarioId = _phaseState.InterventionReturnScenarioId;
            _requestedSpawnId = "retorno_intervencao";
        }
    }

    private InterventionEntrancePlacement? GetActiveInterventionEntrancePlacement()
    {
        if (UsesRoomFormat)
        {
            if (IsInterventionRoom)
                return _phaseState.HasFirstChoice ? InterventionEntrancePlacement.LATE : null;

            // The entrance shows in room 2, or in room 1 after two deaths.
            // Once a solution is applied the entrance closes: the choice is final.
            bool entranceIsAvailable = !_phaseState.HasFirstChoice &&
                (CurrentRoomIndex == 1 ||
                 CurrentRoomIndex == 0 && _phaseState.DeathCount >= 2);
            return entranceIsAvailable ? InterventionEntrancePlacement.LATE : null;
        }
        if (_phaseState == null || !_phaseState.InterventionEntranceIsActive)
        {
            return null;
        }

        return _phaseState.InterventionEntrancePlacement;
    }

    private void DrawRoomFormatEffects(SpriteBatch spriteBatch)
    {
        foreach (InterventionEntrance entrance in _map.InterventionEntrances)
            if (entrance.Placement == GetActiveInterventionEntrancePlacement())
            {
                FloatingPassageSymbols.Draw(spriteBatch, _passageSymbols, entrance.Bounds, _passageTime);
                DrawInteractionPrompt(spriteBatch, entrance.Bounds);
            }
        if (IsInterventionRoom)
            foreach (SceneExit exit in _map.SceneExits)
            {
                FloatingPassageSymbols.Draw(spriteBatch, _passageSymbols, exit.Bounds, _passageTime);
                DrawInteractionPrompt(spriteBatch, exit.Bounds);
            }
        foreach (Rectangle sword in _map.InterventionButtons)
            DrawInteractionPrompt(spriteBatch, _interventionTrigger.GetSwordBounds(sword));

        if (_respawnEffectTime <= 0) return;
        float progress = 1 - _respawnEffectTime / 0.22f;
        Vector2 center = _player.HitBox.Center.ToVector2();
        for (int i = 0; i < 8; i++)
        {
            float angle = i * MathHelper.TwoPi / 8;
            Vector2 point = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (12 + progress * 36);
            spriteBatch.Draw(_fadeTexture, new Rectangle((int)point.X, (int)point.Y, 5, 5),
                new Color(166, 155, 174) * (1 - progress));
        }
    }

    private void DrawInteractionPrompt(SpriteBatch spriteBatch, Rectangle bounds)
    {
        const float scale = 1.0f;
        Vector2 size = _interactionFont.MeasureString("E") * scale;
        Vector2 position = new(bounds.Center.X - size.X / 2, bounds.Top - size.Y - 14);
        var plate = new Rectangle((int)position.X - 10, (int)position.Y - 4,
            (int)MathF.Ceiling(size.X) + 20, (int)MathF.Ceiling(size.Y) + 8);
        spriteBatch.Draw(_fadeTexture, plate, new Color(255, 237, 182));
        plate.Inflate(-2, -2);
        spriteBatch.Draw(_fadeTexture, plate, new Color(17, 20, 26));
        spriteBatch.DrawString(_interactionFont, "E",
            position, new Color(255, 237, 182), 0, Vector2.Zero, scale, SpriteEffects.None, 0);
    }

    private void DrawScreenEffects(SpriteBatch spriteBatch)
    {
        DrawDamageBorder(spriteBatch);
        if (!ShouldDrawFade())
        {
            return;
        }

        spriteBatch.Begin();
        spriteBatch.Draw(_fadeTexture, _fadeRectangle, new Color(Color.Black, _fadeAlpha));
        spriteBatch.End();
    }

    private bool ShouldDrawFade()
    {
        return SceneMode == SceneMode.FADE_OUT
            || SceneMode == SceneMode.FADE_IN
            || SceneMode == SceneMode.CUTSCENE;
    }
}
