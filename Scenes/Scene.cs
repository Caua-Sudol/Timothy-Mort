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

    private const double PlayerGravity = -1800.0;
    private const double SecondsPerFramePlaying = 1.0 / 60.0;
    private const double SecondsPerFrameCutscene = 1.0 / 5.0;

    private const int ScreenWidth = 1920;
    private const int ScreenHeight = 1080;
    private const int FallLimitY = ScreenHeight + 128;

    private const float FadeStep = 0.2f;
    private const float CutsceneCameraZoom = 2.0f;
    private const float CutsceneWalkSpeed = 10.0f;
    private const float CutsceneEndX = 940.0f;
    private const float GelatinMaximumFallSpeed = 96.0f;
    private const float InterventionButtonPressedOffset = 8.0f;
    private const float InterventionButtonPressSpeed = 64.0f;
    private const float ExitDoorRiseSpeed = 768.0f;

    private Texture2D _fadeTexture;
    private SpriteFont _interactionFont;
    private bool _interactPressed;
    private float _respawnEffectTime;
    private bool IsAffordance => _phaseState?.PhaseId == ScenarioCatalog.AffordanceVisibilityScenarioId;

    public void SetInteractionPressed(bool pressed) => _interactPressed = pressed;
    public void AlignCamera(Camera camera) => UpdatePlayingCamera(camera);
    private double _damageParticlesTimer;
    private double _damageImpactMarkTimer;
    private Vector2 _damageContactPoint;
    private Rectangle _fadeRectangle;
    private float _fadeAlpha;

    private Player _player;
    private readonly LevelMap _map;
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
    private float _interventionButtonPressOffset;
    private Vector2 _exitDoorPosition;
    private bool _exitDoorIsOpen;
    private bool _interventionButtonActivationPending;
    private bool _interventionButtonActivationIssued;

    public SceneMode SceneMode { get; private set; }
    public double SecondsPerFrame { get; private set; }
    public bool PlayerIsDead
    {
        get
        {
            return _playerIsDead;
        }
    }

    public bool InterventionButtonPressed { get; private set; }

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
            scenario.Id == ScenarioCatalog.PropVisualScenarioId)
    {
    }

    public Scene(string mapPath, Vector2 playerStart)
        : this(mapPath, playerStart, null, null, false)
    {
    }

    private Scene(
        string mapPath,
        Vector2 playerStart,
        string entrySpawnId,
        PhaseState phaseState,
        bool usesPropVisualPresentation)
    {
        _map = new LevelMap(mapPath);
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

    public void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
    {
        _fadeAlpha = 0.0f;
        _playerIsDead = false;
        _requestedScenarioId = null;
        _requestedSpawnId = null;
        InterventionButtonPressed = false;
        _interventionButtonPressOffset = 0.0f;
        SyncInterventionState();
        _interventionButtonActivationPending = false;
        _interventionButtonActivationIssued = false;
        _usedGelatins.Clear();
        _playerWasTouchingSolidSurface = false;
        _playerWasTouchingGelatinSurface = false;
        _fadeRectangle = new Rectangle(0, 0, ScreenWidth, ScreenHeight);
        _map.LoadContent(graphicsDevice);
        if (IsAffordance) _interactionFont = content.Load<SpriteFont>("patrick");
        _propVisualPresentation?.LoadContent(content);
        _conceptualRuleReminderPresentation?.LoadContent(graphicsDevice, content);
        _exitDoorPosition = GetExitDoorRestingPosition();
        _playerStart = _map.GetPlayerSpawn(_entrySpawnId)
            ?? _map.PlayerSpawn
            ?? _fallbackPlayerStart;
        _checkpoint = _playerStart;

        var fadeColor = Enumerable.Repeat(Color.White, ScreenWidth * ScreenHeight).ToArray();
        _fadeTexture = new Texture2D(graphicsDevice, ScreenWidth, ScreenHeight);
        _fadeTexture.SetData(fadeColor);

        _player = new Player((int)_playerStart.X, (int)_playerStart.Y, PlayerGravity, PlayerWidth, PlayerHeight);
        _player.LoadContent(graphicsDevice, PlayerWidth, PlayerHeight);

        SceneMode = SceneMode.PLAYING;
        SecondsPerFrame = SecondsPerFramePlaying;
    }

    public void Update(Camera camera, double deltaTime)
    {
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
        InterventionButtonPressed = false;
        _interventionButtonPressOffset = 0.0f;
        SyncInterventionState();
        _exitDoorPosition = GetExitDoorRestingPosition();
        _interventionButtonActivationPending = false;
        _interventionButtonActivationIssued = false;

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

    public bool TryTakeInterventionButtonActivation()
    {
        if (!_interventionButtonActivationPending)
        {
            return false;
        }

        _interventionButtonActivationPending = false;
        return true;
    }

private void UpdatePlaying(Camera camera, double deltaTime)
{
    _map.UpdateVisualEffects(deltaTime);
    _respawnEffectTime = Math.Max(0, _respawnEffectTime - (float)deltaTime);
    _map.UpdateDynamicPlatforms(_player, deltaTime);
    _phaseState?.UpdateDamageInvulnerability(deltaTime);
        _player.Update(deltaTime);
        ApplyGelatinFallSpeed();
        bool dashWasAvailableBeforePhysics = _player.CanDash;
        UpdatePlayerPhysics(deltaTime);
        ApplyGelatinFallSpeed();
        _damageParticlesTimer = Math.Max(0, _damageParticlesTimer - deltaTime);
        _damageImpactMarkTimer = Math.Max(0, _damageImpactMarkTimer - deltaTime);
        bool damaged = UpdateDamage();
        if (IsAffordance && damaged)
        {
            RecoverAffordance(camera, false);
            return;
        }

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

        UpdateInterventionButton(deltaTime);
        UpdateExitDoorRise(deltaTime);

        bool playerIsTouchingGelatin = PlayerTouchesAnySurface(
            _map.GetGelatinVolumes(
                _phaseState?.VerticalRouteRevealed == true));

        if (_player.IsGrounded && !playerIsTouchingGelatin)
        {
            _usedGelatins.Clear();
        }

    UpdateGelatinContact();
    UpdateSurfaceContactFeedback();

    if (PlayerFell())
        {
            if (IsAffordance) RecoverAffordance(camera, true);
            else KillPlayer();
            return;
        }

        UpdatePlayingCamera(camera);

        SceneExit sceneExit = _map.GetSceneExit(
            _player.HitBox,
            _exitDoorIsOpen,
            _phaseState?.ExitDoorIsOpen == true);
        if (sceneExit != null)
        {
            _requestedScenarioId = sceneExit.DestinationScenarioId;
            _requestedSpawnId = sceneExit.DestinationSpawnId;
            return;
        }

        InterventionEntrance interventionEntrance = _map.GetInterventionEntrance(
            _player.HitBox,
            GetActiveInterventionEntrancePlacement());

        if (interventionEntrance != null && (!IsAffordance || _interactPressed))
        {
            _requestedScenarioId = interventionEntrance.DestinationScenarioId;
            _requestedSpawnId = interventionEntrance.DestinationSpawnId;
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
            _map.GetSolidColliders(
                _exitDoorIsOpen,
                _phaseState?.VerticalRouteRevealed == true,
                GetAppliedVisualModules().HasFlag(SolutionVisualModule.SAFE_ROUTE),
                _exitDoorPosition),
            deltaTime);
    }

    private bool UpdateDamage()
    {
        if (_phaseState?.HasDamageSystem != true)
        {
            return false;
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

        if (!IsAffordance && _phaseState.CurrentHealth <= 0)
        {
            KillPlayer();
        }
        return true;
    }

    private void UpdateInterventionButton(double deltaTime)
    {
        if (IsAffordance)
        {
            Rectangle reach = _player.HitBox;
            reach.Inflate(32, 24);
            bool nearSword = _map.InterventionButtons.Any(button => reach.Intersects(button));
            InterventionButtonPressed = nearSword && _interactPressed;
            if (InterventionButtonPressed)
                _interventionButtonActivationPending = true;
            return;
        }
        bool playerIsOnButton = _map.HasInterventionButtonUnderPlayer(
            _player.HitBox,
            _player.IsGrounded);

        if (playerIsOnButton)
        {
            InterventionButtonPressed = true;
        }
        else
        {
            InterventionButtonPressed = false;
            _interventionButtonPressOffset = MathHelper.Max(
                0.0f,
                _interventionButtonPressOffset - InterventionButtonPressSpeed * (float)deltaTime);

            if (_interventionButtonPressOffset <= 0.0f)
            {
                _interventionButtonActivationIssued = false;
            }

            return;
        }

        if (_interventionButtonPressOffset < InterventionButtonPressedOffset)
        {
            _interventionButtonPressOffset = MathHelper.Min(
                InterventionButtonPressedOffset,
                _interventionButtonPressOffset + InterventionButtonPressSpeed * (float)deltaTime);
        }

        if (_interventionButtonPressOffset >= InterventionButtonPressedOffset &&
            !_interventionButtonActivationIssued)
        {
            _interventionButtonActivationIssued = true;
            _interventionButtonActivationPending = true;
        }
    }

    private void UpdateExitDoorRise(double deltaTime)
    {
        bool doorShouldMove = InterventionButtonPressed ||
            _phaseState?.VerticalRouteRevealed == true;

        if (!doorShouldMove || _exitDoorIsOpen)
        {
            return;
        }

        Vector2 delta = _map.ExitDoorDestination - _exitDoorPosition;
        float distance = delta.Length();

        if (distance <= 0.0f)
        {
            return;
        }

        float step = ExitDoorRiseSpeed * (float)deltaTime;
        _exitDoorPosition = distance <= step
            ? _map.ExitDoorDestination
            : _exitDoorPosition + delta / distance * step;
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
        _player?.ClearDamageFeedback();
    }

    public void SyncInterventionState()
    {
        _exitDoorIsOpen = _phaseState?.ExitDoorIsOpen == true;
        ClearDamageFeedbackEffects();
    }

    private Vector2 GetExitDoorRestingPosition()
    {
        return _phaseState?.VerticalRouteRevealed == true
            ? _map.ExitDoorDestination
            : _map.ExitDoorStartPosition;
    }

    private void ApplyGelatinFallSpeed()
    {
        foreach (Rectangle gelatin in _map.GetGelatinVolumes(
            _phaseState?.VerticalRouteRevealed == true))
        {
            if (!_player.HitBox.Intersects(gelatin))
            {
                continue;
            }

            _player.LimitFallSpeed(GelatinMaximumFallSpeed);
            return;
        }
    }

private void UpdateGelatinContact()
{
        foreach (Rectangle gelatin in _map.GetGelatinVolumes(
            _phaseState?.VerticalRouteRevealed == true))
        {
        if (!_player.HitBox.Intersects(gelatin) || _usedGelatins.Contains(gelatin))
        {
            continue;
        }

        bool dashWasAvailable = _player.CanDash;

        SolutionVisualModule visualModules = GetAppliedVisualModules();
        bool restoresTrampolineGelatin = visualModules.HasFlag(
            SolutionVisualModule.RESTORE_TRAMPOLINE_GELATIN);

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
            _map.GetGelatinVolumes(verticalRouteRevealed));

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

    private void RecoverAffordance(Camera camera, bool fall)
    {
        _phaseState.RegisterAffordanceRespawn(fall);
        if (_phaseState.CurrentHealth <= 0)
        {
            KillPlayer();
            return;
        }
        RespawnPlayer(camera);
        _respawnEffectTime = 0.22f;
    }

    private bool PlayerFell()
    {
        return _player.Position.Y > (IsAffordance ? ScreenHeight + 16 : FallLimitY);
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
        _player.Move((int)_checkpoint.X, (int)_checkpoint.Y);
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
        camera.Zoom = 1.0f;
        if (IsAffordance) camera.LookAt(new Vector2(ScreenWidth / 2f, ScreenHeight / 2f));
        else camera.Follow(_player);
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

    public void Draw(SpriteBatch spriteBatch, Camera camera)
    {
        DrawWorld(spriteBatch, camera);
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
            _map.GetExitDoorCollider(_exitDoorIsOpen, _exitDoorPosition),
            _map.DebugMeasurementPair,
            GetActiveInterventionEntrancePlacement(),
            _checkpoint);
    }

    private void DrawDamageEventEffects(
        SpriteBatch spriteBatch,
        SolutionVisualModule visualModules)
    {
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

    private void DrawWorld(SpriteBatch spriteBatch, Camera camera)
    {
        if (_propVisualPresentation != null)
        {
            DrawPropVisualWorld(spriteBatch, camera);
            return;
        }

        var cameraTransform = camera.GetTransform();
    spriteBatch.Begin(
        samplerState: SamplerState.PointClamp,
        transformMatrix: cameraTransform);

    SolutionVisualModule visualModules = GetAppliedVisualModules();

    _player.Draw(
        spriteBatch,
        visualModules,
        _phaseState?.DamageInvulnerabilityRemainingSeconds ?? 0);
    _map.Draw(
        spriteBatch,
            GetActiveInterventionEntrancePlacement(),
            _interventionButtonPressOffset,
        _exitDoorPosition,
        _exitDoorIsOpen,
        _phaseState?.VerticalRouteRevealed == true,
        visualModules);

    if (visualModules.HasFlag(SolutionVisualModule.CONCEPTUAL_RULE_REMINDERS))
    {
        _conceptualRuleReminderPresentation?.Draw(
            spriteBatch,
            _map,
            _phaseState?.VerticalRouteRevealed == true);
    }

    if (IsAffordance) DrawAffordanceEffects(spriteBatch);
    DrawDamageEventEffects(spriteBatch, visualModules);
    _player.DrawDashRechargeSymbol(spriteBatch, visualModules);
    _player.DrawDashEnergyAbsorption(spriteBatch, visualModules);

        spriteBatch.End();
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

    private SolutionVisualModule GetAppliedVisualModules()
    {
        return _phaseState?.AppliedVisualModules ?? SolutionVisualModule.NONE;
    }

    private InterventionEntrancePlacement? GetActiveInterventionEntrancePlacement()
    {
        if (IsAffordance)
        {
            bool returningFromIntervention =
                _phaseState.CurrentScenarioId == ScenarioCatalog.AffordanceInterventionScenarioId;
            return returningFromIntervention && !_phaseState.HasFirstChoice
                ? null : InterventionEntrancePlacement.LATE;
        }
        if (_phaseState == null || !_phaseState.InterventionEntranceIsActive)
        {
            return null;
        }

        return _phaseState.InterventionEntrancePlacement;
    }

    private void DrawAffordanceEffects(SpriteBatch spriteBatch)
    {
        foreach (InterventionEntrance entrance in _map.InterventionEntrances)
            if (entrance.Placement == GetActiveInterventionEntrancePlacement())
                DrawInteractionPrompt(spriteBatch, entrance.Bounds);
        foreach (Rectangle sword in _map.InterventionButtons)
            DrawInteractionPrompt(spriteBatch, sword);

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
        Vector2 size = _interactionFont.MeasureString("E") * 0.7f;
        spriteBatch.DrawString(_interactionFont, "E",
            new Vector2(bounds.Center.X - size.X / 2, bounds.Top - size.Y - 8),
            new Color(216, 208, 187), 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);
    }

    private void DrawScreenEffects(SpriteBatch spriteBatch)
    {
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
