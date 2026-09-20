using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DontLikePoetry;

public class Game1 : Game
{
    private const int LogicalWidth = 1920;
    private const int LogicalHeight = 1080;
    private const int WindowMargin = 64;
    private const double SecondsPerFrameMenu = 1.0 / 60.0;
    private const float ScenarioFadeDurationSeconds = 0.75f;
    private const float RoomSlideDuration = 0.3f;
    private RenderTarget2D _slideOutgoing;
    private RenderTarget2D _slideIncoming;
    private float _roomSlideProgress;
    private int _roomSlideDirection = 1;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RenderTarget2D _gameRenderTarget;
    private Camera _camera;
    private DebugView _debugView;
    private Scene _scene;
    private TitleScreen _titleScreen;
    private StartMenu _startMenu;
    private SessionStartScreen _sessionStartScreen;
    private PhaseSelectMenu _phaseSelectMenu;
    private PhaseIntroductionScreen _phaseIntroductionScreen;
    private PhaseResultScreen _phaseResultScreen;
    private StudyCompletionScreen _studyCompletionScreen;
    private string _introductionScenarioId;
    private PauseMenu _pauseMenu;
    private DeathMenu _deathMenu;
    private DamageHud _damageHud;
    private InterventionChoiceMenu _interventionChoiceMenu;
    private PhaseState _phaseState;
    private readonly PhaseProgress _phaseProgress = new PhaseProgress();
    private SessionStore _sessionStore;
    private LocalSessionInfo _pendingActiveSession;
    private string _activeSessionId;
    private string _activeSessionCode;
    private SessionCompletionData _completedSessionData;
    private bool _showStudyCompletionAfterPhaseResult;
    private string _currentScenarioId = ScenarioCatalog.InitialScenarioId;
    private readonly Random _choicePresentationRandom = new Random();
    private AppScreen _activeScreen = AppScreen.SESSION_START;
    private OverlayState _overlayState = OverlayState.NONE;
    private KeyboardState _previousKeyboardState;
    private Texture2D _scenarioFadeTexture;
private float _scenarioFadeAlpha;
private string _pendingScenarioId;
private string _pendingScenarioSpawnId;
private string _mapReloadErrorMessage;
private ScenarioTransitionState _scenarioTransitionState = ScenarioTransitionState.NONE;

    private enum ScenarioTransitionState
    {
        NONE,
        FADING_OUT,
        FADING_IN,
        SLIDING
    }

    public Game1()
    {
        Point initialWindowSize = GetInitialWindowSize();

        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = initialWindowSize.X;
        _graphics.PreferredBackBufferHeight = initialWindowSize.Y;
        _graphics.ApplyChanges();

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
    }

    protected override void Initialize()
    {
        SpriteFont font = Content.Load<SpriteFont>("font");
        SpriteFont titleFont = Content.Load<SpriteFont>("walter");
        SpriteFont bodyFont = Content.Load<SpriteFont>("patrick");
        Texture2D selector = Content.Load<Texture2D>("ui/selector");
        Texture2D symbols = Content.Load<Texture2D>("ui/icons/icons");
        // Recolor only the in-memory rain atlas, preserving its alpha and the source PNG.
        // White premultiplied RGB lets SymbolRain apply a light tint even to dark artwork.
        var symbolPixels = new Color[symbols.Width * symbols.Height];
        symbols.GetData(symbolPixels);
        for (int index = 0; index < symbolPixels.Length; index++)
        {
            byte alpha = symbolPixels[index].A;
            symbolPixels[index] = new Color(alpha, alpha, alpha, alpha);
        }
        symbols.SetData(symbolPixels);
        _debugView = new DebugView(font);
        Vector2 menuCenter = new Vector2(LogicalWidth / 2, LogicalHeight / 2);

        _titleScreen = new TitleScreen(
            Content.Load<SpriteFont>("patrick"),
            Content.Load<Texture2D>("ui/sc_press_any_key"),
            menuCenter, symbols);
        _startMenu = new StartMenu(font, menuCenter);
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
            titleFont, bodyFont, new Vector2(LogicalWidth, LogicalHeight), symbols);
        _phaseResultScreen = new PhaseResultScreen(
            titleFont, bodyFont, new Vector2(LogicalWidth, LogicalHeight), symbols);
        _studyCompletionScreen = new StudyCompletionScreen(
            titleFont, bodyFont, new Vector2(LogicalWidth, LogicalHeight), symbols);
        _pauseMenu = new PauseMenu(titleFont, bodyFont, selector, menuCenter);
        _deathMenu = new DeathMenu(titleFont, bodyFont, selector, menuCenter);
        _damageHud = new DamageHud();
        _interventionChoiceMenu = new InterventionChoiceMenu(font, titleFont, bodyFont);

        Vector2 cameraPosition = new Vector2(LogicalWidth / 2, LogicalHeight / 2);
        Vector2 cameraDimensions = new Vector2(LogicalWidth, LogicalHeight);

        _camera = new Camera(cameraPosition, cameraDimensions, 1.0f);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _gameRenderTarget = new RenderTarget2D(GraphicsDevice, LogicalWidth, LogicalHeight);
        _scenarioFadeTexture = new Texture2D(GraphicsDevice, 1, 1);
        _scenarioFadeTexture.SetData(new[] { Color.White });
        _damageHud.LoadContent(Content);
        _interventionChoiceMenu.LoadContent(GraphicsDevice, Content);
        _phaseSelectMenu.LoadContent(Content);
        _debugView.LoadContent(GraphicsDevice);

        TargetElapsedTime = TimeSpan.FromSeconds(SecondsPerFrameMenu);
        OpenSessionStartFlow();
    }

    protected override void Update(GameTime gameTime)
    {
        double deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.LeftControl) && KeyWasPressed(keyboardState, Keys.P))
        {
            _debugView.Toggle();
        }

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            Exit();

        if (_activeScreen == AppScreen.TITLE)
        {
            _titleScreen.UpdateAnimation(deltaTime);
            UpdateTitleScreen(keyboardState);
        }
        else if (_activeScreen == AppScreen.START_MENU)
        {
            UpdateStartMenu(keyboardState);
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

        _previousKeyboardState = keyboardState;
        
        TargetElapsedTime = TimeSpan.FromSeconds(GetSecondsPerFrame());

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_gameRenderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        if (_activeScreen == AppScreen.TITLE)
        {
            DrawTitleScreen();
        }
        else if (_activeScreen == AppScreen.START_MENU)
        {
            DrawStartMenu();
        }
        else if (_activeScreen == AppScreen.SESSION_START)
        {
            _sessionStartScreen.Draw(_spriteBatch);
        }
        else if (_activeScreen == AppScreen.PHASE_SELECT)
        {
            DrawPhaseSelect();
        }
        else if (_activeScreen == AppScreen.PHASE_INTRODUCTION)
        {
            _phaseIntroductionScreen.Draw(_spriteBatch, _scenarioFadeTexture);
        }
        else if (_activeScreen == AppScreen.PHASE_RESULT)
        {
            _phaseResultScreen.Draw(_spriteBatch, _scenarioFadeTexture);
        }
        else if (_activeScreen == AppScreen.STUDY_COMPLETION)
        {
            _studyCompletionScreen.Draw(_spriteBatch, _scenarioFadeTexture);
        }
        else if (_activeScreen == AppScreen.PLAYING)
        {
            DrawPlaying();
        }

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_gameRenderTarget, GetPresentationRectangle(), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private Point GetInitialWindowSize()
    {
        DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        int availableWidth = displayMode.Width - WindowMargin;
        int availableHeight = displayMode.Height - WindowMargin;
        float widthScale = availableWidth / (float)LogicalWidth;
        float heightScale = availableHeight / (float)LogicalHeight;
        float scale = Math.Min(1.0f, Math.Min(widthScale, heightScale));

        return new Point((int)(LogicalWidth * scale), (int)(LogicalHeight * scale));
    }

    private Rectangle GetPresentationRectangle()
    {
        Viewport viewport = GraphicsDevice.Viewport;
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
        int width = Window.ClientBounds.Width;
        int height = Window.ClientBounds.Height;

        if (width <= 0 || height <= 0 ||
            _graphics.PreferredBackBufferWidth == width && _graphics.PreferredBackBufferHeight == height)
        {
            return;
        }

        _graphics.PreferredBackBufferWidth = width;
        _graphics.PreferredBackBufferHeight = height;
        _graphics.ApplyChanges();
    }

    private void UpdateStartMenu(KeyboardState keyboardState)
    {
        StartMenuOption selectedOption =
            _startMenu.Update(keyboardState, _previousKeyboardState);
        
        if(selectedOption == StartMenuOption.START)
        {
            _overlayState = OverlayState.NONE;
            _activeScreen = AppScreen.PHASE_SELECT;
        }
        else if(selectedOption == StartMenuOption.EXIT)
        {
            Exit();
        }
    }

    private void OpenSessionStartFlow()
    {
        try
        {
            _sessionStore ??= new SessionStore();
            _pendingActiveSession =
                _sessionStore.FindMostRecentActiveSession();

            if (_pendingActiveSession == null)
            {
                ActivateSession(_sessionStore.CreateNewSession(_phaseProgress));
                return;
            }

            _sessionStartScreen.Open(_pendingActiveSession.PublicCode);
            _activeScreen = AppScreen.SESSION_START;
        }
        catch (Exception)
        {
            _pendingActiveSession = null;
            _sessionStartScreen.Open(null);
            _sessionStartScreen.SetError(
                "Não foi possível acessar as sessões locais.");
            _activeScreen = AppScreen.SESSION_START;
        }
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

        try
        {
            if (action == SessionStartAction.CONTINUE)
            {
                _sessionStore.ContinueSession(
                    _pendingActiveSession.SessionId,
                    _phaseProgress);
                ActivateSession(_pendingActiveSession);
            }
            else if (action == SessionStartAction.NEW_SESSION)
            {
                ActivateSession(_sessionStore.CreateNewSession(_phaseProgress));
            }
        }
        catch (Exception)
        {
            _sessionStartScreen.SetError(
                "Não foi possível preparar a sessão local.");
        }
    }

    private void ActivateSession(LocalSessionInfo session)
    {
        _activeSessionId = session.SessionId;
        _activeSessionCode = session.PublicCode;
        _completedSessionData = null;
        _showStudyCompletionAfterPhaseResult = false;
        _pendingActiveSession = null;
        _phaseSelectMenu.SetSessionCode(_activeSessionCode);
        _activeScreen = AppScreen.TITLE;
        _overlayState = OverlayState.NONE;
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

        if (phase != null)
        {
            var introduction = PhaseIntroductionCatalog.Get(phase.Id);
            _phaseIntroductionScreen.Open(introduction.Title, introduction.Text);
            _introductionScenarioId = scenarioId;
            _activeScreen = AppScreen.PHASE_INTRODUCTION;
            return;
        }

        ChangeScenario(scenarioId, null);
        _activeScreen = AppScreen.PLAYING;
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

        if (KeyWasPressed(keyboardState, Keys.Enter) && _introductionScenarioId != null)
        {
            ChangeScenario(_introductionScenarioId, null);
            _introductionScenarioId = null;
            _activeScreen = AppScreen.PLAYING;
        }
    }

    private void UpdatePhaseResult(KeyboardState keyboardState, double deltaTime)
    {
        if (KeyWasPressed(keyboardState, Keys.Enter) || EscapeWasPressed(keyboardState))
        {
            if (_showStudyCompletionAfterPhaseResult)
            {
                ReturnToPhaseSelect();
                _activeScreen = AppScreen.STUDY_COMPLETION;
                _showStudyCompletionAfterPhaseResult = false;
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
            _phaseProgress.Clear();
            _phaseSelectMenu.SetSessionCode(null);
            _activeSessionId = null;
            _activeSessionCode = null;
            _completedSessionData = null;
            OpenSessionStartFlow();
            return;
        }

        if (action == StudyCompletionAction.RETRY_EXPORT)
        {
            TryRefreshSessionExport();
            return;
        }

        if (action == StudyCompletionAction.OPEN_FORM)
        {
            if (ResearchStudyConfiguration.TryOpenMeegaForm(
                _completedSessionData.SessionId,
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
        try
        {
            _completedSessionData =
                _sessionStore.LoadSessionCompletion(_activeSessionId);
            string exportPath = _sessionStore.ExportSession(_activeSessionId);
            _studyCompletionScreen.Open(
                _completedSessionData,
                exportPath,
                null);
        }
        catch (Exception)
        {
            _completedSessionData ??= new SessionCompletionData
            {
                SessionId = _activeSessionId,
                PublicCode = _activeSessionCode,
                Status = "completed"
            };
            _studyCompletionScreen.Open(
                _completedSessionData,
                null,
                "Não foi possível exportar os dados locais.");
        }

        _showStudyCompletionAfterPhaseResult = true;
    }

    private void TryRefreshSessionExport()
    {
        try
        {
            _completedSessionData =
                _sessionStore.LoadSessionCompletion(_activeSessionId);
            string exportPath = _sessionStore.ExportSession(_activeSessionId);
            _studyCompletionScreen.Open(
                _completedSessionData,
                exportPath,
                null);
        }
        catch (Exception)
        {
            _studyCompletionScreen.SetExportResult(
                null,
                "Não foi possível exportar os dados locais.");
        }
    }

    private double GetSecondsPerFrame()
    {
        if (_activeScreen == AppScreen.PLAYING && _overlayState == OverlayState.NONE)
        {
            return _scene.SecondsPerFrame;
        }

        return SecondsPerFrameMenu;
    }

    private void UpdatePlaying(KeyboardState keyboardState, double deltaTime)
    {
        if (_scenarioTransitionState != ScenarioTransitionState.NONE)
        {
            UpdateScenarioTransition(deltaTime);
            return;
        }

    if (keyboardState.IsKeyDown(Keys.LeftControl) &&
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

        if (_overlayState == OverlayState.INTERVENTION_CHOICES)
        {
            if (IsActive)
            {
                _phaseState?.AccumulateActiveTime(deltaTime);
            }

            _interventionChoiceMenu.Update(keyboardState, _previousKeyboardState);

            if (_interventionChoiceMenu.TryTakeCompletedFirstChoice(
                out InterventionSolutionDefinition solution,
                out InterventionJustificationDefinition justification))
            {
                _phaseState.ApplyFirstChoice(solution, justification);
                _scene.SyncInterventionState();
                _overlayState = OverlayState.NONE;
            }
            else if (_interventionChoiceMenu.TryTakeCompletedRevision(
                out InterventionSolutionDefinition revisionSolution))
            {
                _phaseState.ApplyRevision(revisionSolution);
                _scene.SyncInterventionState();
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

        if (IsActive && _scene.SceneMode == SceneMode.PLAYING)
        {
            _phaseState?.AccumulateActiveTime(deltaTime);
        }

        _scene.SetInteractionPressed(KeyWasPressed(keyboardState, Keys.E));
        _scene.Update(_camera, deltaTime);

        if (_scene.TryTakeInterventionButtonActivation())
        {
            if (_phaseState == null)
            {
                return;
            }

            if (!_phaseState.HasFirstChoice)
            {
                _interventionChoiceMenu.OpenInitial(
                    GetOrCreateInitialSolutions(),
                    GetOrCreateInitialJustifications(),
                    _phaseState.ChoiceCatalog.JustificationQuestion);
                _overlayState = OverlayState.INTERVENTION_CHOICES;
                return;
            }

            if (_phaseState.NeedsSolutionRevision)
            {
            _interventionChoiceMenu.OpenRevision(
                _phaseState.ChoiceCatalog.AvailableRevisionSolutions);
                _overlayState = OverlayState.INTERVENTION_CHOICES;
                return;
            }
        }

        if (_scene.TryTakeScenarioTransition(
            out string destinationScenarioId,
            out string destinationSpawnId))
        {
            if (destinationScenarioId == ScenarioCatalog.InitialScenarioId)
            {
                if (!CompleteCurrentPhase())
                {
                    ReturnToPhaseSelect();
                }

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
        if (ScenarioCatalog.IsAffordanceChallenge(_currentScenarioId) &&
            ScenarioCatalog.IsAffordanceChallenge(destinationScenarioId))
        {
            _slideOutgoing ??= new RenderTarget2D(GraphicsDevice, LogicalWidth, LogicalHeight);
            _slideIncoming ??= new RenderTarget2D(GraphicsDevice, LogicalWidth, LogicalHeight);
            _roomSlideDirection = destinationScenarioId == ScenarioCatalog.AffordanceVisibilityScenarioId ||
                _currentScenarioId == ScenarioCatalog.AffordanceRoom3ScenarioId &&
                destinationScenarioId == ScenarioCatalog.AffordanceRoom2ScenarioId ? -1 : 1;
            CaptureRoom(_slideOutgoing);
            ChangeScenario(destinationScenarioId, destinationSpawnId);
            CaptureRoom(_slideIncoming);
            _roomSlideProgress = 0;
            _scenarioFadeAlpha = 0;
            _scenarioTransitionState = ScenarioTransitionState.SLIDING;
            return;
        }
        _pendingScenarioId = destinationScenarioId;
        _pendingScenarioSpawnId = destinationSpawnId;
        _scenarioFadeAlpha = 0.0f;
        _scenarioTransitionState = ScenarioTransitionState.FADING_OUT;
    }

    private void CaptureRoom(RenderTarget2D target)
    {
        GraphicsDevice.SetRenderTarget(target);
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _scene.Draw(_spriteBatch, _camera);
        GraphicsDevice.SetRenderTarget(null);
    }

    protected override void UnloadContent()
    {
        _slideOutgoing?.Dispose();
        _slideIncoming?.Dispose();
        base.UnloadContent();
    }

    private void UpdateScenarioTransition(double deltaTime)
    {
        if (_scenarioTransitionState == ScenarioTransitionState.SLIDING)
        {
            _roomSlideProgress = Math.Min(1, _roomSlideProgress + (float)deltaTime / RoomSlideDuration);
            if (_roomSlideProgress >= 1)
                _scenarioTransitionState = ScenarioTransitionState.NONE;
            return;
        }
        float fadeDelta = (float)(deltaTime / ScenarioFadeDurationSeconds);

        if (_scenarioTransitionState == ScenarioTransitionState.FADING_OUT)
        {
            _scenarioFadeAlpha = Math.Min(1.0f, _scenarioFadeAlpha + fadeDelta);

            if (_scenarioFadeAlpha >= 1.0f)
            {
                ChangeScenario(_pendingScenarioId, _pendingScenarioSpawnId);
                _pendingScenarioId = null;
                _pendingScenarioSpawnId = null;
                _scenarioTransitionState = ScenarioTransitionState.FADING_IN;
            }

            return;
        }

        _scenarioFadeAlpha = Math.Max(0.0f, _scenarioFadeAlpha - fadeDelta);

        if (_scenarioFadeAlpha <= 0.0f)
        {
            _scenarioTransitionState = ScenarioTransitionState.NONE;
        }
    }

    private void ChangeScenario(string scenarioId, string spawnId)
    {
        PhaseDefinition destinationPhase =
            PhaseCatalog.GetForScenario(scenarioId);

        if (destinationPhase == null)
        {
            _phaseState = null;
        }
        else if (_phaseState == null ||
                 _phaseState.PhaseId != destinationPhase.Id)
        {
            SolutionVisualModule? baseVisualModulesOverride =
                destinationPhase.Id ==
                    ScenarioCatalog.ConceptualModelsConstraintsScenarioId
                    ? _phaseProgress.GetTransferredVisualModules()
                    : null;

            _phaseState = new PhaseState(
                destinationPhase,
                scenarioId,
                baseVisualModulesOverride);
        }
        else
        {
            _phaseState.EnterScenario(scenarioId);
        }

        _currentScenarioId = scenarioId;

        ScenarioDefinition scenario = ScenarioCatalog.Get(scenarioId);
        _scene = new Scene(scenario, spawnId, _phaseState);
        _scene.LoadContent(GraphicsDevice, Content);
        _scene.AlignCamera(_camera);
        _overlayState = OverlayState.NONE;
        _mapReloadErrorMessage = null;
    }

    private bool CompleteCurrentPhase()
    {
        PhaseDefinition phase = PhaseCatalog.GetForScenario(_currentScenarioId);
        if (phase != null &&
            (_currentScenarioId == ScenarioCatalog.AffordanceRoom3ScenarioId ||
             phase.Id != ScenarioCatalog.AffordanceVisibilityScenarioId && _currentScenarioId == phase.ProblemScenarioId) &&
            _phaseState?.PhaseId == phase.Id &&
            _phaseState.ExitDoorIsOpen)
        {
            SolutionVisualModule finalSolutionVisualModules =
                string.IsNullOrEmpty(_phaseState.AppliedSolutionId)
                    ? SolutionVisualModule.NONE
                    : _phaseState.ChoiceCatalog
                        .GetSolution(_phaseState.AppliedSolutionId)
                        .VisualModules;

            string persistenceMessage;
            try
            {
                bool completesStudy = phase.Id ==
                    ScenarioCatalog.ConceptualModelsConstraintsScenarioId;
                _sessionStore.SavePhaseCompletion(
                    _activeSessionId,
                    _phaseState,
                    completesStudy);
                _phaseProgress.TryCompletePhase(
                    phase.Id,
                    _phaseState.AppliedSolutionId,
                    finalSolutionVisualModules);

                if (completesStudy)
                {
                    PrepareStudyCompletion();
                    persistenceMessage =
                        "Sessão e telemetria salvas localmente.";
                }
                else
                {
                    persistenceMessage = "Progresso salvo localmente.";
                }
            }
            catch (Exception)
            {
                _showStudyCompletionAfterPhaseResult = false;
                persistenceMessage =
                    "Não foi possível salvar o progresso local.";
            }

            _phaseResultScreen.Open(_phaseState, persistenceMessage);
            _activeScreen = AppScreen.PHASE_RESULT;
            return true;
        }

        return false;
    }

    private void ReturnToPhaseSelect()
    {
        _phaseState = null;
        _scene = null;
        _currentScenarioId = ScenarioCatalog.InitialScenarioId;
        _overlayState = OverlayState.NONE;
        _scenarioTransitionState = ScenarioTransitionState.NONE;
        _scenarioFadeAlpha = 0.0f;
        _pendingScenarioId = null;
        _pendingScenarioSpawnId = null;
        _mapReloadErrorMessage = null;
        _activeScreen = AppScreen.PHASE_SELECT;
    }

    private void UpdatePauseMenu(KeyboardState keyboardState)
    {
        PauseOption selectedOption = _pauseMenu.Update(keyboardState, _previousKeyboardState);

        if (selectedOption == PauseOption.RESUME)
            _overlayState = OverlayState.NONE;
        else if (selectedOption == PauseOption.RESTART)
        {
            _phaseState?.BeginAttempt();
            _scene.Restart(_camera);
            _overlayState = OverlayState.NONE;
        }
        else if (selectedOption == PauseOption.QUIT)
            Exit();
    }

    private void UpdateDeathMenu(KeyboardState keyboardState)
    {
        DeathOption selectedOption = _deathMenu.Update(keyboardState, _previousKeyboardState);

        if (selectedOption == DeathOption.RETRY)
        {
            _phaseState?.BeginAttempt();
            _scene.Restart(_camera);
            _overlayState = OverlayState.NONE;
        }
        else if (selectedOption == DeathOption.QUIT)
            Exit();
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

    private void DrawStartMenu()
    {
        _startMenu.Draw(_spriteBatch);
    }

    private void DrawTitleScreen()
    {
        _titleScreen.Draw(_spriteBatch);
    }

    private void DrawPhaseSelect()
    {
        _phaseSelectMenu.Draw(_spriteBatch);
    }

    private void DrawPlaying()
    {
        if (_scenarioTransitionState == ScenarioTransitionState.SLIDING)
        {
            float progress = MathHelper.SmoothStep(0, 1, _roomSlideProgress);
            int offset = (int)(LogicalWidth * progress) * _roomSlideDirection;
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_slideOutgoing, new Rectangle(-offset, 0, LogicalWidth, LogicalHeight), Color.White);
            _spriteBatch.Draw(_slideIncoming, new Rectangle(LogicalWidth * _roomSlideDirection - offset, 0, LogicalWidth, LogicalHeight), Color.White);
            _spriteBatch.End();
        }
        else
        {
            _scene.Draw(_spriteBatch, _camera);
            _scene.DrawDebug(_spriteBatch, _camera, _debugView);
            _debugView.DrawHud(_spriteBatch, _currentScenarioId, _scene.Player);
        }

        if (_phaseState != null)
        {
            int maximumHealth = _phaseState.HasDamageSystem ? _phaseState.MaximumHealth : 1;
            int currentHealth = _scene.PlayerIsDead ? 0 :
                _phaseState.HasDamageSystem ? _phaseState.CurrentHealth : 1;
            _damageHud.Draw(
                _spriteBatch,
                currentHealth,
                maximumHealth,
                _phaseState.AppliedVisualModules);
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

    DrawScenarioTransition();
    _debugView.DrawReloadError(_spriteBatch, _mapReloadErrorMessage);
}

    private void DrawScenarioTransition()
    {
        if (_scenarioTransitionState == ScenarioTransitionState.NONE ||
            _scenarioTransitionState == ScenarioTransitionState.SLIDING)
        {
            return;
        }

        _spriteBatch.Begin();
        _spriteBatch.Draw(
            _scenarioFadeTexture,
            new Rectangle(0, 0, LogicalWidth, LogicalHeight),
            Color.Black * _scenarioFadeAlpha);
        _spriteBatch.End();
    }
}
