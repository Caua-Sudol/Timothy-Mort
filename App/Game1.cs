using System;
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

        if (keyboardState.IsKeyDown(Keys.LeftControl) && KeyWasPressed(keyboardState, Keys.P))
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

        if (phase != null)
        {
            var introduction = PhaseIntroductionCatalog.Get(phase.Id);
            _phaseIntroductionScreen.Open(introduction.Title, introduction.Text);
            _introductionScenarioId = scenarioId;
            _activeScreen = AppScreen.PHASE_INTRODUCTION;
            return;
        }

        _phaseRun.ChangeScenario(scenarioId, null);
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
            _phaseRun.ChangeScenario(_introductionScenarioId, null);
            _introductionScenarioId = null;
            _activeScreen = AppScreen.PLAYING;
        }
    }

    private void UpdatePhaseResult(KeyboardState keyboardState, double deltaTime)
    {
        if (KeyWasPressed(keyboardState, Keys.Enter) || EscapeWasPressed(keyboardState))
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
            _studySession.Reset();
            _phaseSelectMenu.SetSessionCode(null);
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
                _studySession.CompletionData.SessionId,
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

        if (_phaseRun.ReturnToSelectionRequested && !CompleteCurrentPhase())
            ReturnToPhaseSelect();
    }

    private bool CompleteCurrentPhase()
    {
        if (_phaseRun.CanComplete)
        {
            string persistenceMessage;
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
                    persistenceMessage = "Progresso salvo localmente.";
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
