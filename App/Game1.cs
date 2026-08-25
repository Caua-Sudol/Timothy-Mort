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

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private RenderTarget2D _gameRenderTarget;
    private Camera _camera;
    private DebugView _debugView;
    private Scene _scene;
    private StartMenu _startMenu;
    private PauseMenu _pauseMenu;
    private DeathMenu _deathMenu;
    private DamageHud _damageHud;
    private InterventionChoiceMenu _interventionChoiceMenu;
    private PhaseState _phaseState;
    private string _currentScenarioId = ScenarioCatalog.InitialScenarioId;
    private readonly Random _choicePresentationRandom = new Random();
    private AppScreen _activeScreen = AppScreen.START_MENU;
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
        FADING_IN
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
        _scene = new Scene(ScenarioCatalog.Initial, null, _phaseState);

        SpriteFont font = Content.Load<SpriteFont>("font");
        _debugView = new DebugView(font);
        Vector2 menuCenter = new Vector2(LogicalWidth / 2, LogicalHeight / 2);

        _startMenu = new StartMenu(font, menuCenter);
        _pauseMenu = new PauseMenu(font, menuCenter);
        _deathMenu = new DeathMenu(font, menuCenter);
        _damageHud = new DamageHud();
        _interventionChoiceMenu = new InterventionChoiceMenu(font);

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
        _damageHud.LoadContent(GraphicsDevice);
        _interventionChoiceMenu.LoadContent(GraphicsDevice);
        _debugView.LoadContent(GraphicsDevice);
        _scene.LoadContent(GraphicsDevice, Content);

        TargetElapsedTime = TimeSpan.FromSeconds(SecondsPerFrameMenu);
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

        if(_activeScreen == AppScreen.PLAYING)
        {
            UpdatePlaying(keyboardState, deltaTime);
        }
        if(_activeScreen == AppScreen.START_MENU)
        {
            UpdateStartMenu(keyboardState);
        }

        _previousKeyboardState = keyboardState;
        
        TargetElapsedTime = TimeSpan.FromSeconds(GetSecondsPerFrame());

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_gameRenderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        if(_activeScreen == AppScreen.PLAYING)
        {
            DrawPlaying();
        }
        if(_activeScreen == AppScreen.START_MENU)
        {
            DrawStartMenu();
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
        StartMenuOption selectedOption = _startMenu.Update(keyboardState);
        
        if(selectedOption == StartMenuOption.START)
        {
            _activeScreen = AppScreen.PLAYING;
            _overlayState = OverlayState.NONE;
        }
        else if(selectedOption == StartMenuOption.EXIT)
        {
            Exit();
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
        _pendingScenarioId = destinationScenarioId;
        _pendingScenarioSpawnId = destinationSpawnId;
        _scenarioFadeAlpha = 0.0f;
        _scenarioTransitionState = ScenarioTransitionState.FADING_OUT;
    }

    private void UpdateScenarioTransition(double deltaTime)
    {
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
            _phaseState = new PhaseState(destinationPhase, scenarioId);
        }
        else
        {
            _phaseState.EnterScenario(scenarioId);
        }

        _currentScenarioId = scenarioId;

        ScenarioDefinition scenario = ScenarioCatalog.Get(scenarioId);
        _scene = new Scene(scenario, spawnId, _phaseState);
        _scene.LoadContent(GraphicsDevice, Content);
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

    private void DrawPlaying()
    {
        _scene.Draw(_spriteBatch, _camera);
        _scene.DrawDebug(_spriteBatch, _camera, _debugView);
        _debugView.DrawHud(_spriteBatch, _currentScenarioId, _scene.Player);

        if (_phaseState?.HasDamageSystem == true)
        {
            _damageHud.Draw(
                _spriteBatch,
                _phaseState.CurrentHealth,
                _phaseState.MaximumHealth,
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
        if (_scenarioTransitionState == ScenarioTransitionState.NONE)
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
