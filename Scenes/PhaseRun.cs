using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public sealed class PhaseRun : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly LdtkTextureCache _ldtkTextureCache = new();
    private readonly ContentManager _content;
    private readonly Vector2 _logicalSize;
    private readonly PhaseProgress _phaseProgress;
    private readonly Camera _camera;
    private readonly MortSubtitle _mortSubtitle;
    private readonly PauseMenu _pauseMenu;
    private readonly DeathMenu _deathMenu;
    private readonly DamageHud _damageHud = new();
    private readonly InterventionChoiceMenu _interventionChoiceMenu;
    private readonly ControlSchemeMenu _controlSchemeMenu;
    private readonly ScenarioTransition _scenarioTransition = new();
    private readonly EyeBlinkTransition _solutionBlink = new();
    private readonly Random _choicePresentationRandom = new();
    private SpriteBatch _spriteBatch;
    private Scene _scene;
    private PhaseState _phaseState;
    private AffordanceBackground _affordanceBackground;
    private string _currentScenarioId = ScenarioCatalog.InitialScenarioId;
    private OverlayState _overlayState = OverlayState.NONE;
    private KeyboardState _previousKeyboardState;
    private bool _mortCombinationSpoken;
    // First solution applied: Mort's "try the others" balloon waits for the comment to end.
    private bool _tryOthersCalloutPending;
    private bool _mortPassageSpoken;
    // The passage hint is only spoken the first time Affordance is played in the session.
    private bool _affordancePassageHintShown;
    private bool _sceneUpdatedLastFrame;
    private string _mapReloadErrorMessage;

    // Session telemetry (rooms, deaths, controls, intervention); set by Game1.
    public Func<SessionActivityRecorder> ActivityProvider { get; set; }
    private SessionActivityRecorder Activity => ActivityProvider?.Invoke();
    // Death counts already credited to the current room.
    private int _recordedPhaseDeaths;
    private int _recordedTutorialFalls;

    public PhaseState State => _phaseState;
    public bool IsInTutorial => _currentScenarioId == ScenarioCatalog.TutorialScenarioId;
    public bool ExitRequested { get; private set; }
    public bool ReturnToSelectionRequested { get; private set; }
    // Leave the phase from the pause or death menu without trying to complete it.
    public bool ReturnToMenuRequested { get; private set; }
    public bool HasOverlay => _overlayState != OverlayState.NONE;
    public double SecondsPerFrame => _scene.SecondsPerFrame;

    public bool CanComplete
    {
        get
        {
            PhaseDefinition phase = PhaseCatalog.GetForScenario(_currentScenarioId);
            // Room-format phases finish from their last room; the others from the problem room.
            return phase != null &&
                _currentScenarioId == (phase.UsesRoomFormat
                    ? phase.LastRoomScenarioId
                    : phase.ProblemScenarioId) &&
                _phaseState?.PhaseId == phase.Id &&
                _phaseState.ExitDoorIsOpen;
        }
    }

    public PhaseRun(
        GraphicsDevice graphicsDevice,
        ContentManager content,
        Vector2 logicalSize,
        PhaseProgress phaseProgress,
        SpriteFont font,
        SpriteFont titleFont,
        SpriteFont bodyFont,
        Texture2D selector,
        MortSubtitle mortSubtitle)
    {
        _graphicsDevice = graphicsDevice;
        _content = content;
        _logicalSize = logicalSize;
        _phaseProgress = phaseProgress;
        _mortSubtitle = mortSubtitle;
        Vector2 center = logicalSize / 2f;
        _camera = new Camera(center, logicalSize, 1.0f);
        _pauseMenu = new PauseMenu(titleFont, bodyFont, selector, center);
        _deathMenu = new DeathMenu(titleFont, bodyFont, selector, center);
        _interventionChoiceMenu = new InterventionChoiceMenu(font, titleFont, bodyFont);
        _controlSchemeMenu = new ControlSchemeMenu(titleFont, bodyFont, selector, center);
    }

    public void LoadContent(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
        _damageHud.LoadContent(_content);
        _interventionChoiceMenu.LoadContent(_graphicsDevice, _content);
    }

    public void ClearOverlay()
    {
        _overlayState = OverlayState.NONE;
    }

    public void Reset()
    {
        _mortSubtitle.Clear();
        _phaseState = null;
        _scene?.ReleaseLdtkTextures();
        _scene = null;
        _ldtkTextureCache.Dispose();
        _currentScenarioId = ScenarioCatalog.InitialScenarioId;
        _overlayState = OverlayState.NONE;
        _scenarioTransition.Reset();
        _mapReloadErrorMessage = null;
    }

    public void Dispose()
    {
        _scene?.ReleaseLdtkTextures();
        _ldtkTextureCache.Dispose();
        _scenarioTransition.Dispose();
    }

    public void DrawChoiceBlink(SpriteBatch spriteBatch, Texture2D fadeTexture)
    {
        _solutionBlink.Draw(spriteBatch, fadeTexture,
            GameViewport.LogicalWidth, GameViewport.LogicalHeight);
    }

    public void Update(
        KeyboardState keyboardState,
        KeyboardState previousKeyboardState,
        double deltaTime,
        bool isActive)
    {
        _previousKeyboardState = previousKeyboardState;
        RecordNewDeaths();
        ExitRequested = false;
        ReturnToSelectionRequested = false;
        ReturnToMenuRequested = false;
        bool sceneWasUpdatedLastFrame = _sceneUpdatedLastFrame;
        _sceneUpdatedLastFrame = false;
        if (_solutionBlink.Active)
        {
            if (isActive) _solutionBlink.Update(deltaTime);
            return;
        }
        if (_scenarioTransition.IsActive)
        {
            UpdateScenarioTransition(deltaTime);
            return;
        }

        // Wait until the room slide finishes so the blink reveals the live scene,
        // rather than a captured image of the room before the demonstration.
        // The combined demo only builds on the phase's best choice;
        // with any other choice it would replace the player's own solution.
        if (_currentScenarioId == _phaseState?.Definition.LastRoomScenarioId &&
            !_mortCombinationSpoken && isActive && AppliedBestSolution)
        {
            _mortCombinationSpoken = true;
            _mortSubtitle.Show(false, MortDialogueCatalog.Conclusion(_phaseState.AppliedSolutionId));
            _solutionBlink.Start(() =>
            {
                _phaseState.RegisterDemonstration();
                _scene.EnableCombinationDemo();
            });
            return;
        }

        if (ResearchStudyConfiguration.DevToolsEnabled &&
            keyboardState.IsKeyDown(Keys.LeftControl) &&
            KeyWasPressed(keyboardState, Keys.R) &&
            (_overlayState == OverlayState.NONE || _overlayState == OverlayState.PAUSED) &&
            _scene.SceneMode == SceneMode.PLAYING)
        {
            if (_scene.TryReloadMap(_camera, out string errorMessage))
            {
                _mapReloadErrorMessage = null;
            }
            else
            {
                _mapReloadErrorMessage = errorMessage;
            }

            return;
        }

        if (_overlayState == OverlayState.DEATH_MENU)
        {
            UpdateDeathMenu(keyboardState);
            return;
        }

        if (_overlayState == OverlayState.TUTORIAL_CONTROLS)
        {
            UpdateControlSchemeMenu(keyboardState);
            return;
        }

        if (_overlayState == OverlayState.INTERVENTION_CHOICES)
        {
            if (isActive)
            {
                _phaseState?.AccumulateActiveTime(deltaTime);
                Activity?.AddActiveTime(deltaTime);
            }

            _interventionChoiceMenu.Update(keyboardState, _previousKeyboardState);

            if (_interventionChoiceMenu.TryTakeCompletedSolution(
                out InterventionSolutionDefinition solution))
            {
                ApplyChoiceWithBlink(() =>
                {
                    _phaseState.ApplySolution(solution);
                    Activity?.RecordIntervention(_phaseState.PhaseId, "solucao", solution.Id);
                }, solution.Id);
            }
            else if (_interventionChoiceMenu.TryTakeExitJustification(
                out InterventionJustificationDefinition justification))
            {
                // The scene leaves through the exit on its next update.
                _phaseState.SetJustification(justification);
                Activity?.RecordIntervention(_phaseState.PhaseId, "justificativa", justification.Id);
                _overlayState = OverlayState.NONE;
            }
            else if (_interventionChoiceMenu.TryTakeContinueTesting())
            {
                _scene.CancelInterventionExit();
                Activity?.RecordIntervention(_phaseState.PhaseId, "continuar_testando", null);
                _overlayState = OverlayState.NONE;
            }

            return;
        }

        if (EscapeWasPressed(keyboardState))
        {
            TogglePause();
        }

        if (_overlayState == OverlayState.PAUSED)
        {
            UpdatePauseMenu(keyboardState);
            return;
        }

        if (isActive && _scene.SceneMode == SceneMode.PLAYING)
        {
            if (ResearchStudyConfiguration.DevToolsEnabled && _scene.SupportsCameraScalePreview)
            {
                if (KeyWasPressed(keyboardState, Keys.D1) || KeyWasPressed(keyboardState, Keys.NumPad1))
                    _scene.TrySetPlayerScale(1);
                else if (KeyWasPressed(keyboardState, Keys.D2) || KeyWasPressed(keyboardState, Keys.NumPad2))
                    _scene.TrySetPlayerScale(2);
                else if (KeyWasPressed(keyboardState, Keys.D3) || KeyWasPressed(keyboardState, Keys.NumPad3))
                    _scene.TrySetPlayerScale(3);
                else if (KeyWasPressed(keyboardState, Keys.D4) || KeyWasPressed(keyboardState, Keys.NumPad4))
                    _scene.TrySetPlayerScale(4);
            }

            _phaseState?.AccumulateActiveTime(deltaTime);
            Activity?.AddActiveTime(deltaTime);
            _mortSubtitle.Update(deltaTime);

            // Dropped if the player already justified (left the intervention room).
            if (_tryOthersCalloutPending && _phaseState?.HasJustification == true)
                _tryOthersCalloutPending = false;
            if (_tryOthersCalloutPending && !_mortSubtitle.IsShowing &&
                _scene.TryStartMortCallout(MortDialogueCatalog.TryOtherSolutions))
                _tryOthersCalloutPending = false;
        }

        _scene.SetInteractionPressed(KeyWasPressed(keyboardState, Keys.E));
        // After a menu, transition or new room, keys still held (e.g. Space used to confirm)
        // must not count as a fresh jump or dash.
        if (!sceneWasUpdatedLastFrame)
            _scene.ResyncPlayerInput();
        _sceneUpdatedLastFrame = true;
        _scene.Update(_camera, deltaTime);

        if (_scene.TryTakeTutorialExit())
        {
            _controlSchemeMenu.OpenExitConfirmation();
            _overlayState = OverlayState.TUTORIAL_CONTROLS;
            return;
        }

        if (_scene.TryTakeControlsSignActivation())
        {
            _controlSchemeMenu.OpenChoice();
            _overlayState = OverlayState.TUTORIAL_CONTROLS;
            return;
        }

        if (!_mortPassageSpoken && _scene.HasVisibleInterventionPassage)
        {
            _mortPassageSpoken = true;
            if (!_affordancePassageHintShown &&
                _phaseState?.PhaseId == ScenarioCatalog.AffordanceVisibilityScenarioId)
            {
                _affordancePassageHintShown = true;
                _mortSubtitle.Show(false, MortDialogueCatalog.Passage);
            }
        }


        if (_scene.TryTakeInterventionButtonActivation())
        {
            if (_phaseState == null)
            {
                return;
            }

            // Room format: the sword tries solutions until the player leaves the intervention;
            // the justification is asked at the exit.
            if (!_phaseState.HasFirstChoice ||
                _phaseState.Definition.UsesRoomFormat && !_phaseState.ChoiceLocked)
            {
                _interventionChoiceMenu.OpenSolutions(
                    GetOrCreateInitialSolutions(),
                    _phaseState.ChoiceCatalog.SolutionQuestion);
                _overlayState = OverlayState.INTERVENTION_CHOICES;
                return;
            }
        }

        // E at the intervention exit: justification, or "keep testing" to stay.
        if (_scene.TryTakeInterventionExitRequest() && _phaseState != null)
        {
            _interventionChoiceMenu.OpenExitJustification(
                GetOrCreateInitialJustifications(),
                _phaseState.ChoiceCatalog.JustificationQuestion);
            _overlayState = OverlayState.INTERVENTION_CHOICES;
            return;
        }

        if (_scene.TryTakeScenarioTransition(
            out string destinationScenarioId,
            out string destinationSpawnId))
        {
            if (destinationScenarioId == ScenarioCatalog.InitialScenarioId)
            {
                ReturnToSelectionRequested = true;

                return;
            }

            BeginScenarioTransition(destinationScenarioId, destinationSpawnId);
            return;
        }

        if (_scene.PlayerIsDead)
        {
            _phaseState?.RegisterDeath();
            OpenDeathMenu();
        }
    }

    // Credits new deaths (phase deaths, tutorial falls) to the current room.
    private void RecordNewDeaths()
    {
        int phaseDeaths = _phaseState?.TotalDeathCount ?? 0;
        if (phaseDeaths > _recordedPhaseDeaths)
            Activity?.AddDeaths(phaseDeaths - _recordedPhaseDeaths);
        _recordedPhaseDeaths = phaseDeaths;

        int tutorialFalls = _scene?.TutorialFallCount ?? 0;
        if (tutorialFalls > _recordedTutorialFalls)
            Activity?.AddDeaths(tutorialFalls - _recordedTutorialFalls);
        _recordedTutorialFalls = tutorialFalls;
    }

    private bool AppliedBestSolution =>
        _phaseState?.Definition.BestSolutionId != null &&
        _phaseState.AppliedSolutionId == _phaseState.Definition.BestSolutionId;

    private void ApplyChoiceWithBlink(Action applyChoice, string solutionId)
    {
        _overlayState = OverlayState.NONE;
        void Apply()
        {
            applyChoice();
            _scene.SyncInterventionState();
            ShowMortChoiceComment(solutionId);
        }
        if (_phaseState?.Definition.UsesRoomFormat == true)
            _solutionBlink.Start(Apply);
        else
            Apply();
    }

    private void ShowMortChoiceComment(string solutionId)
    {
        if (_phaseState?.Definition.UsesRoomFormat == true)
        {
            _mortSubtitle.Show(false, MortDialogueCatalog.AfterChoice(solutionId));
            if (_phaseState.TryTakeTryOthersCallout())
                _tryOthersCalloutPending = true;
        }
    }

    private IReadOnlyList<InterventionSolutionDefinition> GetOrCreateInitialSolutions()
    {
        if (!_phaseState.HasInitialSolutionPresentation)
        {
            IReadOnlyList<PresentedSolutionDefinition> presentation =
                _phaseState.ChoiceCatalog.CreateInitialSolutionPresentation(
                    _choicePresentationRandom);
            _phaseState.RegisterInitialSolutionPresentation(presentation);
        }

        IReadOnlyList<PresentedSolutionDefinition> registeredPresentation =
            _phaseState.InitialSolutionPresentation;
        var solutionsByPosition =
            new InterventionSolutionDefinition[registeredPresentation.Count];

        foreach (PresentedSolutionDefinition entry in registeredPresentation)
        {
            solutionsByPosition[entry.Position - 1] =
                _phaseState.ChoiceCatalog.GetSolution(entry.SolutionId);
        }

        return Array.AsReadOnly(solutionsByPosition);
    }

    private IReadOnlyList<InterventionJustificationDefinition> GetOrCreateInitialJustifications()
    {
        if (!_phaseState.HasInitialJustificationPresentation)
        {
            IReadOnlyList<PresentedJustificationDefinition> presentation =
                _phaseState.ChoiceCatalog.CreateInitialJustificationPresentation(
                    _choicePresentationRandom);
            _phaseState.RegisterInitialJustificationPresentation(presentation);
        }

        IReadOnlyList<PresentedJustificationDefinition> registeredPresentation =
            _phaseState.InitialJustificationPresentation;
        var justificationsByPosition =
            new InterventionJustificationDefinition[registeredPresentation.Count];

        foreach (PresentedJustificationDefinition entry in registeredPresentation)
        {
            justificationsByPosition[entry.Position - 1] =
                _phaseState.ChoiceCatalog.GetJustification(entry.JustificationId);
        }

        return Array.AsReadOnly(justificationsByPosition);
    }

    private void BeginScenarioTransition(
        string destinationScenarioId,
        string destinationSpawnId)
    {
        PhaseDefinition phase = _phaseState?.Definition;
        int currentRoom = phase?.GetRoomIndex(_currentScenarioId) ?? -1;
        int destinationRoom = phase?.GetRoomIndex(destinationScenarioId) ?? -1;
        if (currentRoom >= 0 && destinationRoom >= 0)
        {
            // Rooms sit side by side: going to an earlier room slides left.
            int slideDirection = destinationRoom < currentRoom ? -1 : 1;
            _scenarioTransition.BeginSlide(
                _graphicsDevice,
                slideDirection,
                AffordanceBackground.GetRoomOffset(_currentScenarioId));
            CaptureRoom(_scenarioTransition.OutgoingRoom);
            ChangeScenario(destinationScenarioId, destinationSpawnId);
            CaptureRoom(_scenarioTransition.IncomingRoom);
            return;
        }
        _scenarioTransition.BeginFade(destinationScenarioId, destinationSpawnId);
    }

    private void CaptureRoom(RenderTarget2D target)
    {
        _graphicsDevice.SetRenderTarget(target);
        // Capture gameplay only; the continuous panorama is drawn behind both rooms.
        _graphicsDevice.Clear(Color.Transparent);
        _scene.Draw(_spriteBatch, _camera);
        _graphicsDevice.SetRenderTarget(null);
    }

    private void UpdateScenarioTransition(double deltaTime)
    {
        if (_scenarioTransition.Update(
            deltaTime,
            out string destinationScenarioId,
            out string destinationSpawnId))
        {
            ChangeScenario(destinationScenarioId, destinationSpawnId);
        }
    }

    public void ChangeScenario(string scenarioId, string spawnId)
    {
        using var loading = new LoadDiagnostics($"cenario {scenarioId}");
        // Deaths still uncounted belong to the room being left.
        RecordNewDeaths();
        PhaseDefinition destinationPhase =
            PhaseCatalog.GetForScenario(scenarioId);
        bool changesPhase = _phaseState?.PhaseId != destinationPhase?.Id;

        if (destinationPhase == null)
        {
            _mortSubtitle.Clear();
            _phaseState = null;
        }
        else if (_phaseState == null ||
                 _phaseState.PhaseId != destinationPhase.Id)
        {
            // Phase 3 uses its own fixed damage feedback; nothing is inherited.
            _phaseState = new PhaseState(
                destinationPhase,
                scenarioId);
            _mortCombinationSpoken = false;
            _mortPassageSpoken = false;
            _tryOthersCalloutPending = false;
            _mortSubtitle.Clear();
            string[] introduction = MortDialogueCatalog.GetIntroduction(destinationPhase.Id);
            if (introduction != null)
                _mortSubtitle.Show(false, introduction);
        }
        else
        {
            _phaseState.EnterScenario(scenarioId);
        }

        _currentScenarioId = scenarioId;

        loading.Mark("estado da fase");
        // Every room-format phase reuses the Affordance panorama.
        if (destinationPhase?.UsesRoomFormat == true)
            _affordanceBackground ??= new AffordanceBackground(_content);

        loading.Mark("cenario de fundo (criar ou reutilizar)", includeInSummary: true);
        ScenarioDefinition scenario = ScenarioCatalog.Get(scenarioId);
        _scene?.ReleaseLdtkTextures();
        if (changesPhase) _ldtkTextureCache.Dispose();
        loading.Mark("liberar tilesets anteriores");
        _scene = new Scene(scenario, spawnId, _phaseState);
        _scene.UseLdtkTextureCache(_ldtkTextureCache);
        _scene.LoadContent(_graphicsDevice, _content);
        loading.Mark("criar e carregar Scene");
        _recordedPhaseDeaths = _phaseState?.TotalDeathCount ?? 0;
        _recordedTutorialFalls = 0;
        Activity?.EnterRoom(_phaseState?.PhaseId, scenarioId);
        _scene.AlignCamera(_camera);
        if (scenarioId == _phaseState?.Definition.LastRoomScenarioId && _mortCombinationSpoken &&
            AppliedBestSolution)
            _scene.EnableCombinationDemo();
        _overlayState = OverlayState.NONE;
        _mapReloadErrorMessage = null;
    }

    private void UpdatePauseMenu(KeyboardState keyboardState)
    {
        PauseOption selectedOption = _pauseMenu.Update(keyboardState, _previousKeyboardState);

        if (selectedOption == PauseOption.RESUME)
            _overlayState = OverlayState.NONE;
        else if (selectedOption == PauseOption.RESTART)
        {
            RestartAttempt();
        }
        else if (selectedOption == PauseOption.MAIN_MENU)
            ReturnToMenuRequested = true;
        else if (selectedOption == PauseOption.QUIT)
            ExitRequested = true;
    }

    private void UpdateControlSchemeMenu(KeyboardState keyboardState)
    {
        switch (_controlSchemeMenu.Update(keyboardState, _previousKeyboardState))
        {
            case ControlSchemeMenuResult.CLOSED:
                Activity?.RecordControlScheme(TutorialControls.Current);
                _overlayState = OverlayState.NONE;
                break;
            case ControlSchemeMenuResult.CONTINUE:
                Activity?.RecordTutorialCompleted();
                _overlayState = OverlayState.NONE;
                ReturnToSelectionRequested = true;
                break;
            case ControlSchemeMenuResult.TEST_MORE:
                _scene.StepBackFromTutorialExit();
                _overlayState = OverlayState.NONE;
                break;
        }
    }

    private void UpdateDeathMenu(KeyboardState keyboardState)
    {
        DeathOption selectedOption = _deathMenu.Update(keyboardState, _previousKeyboardState);

        if (selectedOption == DeathOption.RETRY)
        {
            RestartAttempt();
        }
        else if (selectedOption == DeathOption.MAIN_MENU)
            ReturnToMenuRequested = true;
        else if (selectedOption == DeathOption.QUIT)
            ExitRequested = true;
    }

    private void RestartAttempt()
    {
        _phaseState?.BeginAttempt();
        // Room format: a new attempt starts from room 1, or from room 2 once the choice is
        // made (justified), so the solution is not replayed through room 1.
        // The intervention room keeps restarting in place.
        // Debug keeps restarting in the current room to speed up testing.
        PhaseDefinition phase = _phaseState?.Definition;
        int restartRoom = _phaseState?.HasJustification == true ? 1 : 0;
        if (!ResearchStudyConfiguration.DevToolsEnabled &&
            phase != null && phase.UsesRoomFormat &&
            phase.RoomScenarioIds.Count > restartRoom &&
            phase.GetRoomIndex(_currentScenarioId) >= 0 &&
            phase.GetRoomIndex(_currentScenarioId) != restartRoom)
        {
            ChangeScenario(phase.RoomScenarioIds[restartRoom], null);
            return;
        }
        _scene.Restart(_camera);
        _overlayState = OverlayState.NONE;
    }

    private bool EscapeWasPressed(KeyboardState keyboardState)
    {
        return KeyWasPressed(keyboardState, Keys.Escape);
    }

    private bool KeyWasPressed(KeyboardState keyboardState, Keys key)
    {
        return keyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }

    private void TogglePause()
    {
        if (_overlayState == OverlayState.PAUSED)
        {
            _overlayState = OverlayState.NONE;
        }
        else
        {
            _overlayState = OverlayState.PAUSED;
            _pauseMenu.Open();
        }
    }

    private void OpenDeathMenu()
    {
        _overlayState = OverlayState.DEATH_MENU;
        _deathMenu.Open();
    }

    public void Draw(DebugView debugView, Texture2D fadeTexture)
    {
        if (_scenarioTransition.IsSliding)
        {
            float backgroundOffset = _scenarioTransition.GetSlideBackgroundOffset(
                AffordanceBackground.GetRoomOffset(_currentScenarioId));
            _affordanceBackground.DrawBackground(_spriteBatch, backgroundOffset);
            _scenarioTransition.DrawSlide(_spriteBatch);
            _affordanceBackground.DrawForeground(_spriteBatch, backgroundOffset);
        }
        else
        {
            bool hasAffordanceBackground = PhaseCatalog.IsChallengeRoom(_currentScenarioId);
            if (hasAffordanceBackground)
            {
                _affordanceBackground.DrawBackground(_spriteBatch,
                    AffordanceBackground.GetRoomOffset(_currentScenarioId));
            }
            _scene.Draw(_spriteBatch, _camera);
            if (hasAffordanceBackground)
                _affordanceBackground.DrawForeground(_spriteBatch,
                    AffordanceBackground.GetRoomOffset(_currentScenarioId));
            _scene.DrawDebug(_spriteBatch, _camera, debugView);
            debugView.DrawHud(_spriteBatch, _currentScenarioId, _scene.Player);
        }

        if (_phaseState != null)
        {
            int maximumHealth = _phaseState.HasDamageSystem ? _phaseState.MaximumHealth : 1;
            int currentHealth = _scene.PlayerIsDead ? 0 :
                _phaseState.HasDamageSystem ? _phaseState.CurrentHealth : 1;
            _damageHud.Draw(_spriteBatch, currentHealth, maximumHealth);
        }

        if (_overlayState == OverlayState.PAUSED)
        {
            _pauseMenu.Draw(_spriteBatch);
        }
        else if (_overlayState == OverlayState.DEATH_MENU)
        {
            _deathMenu.Draw(_spriteBatch);
        }
        else if (_overlayState == OverlayState.INTERVENTION_CHOICES)
        {
            _interventionChoiceMenu.Draw(_spriteBatch);
        }
        else if (_overlayState == OverlayState.TUTORIAL_CONTROLS)
        {
            _controlSchemeMenu.Draw(_spriteBatch, fadeTexture);
        }

        if (_overlayState == OverlayState.NONE &&
            !_scenarioTransition.IsActive &&
            _scene.SceneMode == SceneMode.PLAYING && !_scene.PlayerIsDead)
            _mortSubtitle.Draw(_spriteBatch, fadeTexture,
                _logicalSize);

        _scenarioTransition.DrawFade(_spriteBatch, fadeTexture);
        debugView.DrawReloadError(_spriteBatch, _mapReloadErrorMessage);
    }
}
