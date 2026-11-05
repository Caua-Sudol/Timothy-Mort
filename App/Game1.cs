using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public class Game1 : Game
{
    private const double SecondsPerFrameMenu = 1.0 / 60.0;

    private readonly GameViewport _viewport;
    private SpriteBatch _spriteBatch;
    private DebugView _debugView;
    private ScaleTestRoom _scaleTestRoom;
    private PhaseRun _phaseRun;
    private TitleScreen _titleScreen;
    private SessionStartScreen _sessionStartScreen;
    private PhaseSelectMenu _phaseSelectMenu;
    private PhaseIntroductionScreen _phaseIntroductionScreen;
    private PhaseResultScreen _phaseResultScreen;
    private StudyCompletionScreen _studyCompletionScreen;
    private string _introductionScenarioId;
    // The tutorial plays once per session, the first time Affordance is chosen.
    private bool _tutorialShown;
    private MortSubtitle _mortSubtitle;
    private readonly PhaseProgress _phaseProgress = new PhaseProgress();
    private readonly StudySessionFlow _studySession;
    private AppScreen _activeScreen = AppScreen.SESSION_START;
    private KeyboardState _previousKeyboardState;
    private Texture2D _scenarioFadeTexture;

    public Game1()
    {
        _viewport = new GameViewport(this);
        _studySession = new StudySessionFlow(_phaseProgress);
        Window.Title = "Timothy & Mort";

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        SpriteFont font = Content.Load<SpriteFont>("font");
        SpriteFont titleFont = Content.Load<SpriteFont>("walter");
        SpriteFont bodyFont = Content.Load<SpriteFont>("patrick");
        Texture2D selector = Content.Load<Texture2D>("ui/selector");
        Texture2D symbols = Content.Load<Texture2D>("ui/icons/icons");
        var symbolPixels = new Color[symbols.Width * symbols.Height];
        symbols.GetData(symbolPixels);
        for (int index = 0; index < symbolPixels.Length; index++)
        {
            byte alpha = symbolPixels[index].A;
            symbolPixels[index] = new Color(alpha, alpha, alpha, alpha);
        }
        symbols.SetData(symbolPixels);
        _debugView = new DebugView(font);
        _mortSubtitle = new MortSubtitle(bodyFont);
        Vector2 menuCenter = _viewport.LogicalSize / 2f;

        _titleScreen = new TitleScreen(
            Content.Load<SpriteFont>("patrick"),
            Content.Load<Texture2D>("ui/sc_press_any_key"),
            menuCenter, symbols);
        _sessionStartScreen = new SessionStartScreen(
            Content.Load<SpriteFont>("walter"),
            Content.Load<SpriteFont>("patrick"),
            Content.Load<Texture2D>("ui/selector"),
            menuCenter);
        _phaseSelectMenu = new PhaseSelectMenu(
            Content.Load<SpriteFont>("walter"),
            Content.Load<SpriteFont>("patrick"),
            menuCenter,
            _phaseProgress);
        _phaseIntroductionScreen = new PhaseIntroductionScreen(
            titleFont, bodyFont, _viewport.LogicalSize, symbols);
        _phaseResultScreen = new PhaseResultScreen(
            titleFont, bodyFont, _viewport.LogicalSize, symbols);
        _studyCompletionScreen = new StudyCompletionScreen(
            titleFont, bodyFont, _viewport.LogicalSize, symbols);
        _phaseRun = new PhaseRun(
            GraphicsDevice, Content, _viewport.LogicalSize, _phaseProgress,
            font, titleFont, bodyFont, selector, _mortSubtitle);
        // Telemetry of the active session (changes when a new session starts).
        _phaseRun.ActivityProvider = () => _studySession.Activity;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _viewport.CreateRenderTarget(GraphicsDevice);
        _scenarioFadeTexture = new Texture2D(GraphicsDevice, 1, 1);
        _scenarioFadeTexture.SetData(new[] { Color.White });
        _phaseRun.LoadContent(_spriteBatch);
        _phaseSelectMenu.LoadContent(Content);
        _debugView.LoadContent(GraphicsDevice);

        TargetElapsedTime = TimeSpan.FromSeconds(SecondsPerFrameMenu);
        OpenSessionStartFlow();
    }

    protected override void Update(GameTime gameTime)
    {
        double deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var keyboardState = Keyboard.GetState();
        UiPointer.Update(
            Mouse.GetState(),
            GameViewport.GetPresentationRectangle(
                new Viewport(GraphicsDevice.PresentationParameters.Bounds)),
            IsActive);

        if (ResearchStudyConfiguration.DevToolsEnabled &&
            keyboardState.IsKeyDown(Keys.LeftControl) && KeyWasPressed(keyboardState, Keys.P))
        {
            if (_activeScreen == AppScreen.SCALE_TEST) _scaleTestRoom.ToggleDebug();
            else _debugView.Toggle();
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            Exit();

        if (_activeScreen == AppScreen.TITLE)
        {
            _titleScreen.UpdateAnimation(deltaTime);
            UpdateTitleScreen(keyboardState);
        }
        else if (_activeScreen == AppScreen.SESSION_START)
        {
            UpdateSessionStart(keyboardState);
        }
        else if (_activeScreen == AppScreen.PHASE_SELECT)
        {
            _phaseSelectMenu.UpdateAnimation(deltaTime);
            UpdatePhaseSelect(keyboardState);
        }
        else if (_activeScreen == AppScreen.PHASE_INTRODUCTION)
        {
            UpdatePhaseIntroduction(keyboardState, deltaTime);
        }
        else if (_activeScreen == AppScreen.PHASE_RESULT)
        {
            UpdatePhaseResult(keyboardState, deltaTime);
        }
        else if (_activeScreen == AppScreen.STUDY_COMPLETION)
        {
            UpdateStudyCompletion(keyboardState, deltaTime);
        }
        else if (_activeScreen == AppScreen.PLAYING)
        {
            UpdatePlaying(keyboardState, deltaTime);
        }
        else if (_activeScreen == AppScreen.SCALE_TEST)
        {
            UpdateScaleTestRoom(keyboardState, deltaTime);
        }

        _previousKeyboardState = keyboardState;
        
        TargetElapsedTime = TimeSpan.FromSeconds(GetSecondsPerFrame());

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _viewport.BeginDraw(GraphicsDevice);

        if (_activeScreen == AppScreen.TITLE)
        {
            _titleScreen.Draw(_spriteBatch);
        }
        else if (_activeScreen == AppScreen.SESSION_START)
        {
            _sessionStartScreen.Draw(_spriteBatch);
        }
        else if (_activeScreen == AppScreen.PHASE_SELECT)
        {
            _phaseSelectMenu.Draw(_spriteBatch);
        }
        else if (_activeScreen == AppScreen.PHASE_INTRODUCTION)
        {
            _phaseIntroductionScreen.Draw(_spriteBatch, _scenarioFadeTexture);
        }
        else if (_activeScreen == AppScreen.PHASE_RESULT)
        {
            _phaseResultScreen.Draw(_spriteBatch, _scenarioFadeTexture);
            _mortSubtitle.Draw(_spriteBatch, _scenarioFadeTexture,
                _viewport.LogicalSize, onResult: true);
        }
        else if (_activeScreen == AppScreen.STUDY_COMPLETION)
        {
            _studyCompletionScreen.Draw(_spriteBatch, _scenarioFadeTexture);
        }
        else if (_activeScreen == AppScreen.PLAYING)
        {
            _phaseRun.Draw(_debugView, _scenarioFadeTexture);
        }
        else if (_activeScreen == AppScreen.SCALE_TEST)
        {
            _scaleTestRoom.Draw(_spriteBatch);
        }

        _phaseRun.DrawChoiceBlink(_spriteBatch, _scenarioFadeTexture);
        _viewport.Present(GraphicsDevice, _spriteBatch);

        base.Draw(gameTime);
    }

    private void OpenSessionStartFlow()
    {
        if (!_studySession.TryOpen(out bool activatedSession))
        {
            _sessionStartScreen.Open(null);
            _sessionStartScreen.SetError(
                "Não foi possível acessar as sessões locais.");
            _activeScreen = AppScreen.SESSION_START;
            return;
        }

        if (activatedSession)
        {
            ActivateSession();
            return;
        }

        _sessionStartScreen.Open(_studySession.PendingSession.PublicCode);
        _activeScreen = AppScreen.SESSION_START;
    }

    private void UpdateSessionStart(KeyboardState keyboardState)
    {
        SessionStartAction action = _sessionStartScreen.Update(
            keyboardState,
            _previousKeyboardState);

        if (action == SessionStartAction.BACK)
        {
            Exit();
            return;
        }

        if (action == SessionStartAction.NONE)
        {
            return;
        }

        bool activatedSession = action == SessionStartAction.CONTINUE
            ? _studySession.TryContinuePendingSession()
            : _studySession.TryStartNewSession();
        if (!activatedSession)
        {
            _sessionStartScreen.SetError(
                "Não foi possível preparar a sessão local.");
            return;
        }

        ActivateSession();
    }

    private void ActivateSession()
    {
        _phaseSelectMenu.SetSessionCode(_studySession.ActiveSession.PublicCode);
        _activeScreen = AppScreen.TITLE;
        _phaseRun.ClearOverlay();
    }

    private void UpdateTitleScreen(KeyboardState keyboardState)
    {
        if (_titleScreen.Update(keyboardState, _previousKeyboardState))
        {
            _activeScreen = AppScreen.PHASE_SELECT;
        }
    }

    private void UpdatePhaseSelect(KeyboardState keyboardState)
    {
        if (EscapeWasPressed(keyboardState))
        {
            _activeScreen = AppScreen.TITLE;
            return;
        }

        PhaseSelectOption selectedOption =
            _phaseSelectMenu.Update(keyboardState, _previousKeyboardState);

        if (selectedOption == PhaseSelectOption.SCALE_TEST)
        {
            _scaleTestRoom ??= new ScaleTestRoom(
                GraphicsDevice,
                Content,
                _viewport.LogicalSize);
            _activeScreen = AppScreen.SCALE_TEST;
            return;
        }

        string scenarioId = selectedOption switch
        {
            PhaseSelectOption.AFFORDANCE => ScenarioCatalog.AffordanceVisibilityScenarioId,
            PhaseSelectOption.FEEDBACK => ScenarioCatalog.DamageFeedbackScenarioId,
            PhaseSelectOption.CONCEPTUAL_MODELS => ScenarioCatalog.ConceptualModelsConstraintsScenarioId,
            PhaseSelectOption.PROP_VISUAL => ScenarioCatalog.PropVisualScenarioId,
            _ => null
        };

        if (scenarioId == null)
        {
            return;
        }

        PhaseDefinition phase = PhaseCatalog.GetForScenario(scenarioId);
        if (phase != null && !_phaseProgress.IsUnlocked(phase.Id))
        {
            return;
        }

        if (phase?.Id == ScenarioCatalog.AffordanceVisibilityScenarioId && !_tutorialShown)
        {
            _tutorialShown = true;
            _phaseRun.ChangeScenario(ScenarioCatalog.TutorialScenarioId, null);
            _activeScreen = AppScreen.PLAYING;
            return;
        }

        if (phase != null)
        {
            OpenPhaseIntroduction(phase.Id, scenarioId);
            return;
        }

        _phaseRun.ChangeScenario(scenarioId, null);
        _activeScreen = AppScreen.PLAYING;
    }

    private void OpenPhaseIntroduction(string phaseId, string scenarioId)
    {
        var introduction = PhaseIntroductionCatalog.Get(phaseId);
        _phaseIntroductionScreen.Open(introduction.Title, introduction.Text);
        _introductionScenarioId = scenarioId;
        _activeScreen = AppScreen.PHASE_INTRODUCTION;
    }

    private void UpdatePhaseIntroduction(KeyboardState keyboardState, double deltaTime)
    {
        if (EscapeWasPressed(keyboardState))
        {
            _introductionScenarioId = null;
            _activeScreen = AppScreen.PHASE_SELECT;
            return;
        }

        _phaseIntroductionScreen.Update(deltaTime);

        bool confirmed = MenuInput.Confirm(keyboardState, _previousKeyboardState) || UiPointer.LeftClicked;
        // The first confirm only finishes the typing text; the next one starts the phase.
        if (confirmed && _phaseIntroductionScreen.IsTyping)
        {
            _phaseIntroductionScreen.CompleteTyping();
            return;
        }

        if (confirmed && _introductionScenarioId != null)
        {
            _phaseRun.ChangeScenario(_introductionScenarioId, null);
            _introductionScenarioId = null;
            _activeScreen = AppScreen.PLAYING;
        }
    }

    private void UpdatePhaseResult(KeyboardState keyboardState, double deltaTime)
    {
        bool confirmed = MenuInput.Confirm(keyboardState, _previousKeyboardState) || UiPointer.LeftClicked;
        // The first confirm only finishes the typing text; Esc still leaves at once.
        if (confirmed && _phaseResultScreen.IsTyping)
        {
            _phaseResultScreen.CompleteTyping();
            return;
        }

        if (confirmed || EscapeWasPressed(keyboardState))
        {
            if (_studySession.ShowCompletionAfterPhaseResult)
            {
                ReturnToPhaseSelect();
                _activeScreen = AppScreen.STUDY_COMPLETION;
                _studySession.ShowCompletionAfterPhaseResult = false;
                return;
            }

            ReturnToPhaseSelect();
            return;
        }

        _phaseResultScreen.Update(deltaTime);
    }

    private void UpdateStudyCompletion(
        KeyboardState keyboardState,
        double deltaTime)
    {
        StudyCompletionAction action = _studyCompletionScreen.Update(
            keyboardState,
            _previousKeyboardState,
            deltaTime);

        if (action == StudyCompletionAction.RETURN_TO_MENU)
        {
            string previousExportFolder = string.IsNullOrEmpty(_studyCompletionScreen.ExportPath)
                ? null
                : Path.GetDirectoryName(_studyCompletionScreen.ExportPath);
            _studySession.Reset();
            _phaseSelectMenu.SetSessionCode(null);

            // A finished study goes straight to phase select with a fresh session.
            if (_studySession.TryStartNewSession())
            {
                ActivateSession();
                _phaseSelectMenu.ShowNewSessionNotice(previousExportFolder);
                _activeScreen = AppScreen.PHASE_SELECT;
                return;
            }

            OpenSessionStartFlow();
            return;
        }

        if (action == StudyCompletionAction.RETRY_EXPORT)
        {
            TryRefreshSessionExport();
            return;
        }

        if (action == StudyCompletionAction.OPEN_EXPORT_FOLDER)
        {
            _studyCompletionScreen.SetFolderStatus(
                ResearchStudyConfiguration.TryOpenExportFolder(
                    _studyCompletionScreen.ExportPath,
                    out string folderError)
                    ? "Pasta dos dados aberta."
                    : folderError);
            return;
        }

        if (action == StudyCompletionAction.OPEN_FORM)
        {
            // The visible code (TM-...) is sent, the same one shown on screen and in the file name.
            if (ResearchStudyConfiguration.TryOpenMeegaForm(
                _studySession.CompletionData.PublicCode,
                out string errorMessage))
            {
                _studyCompletionScreen.SetFormStatus(
                    "Questionário MEEGA+ aberto no navegador.");
            }
            else
            {
                _studyCompletionScreen.SetFormStatus(errorMessage);
            }
        }
    }

    private void PrepareStudyCompletion()
    {
        if (_studySession.TryPrepareCompletion(out string exportPath))
        {
            _studyCompletionScreen.Open(
                _studySession.CompletionData,
                exportPath,
                null);
        }
        else
        {
            _studyCompletionScreen.Open(
                _studySession.CompletionData,
                null,
                "Não foi possível exportar os dados locais.");
        }

        _studySession.ShowCompletionAfterPhaseResult = true;
    }

    private void TryRefreshSessionExport()
    {
        if (_studySession.TryRefreshCompletionExport(out string exportPath))
        {
            _studyCompletionScreen.Open(
                _studySession.CompletionData,
                exportPath,
                null);
        }
        else
        {
            _studyCompletionScreen.SetExportResult(
                null,
                "Não foi possível exportar os dados locais.");
        }
    }

    private double GetSecondsPerFrame()
    {
        if (_activeScreen == AppScreen.PLAYING && !_phaseRun.HasOverlay)
        {
            return _phaseRun.SecondsPerFrame;
        }

        return SecondsPerFrameMenu;
    }

    private void UpdateScaleTestRoom(KeyboardState keyboardState, double deltaTime)
    {
        if (!IsActive)
            return;

        if (EscapeWasPressed(keyboardState))
        {
            _activeScreen = AppScreen.PHASE_SELECT;
            return;
        }

        _scaleTestRoom.Update(keyboardState, _previousKeyboardState, deltaTime);
    }

    protected override void UnloadContent()
    {
        // Closing the game (menu or window) keeps the latest room times and deaths.
        _studySession.Activity?.Save();
        _phaseRun?.Dispose();
        base.UnloadContent();
    }

    private void UpdatePlaying(KeyboardState keyboardState, double deltaTime)
    {
        _phaseRun.Update(keyboardState, _previousKeyboardState, deltaTime, IsActive);
        if (_phaseRun.ExitRequested)
        {
            Exit();
            return;
        }

        if (_phaseRun.ReturnToMenuRequested)
        {
            _studySession.Activity?.Save();
            ReturnToPhaseSelect();
            return;
        }

        // Leaving the tutorial goes on to the Affordance introduction.
        if (_phaseRun.ReturnToSelectionRequested && _phaseRun.IsInTutorial)
        {
            OpenPhaseIntroduction(ScenarioCatalog.AffordanceVisibilityScenarioId,
                ScenarioCatalog.AffordanceVisibilityScenarioId);
            return;
        }

        if (_phaseRun.ReturnToSelectionRequested && !CompleteCurrentPhase())
            ReturnToPhaseSelect();
    }

    private bool CompleteCurrentPhase()
    {
        if (_phaseRun.CanComplete)
        {
            string persistenceMessage;
            // The study ends after the last phase: Conceptual models (or Affordance in beta mode).
            bool completesStudy = _phaseRun.State.PhaseId ==
                (ResearchStudyConfiguration.BetaAffordanceOnly
                    ? ScenarioCatalog.AffordanceVisibilityScenarioId
                    : ScenarioCatalog.ConceptualModelsConstraintsScenarioId);
            if (_studySession.TryRecordPhaseCompletion(
                _phaseRun.State,
                completesStudy))
            {
                if (completesStudy)
                {
                    PrepareStudyCompletion();
                    persistenceMessage =
                        "Sessão e telemetria salvas localmente.";
                }
                else
                {
                    // Partial export to the same file, so the data survives if the player stops early.
                    persistenceMessage = _studySession.TryExportProgress(out string progressPath)
                        ? $"Progresso salvo em: {progressPath}"
                        : "Progresso salvo localmente.";
                }
            }
            else
            {
                _studySession.ShowCompletionAfterPhaseResult = false;
                persistenceMessage =
                    "Não foi possível salvar o progresso local.";
            }

            _mortSubtitle.Clear();
            _phaseResultScreen.Open(_phaseRun.State, persistenceMessage);
            _activeScreen = AppScreen.PHASE_RESULT;
            return true;
        }

        return false;
    }

    private void ReturnToPhaseSelect()
    {
        _phaseRun.Reset();
        _activeScreen = AppScreen.PHASE_SELECT;
    }

    private bool EscapeWasPressed(KeyboardState keyboardState)
    {
        return KeyWasPressed(keyboardState, Keys.Escape);
    }

    private bool KeyWasPressed(KeyboardState keyboardState, Keys key)
    {
        return keyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }

}
